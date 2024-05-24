using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Runtime.CompilerServices;

namespace LotService.Models
{
    public class LotModel
    {
        [BsonElement("_id")]
        private string _id = Guid.NewGuid().ToString();
        private List<BidModel>? bids = new List<BidModel>();
        private string lotName;
        private string location;
        private bool onlineAuction;
        private int startingPrice;
        private int minimumBid;
        private bool open;
        private DateTime lotCreationTime;
        private DateTime lotEndTime;

        public string? LotId
        {
            get => _id;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Lot ID cannot be null or empty.");

                if (!Guid.TryParse(value, out _))
                    throw new ArgumentException("Lot ID must be a valid GUID.");

                _id = value;
            }
        }

        public List<BidModel>? Bids
        {
            get => bids;
            set => bids = value ?? new List<BidModel>();
        }

        public string LotName
        {
            get => lotName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Lot name cannot be null or empty.");
                lotName = value;
            }
        }

        public string Location
        {
            get => location;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Location cannot be null or empty.");
                location = value;
            }
        }

        public bool OnlineAuction
        {
            get => onlineAuction;
            set => onlineAuction = value;
        }

        public int StartingPrice
        {
            get => startingPrice;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Starting price cannot be negative.");
                startingPrice = value;
            }
        }

        public int MinimumBid
        {
            get => minimumBid;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Minimum bid cannot be negative.");
                minimumBid = value;
            }
        }

        public DateTime LotCreationTime
        {
            get => lotCreationTime;
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentException("Lot creation time cannot be in the future.");
                lotCreationTime = value;
            }
        }

        public bool Open { get => open; set => open = value; }
        public DateTime LotEndTime { 
            get => lotEndTime; set
            {
                if(value < LotCreationTime)
                    throw new ArgumentException("Lot end time cannot be before lot creation time.");
                lotEndTime = value;
            }
        }
    }
}
