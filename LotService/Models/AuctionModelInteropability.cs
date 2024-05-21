namespace LotService.Models
{
    public class AuctionModelInteropability
    {

        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal CurrentBid { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public static class AuctionMapper
    {
        public static AuctionModelInteropability MapToAuction(LotModel lot)
        {
            return new AuctionModelInteropability
            {
                Id = Guid.NewGuid(), // You might want to consider a different mapping if LotId should map to Id
                Title = lot.LotName,
                Description = $"{lot.LotName} located in {lot.Location}",
                StartDate = lot.LotCreationTime,
                EndDate = lot.LotEndTime,
                CurrentBid = lot.Bids?.OrderByDescending(b => b.Amount).FirstOrDefault()?.Amount ?? (decimal)lot.StartingPrice,
                CreatedBy = "System", // Placeholder for actual creation user
                CreatedAt = lot.LotCreationTime
            };
        }
    }

}
