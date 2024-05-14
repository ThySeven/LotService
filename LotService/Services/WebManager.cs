namespace LotService.Services
{
    public class WebManager
    {
        private static WebManager _instance = new WebManager();
        public static WebManager GetInstance {  get { return _instance; } }

        public HttpClient HttpClient;

        public WebManager()
        {
            HttpClient = new HttpClient();
        }
    }
}
