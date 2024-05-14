using LotService.Models;

namespace LotService.Services
{
    public interface ILotService
    {
        public Task CreateLot(LotModel lot);
        public Task CloseLot(LotModel lot);
        public Task CheckLotTimer();
        public Task DeleteLot(string lotId);
        public Task UpdateLot(LotModel lot);
        public Task UpdateLotPrice(BidModel bid);
        public Task<List<LotModel>> GetLots();
        public Task<LotModel> GetLot(string lotId);
    }
}
