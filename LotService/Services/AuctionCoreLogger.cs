using NLog;
using ILogger = NLog.ILogger;

namespace LotService.Services
{
    public class AuctionCoreLogger
    {
        public static ILogger Logger { get; } = LogManager.GetCurrentClassLogger();
    }
}