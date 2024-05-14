namespace LotService.Models
{
    public class BidModel
    {
        public int Amount { get; set; }
        public string BidderId { get; set; }
        public string LotId { get; set; }
        public DateTime Timestamp { get; set; }

        public BidModel(int amount, string bidder, DateTime timestamp)
        {
            Amount = amount;
            BidderId = bidder;
            Timestamp = timestamp;
        }

        public BidModel()
        {

        }
    }
}
