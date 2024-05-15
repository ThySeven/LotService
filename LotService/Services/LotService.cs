using LotService.Models;
using MongoDB.Driver;
using System.Text.Json;

namespace LotService.Services
{
    public class LotService : ILotService
    {
        private readonly IMongoCollection<LotModel> _lotsCollection;
        private readonly HttpClient _httpClient;
        private string _connectionString = Environment.GetEnvironmentVariable("MongoDBConnectionString");

        public LotService()
        {
            var mongoClient = new MongoClient(_connectionString);
            var database = mongoClient.GetDatabase("AuctionCoreServices");
            _lotsCollection = database.GetCollection<LotModel>("Lots");
            _httpClient = new HttpClient();
        }

        public async Task CheckLotTimer()
        {
            var filter = Builders<LotModel>.Filter.And(
                Builders<LotModel>.Filter.Where(lot => lot.Open),
                Builders<LotModel>.Filter.Where(lot => lot.LotCreationTime <= lot.LotCreationTime) // Assuming the lot expires after 30 minutes
            );

            var expiredLots = await _lotsCollection.Find(filter).ToListAsync();
            foreach (var lot in expiredLots)
            {
                await CloseLot(lot);
            }
        }

        public async Task CloseLot(LotModel lot)
        {
            lot.Open = false;
            await _lotsCollection.ReplaceOneAsync(l => l.LotId == lot.LotId, lot);

            var highestBid = lot.Bids.OrderByDescending(b => b.Amount).FirstOrDefault();
            if (highestBid == null)
            {
                AuctionCoreLogger.Logger.Error($"Lot {lot.LotName} - {lot.LotId} closed with no bidders");
                throw new Exception("No bids found for the lot CloseLot");
            }

            var user = await FetchUserAsync(highestBid.BidderId);
            if (user == null)
            {
                //AuctionCoreLogger.Logger.Error($"Lot {lot.LotName} - {lot.LotId} closed with highest bidder having no account");
                //throw new Exception("User not found");
            }

            var invoice = new InvoiceModel
            {
                Id = Guid.NewGuid().ToString(),
                Description = "Invoice for lot " + lot.LotName,
                PaidStatus = false,
                Address = user.Address,
                Email = user.Email,
                Price = highestBid.Amount
            };

            var response = await WebManager.GetInstance.HttpClient.PostAsJsonAsync($"http://{Environment.GetEnvironmentVariable("InvoiceServiceEndpoint")}/invoice/create", invoice);
            if (!response.IsSuccessStatusCode)
            {
                AuctionCoreLogger.Logger.Error($"Lot {lot.LotName} - {lot.LotId} failed to send and create invoice \nStatuscode: {response.StatusCode} \nRequestMessage {response.RequestMessage} \nContent: {response.Content}");
                throw new Exception("Failed to create invoice CloseLot");
            }
        }

        private async Task<UserModelDTO> FetchUserAsync(string bidderId)
        {
            var response = await WebManager.GetInstance.HttpClient.GetAsync($"http://{Environment.GetEnvironmentVariable("UserServiceEndpoint")}/user/{bidderId}");
            if (response.IsSuccessStatusCode)
            {
                UserModelDTO user = await response.Content.ReadFromJsonAsync<UserModelDTO>()
                AuctionCoreLogger.Logger.Info($"Fetched user from userservice: {JsonSerializer.Serialize(user)}");
                return user;
            }
            else
            {
                AuctionCoreLogger.Logger.Error($"Failed to contact UserService {response.ReasonPhrase}");
            }
            return null;
        }

        public async Task CreateLot(LotModel lot)
        {
            await _lotsCollection.InsertOneAsync(lot);
            AuctionCoreLogger.Logger.Info($"Lot {lot.LotName} - {lot.LotId} created");
        }

        public async Task DeleteLot(string lotId)
        {
            var lot = await GetLot(lotId);
            await _lotsCollection.DeleteOneAsync(lot => lot.LotId == lotId);
            AuctionCoreLogger.Logger.Info($"Lot {lot.LotName} - {lot.LotId} deleted");
        }

