using MongoDB.Bson;

namespace LotService.Models
{
    public class AuctionModelInteropability
    {

        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? CurrentBid { get; set; }
        public string CreatedBy { get; set; }
        public string? PurchasedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public static class AuctionMapper
    {
        public static AuctionModelInteropability MapToAuction(LotModel lot)
        {
            return new AuctionModelInteropability
            {
                Id = Guid.Parse(lot.LotId), // You might want to consider a different mapping if LotId should map to Id
                Title = lot.LotName,
                Description = $"{lot.LotName} located in {lot.Location}",
                StartDate = lot.LotCreationTime,
                EndDate = lot.LotEndTime,
                CurrentBid = lot.Bids?.OrderByDescending(b => b.Amount).FirstOrDefault()?.Amount ?? (decimal)lot.StartingPrice,
                PurchasedBy = lot.Open == false ? "Auction has not ended" : lot.Bids?.OrderByDescending(b => b.Amount).FirstOrDefault()?.BidderId ?? "Purchaser not foind",
                CreatedBy = "CEO, Kell Olsen",
                CreatedAt = lot.LotCreationTime
            };
        }
    }

}
