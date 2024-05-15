using LotService.Models;
using NLog.Config;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LotService.Services
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ILotService _lotservice;

        public Worker(ILogger<Worker> logger, ILotService lotservice)
        {
            _logger = logger;
            _lotservice = lotservice;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory { HostName = Environment.GetEnvironmentVariable("RabbitMQHostName") };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: Environment.GetEnvironmentVariable("RabbitMQQueueName"),
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var biddingConsumer = new EventingBasicConsumer(channel);
            biddingConsumer.Received += async (model, ea) =>
            {
                var mailBody = ea.Body.ToArray();
                var uftString = Encoding.UTF8.GetString(mailBody);
                try
                {
                    var message = JsonSerializer.Deserialize<BidModel>(uftString);
                    AuctionCoreLogger.Logger.Info($"Recieved bid from biddingservice: \nBidderid: {message.BidderId} \nAmount: {message.Amount} \nTime: {message.Timestamp}");
                    await _lotservice.UpdateLotPrice(message); // Adjust according to your actual service method

                }
                catch (Exception ex)
                {
                    AuctionCoreLogger.Logger.Error(ex);
                    AuctionCoreLogger.Logger.Error("Failed to receive/parse bid from bidservice");
                }

            };

            channel.BasicConsume(queue: Environment.GetEnvironmentVariable("RabbitMQQueueName"),
                                 autoAck: true,
                                 consumer: biddingConsumer);

            var task1 = Task.Run(async () => await RunEveryFiveSeconds(stoppingToken));
            var task2 = Task.Run(async () => await RunEverySecond(stoppingToken));

            await Task.WhenAll(task1, task2);
        }

        private async Task RunEveryFiveSeconds(CancellationToken stoppingToken)
        {
            AuctionCoreLogger.Logger.Info("Running 5-second listener");
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Run(() => _lotservice.CheckLotTimer());
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task RunEverySecond(CancellationToken stoppingToken)
        {
            AuctionCoreLogger.Logger.Info("Running 1-second listener");
            while (!stoppingToken.IsCancellationRequested)
            {
                // 
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}