        public async Task<LotModel> GetLot(string lotId)
        {
            return await _lotsCollection.Find(lot => lot.LotId == lotId).FirstOrDefaultAsync();
        }

        public async Task<List<LotModel>> GetLots()
        {
            return await _lotsCollection.Find(_ => true).ToListAsync();
        }

        public async Task UpdateLot(LotModel lot)
        {
            var update = Builders<LotModel>.Update
                .Set(l => l.LotName, lot.LotName)
                .Set(l => l.Location, lot.Location)
                .Set(l => l.OnlineAuction, lot.OnlineAuction)
                .Set(l => l.StartingPrice, lot.StartingPrice)
                .Set(l => l.MinimumBid, lot.MinimumBid)
                .Set(l => l.Open, lot.Open)
                .Set(l => l.LotCreationTime, lot.LotCreationTime);

            await _lotsCollection.UpdateOneAsync(l => l.LotId == lot.LotId, update);
            AuctionCoreLogger.Logger.Info($"Lot {lot.LotName} - {lot.LotId} updated");
        }

        public async Task UpdateLotPrice(BidModel bid)
        {

            var lot = await _lotsCollection.Find(l => l.LotId == bid.LotId).FirstOrDefaultAsync();
            UserModelDTO user;
            if (lot == null)
            {
                AuctionCoreLogger.Logger.Error($"Lot for bid not found {bid.LotId} {bid.BidderId}");
                throw new Exception("Lot not found UpdateLotPrice");
            }

            try
            {
                user = await FetchUserAsync(bid.BidderId);

            }
            catch(Exception ex)
            {
                AuctionCoreLogger.Logger.Error($"User for bid not found {bid.LotId} {bid.BidderId}");
                AuctionCoreLogger.Logger.Error($"{ex}");
                throw new Exception("User not found UpdateLotPrice");

            }


            if (bid.Timestamp > lot.LotEndTime)
            {
                AuctionCoreLogger.Logger.Error($"Bid for lot {bid.LotId} {bid.BidderId} attempted {(bid.Timestamp - lot.LotEndTime).Seconds} seconds past lot end time");
                throw new Exception("Lot has ended UpdateLotPrice");
            }

            var highestBid = lot.Bids.OrderByDescending(b => b.Amount).FirstOrDefault();
            if (highestBid != null && bid.Amount <= highestBid.Amount)
            {
                AuctionCoreLogger.Logger.Warn($"Bid made that is under the current price by {user.UserName} \nAttempt:{bid.Amount}\nHighest Bid: {highestBid.Amount}");
                return;
            }


            lot.Bids.Add(bid);
            lot.Bids = lot.Bids.OrderByDescending(b => b.Amount).ToList();
            if (bid.Timestamp > lot.LotEndTime.AddSeconds(-30))
                lot.LotEndTime = lot.LotEndTime.AddSeconds(30);
            await _lotsCollection.ReplaceOneAsync(l => l.LotId == lot.LotId, lot);

            if (highestBid != null)
            {
                UserModelDTO oldUser = await FetchUserAsync(highestBid.BidderId);
                Notification notification = new Notification()
                {
                    LotId = lot.LotId,
                    LotName = lot.LotName,
                    TimeStamp = bid.Timestamp,
                    ReceiverMail = oldUser.Email,
                    NewLotPrice = bid.Amount,
                    NewBidLink = $"https://{Environment.GetEnvironmentVariable("PublicIp")}"
                };
                var response = await WebManager.GetInstance.HttpClient.PostAsJsonAsync($"https://{Environment.GetEnvironmentVariable("BiddingServiceEndpoint")}/api/notification", notification);
                if (response.IsSuccessStatusCode)
                {
                    AuctionCoreLogger.Logger.Info($"Overbid notification sent to {oldUser.Email}");
                }
                else
                {
                    AuctionCoreLogger.Logger.Info($"Failed to contact biddingservice {response.ReasonPhrase}");
                }
            }
            AuctionCoreLogger.Logger.Info($"Lot {lot.LotName} - {lot.LotId} updated");

        }
    }
}
