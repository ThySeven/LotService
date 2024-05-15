using LotService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using System.Text;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings()
.GetCurrentClassLogger();
logger.Debug("init main");

var builder = WebApplication.CreateBuilder(args);


var EndPoint = Environment.GetEnvironmentVariable("VAULT_IP");
var httpClientHandler = new HttpClientHandler();
httpClientHandler.ServerCertificateCustomValidationCallback =
(message, cert, chain, sslPolicyErrors) => { return true; };


IAuthMethodInfo authMethod =
new TokenAuthMethodInfo(Environment.GetEnvironmentVariable("VAULT_SECRET"));
// Initialize settings. You can also set proxies, custom delegates etc. here.
var vaultClientSettings = new VaultClientSettings(EndPoint, authMethod)
{
    Namespace = "",
    MyHttpClientProviderFunc = handler
    => new HttpClient(httpClientHandler)
    {
        BaseAddress = new Uri(EndPoint)
    }
};
IVaultClient vaultClient = new VaultClient(vaultClientSettings);
Secret<SecretData> kv2Secret = await vaultClient.V1.Secrets.KeyValue.V2
.ReadSecretAsync(path: "jwt", mountPoint: "secret");
var jwtSecret = kv2Secret.Data.Data["secret"];
var jwtIssuer = kv2Secret.Data.Data["issuer"];
var vaultjwtInternalApiKey = kv2Secret.Data.Data["internalApiKey"];

string mySecret = Convert.ToString(jwtSecret) ?? "none";
string myIssuer = Convert.ToString(jwtIssuer) ?? "none";
string vaultInternalApiKey = Convert.ToString(vaultjwtInternalApiKey) ?? "none";
WebManager.GetInstance.HttpClient.DefaultRequestHeaders.Add("X-Internal-ApiKey", vaultInternalApiKey);
builder.Services
.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = myIssuer,
        ValidAudience = "http://localhost",
        IssuerSigningKey =
    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(mySecret))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Check for the internal API key header
            if (context.Request.Headers.TryGetValue("X-Internal-ApiKey", out var extractedApiKey))
            {
                var internalApiKey = vaultInternalApiKey; // or fetch from configuration
                if (internalApiKey.Equals(extractedApiKey))
                {
                    // Set a flag to indicate this is an internal request
                    context.HttpContext.Items["InternalRequest"] = true;

                    // Skip JWT token validation for internal requests
                    context.NoResult();
                    context.Response.Headers.Add("X-Auth-Skipped", "true");
                }
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // If it's an internal request, mark it as successful
            if (context.HttpContext.Items.ContainsKey("InternalRequest"))
            {
                context.Success();
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // If it's an internal request, don't return a 401 error
            if (context.HttpContext.Items.ContainsKey("InternalRequest"))
            {
                context.HandleResponse(); // This suppresses the default 401
            }
            return Task.CompletedTask;
        }
    };
});
// Add services to the container.


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<ILotService, LotService.Services.LotService>();
builder.Logging.ClearProviders();
builder.Host.UseNLog();
var app = builder.Build();

app.Logger.LogInformation("Starting service");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.Logger.LogInformation("Controllers mapped");

app.Run();
NLog.LogManager.Shutdown();