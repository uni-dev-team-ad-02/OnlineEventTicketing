using OnlineEventTicketing.Data.Entity;
using OnlineEventTicketing.Repository;

namespace OnlineEventTicketing.Business
{
    public class PromotionService : IPromotionService
    {
        private readonly IPromotionRepository _promotionRepository;

        public PromotionService(IPromotionRepository promotionRepository)
        {
            _promotionRepository = promotionRepository;
        }

        public async Task<IEnumerable<Promotion>> GetAllPromotionsAsync()
        {
            return await _promotionRepository.GetAllPromotionsAsync();
        }

        public async Task<Promotion?> GetPromotionByIdAsync(int id)
        {
            return await _promotionRepository.GetPromotionByIdAsync(id);
        }

        public async Task<IEnumerable<Promotion>> GetPromotionsByEventIdAsync(int eventId)
        {
            return await _promotionRepository.GetPromotionsByEventIdAsync(eventId);
        }

        public async Task<Promotion?> GetPromotionByCodeAsync(string code)
        {
            return await _promotionRepository.GetPromotionByCodeAsync(code);
        }

        public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync()
        {
            return await _promotionRepository.GetActivePromotionsAsync();
        }

        public async Task<bool> CreatePromotionAsync(Promotion promotion)
        {
            // Validate promotion dates
            if (promotion.StartDate >= promotion.EndDate)
                return false;

            // Validate discount percentage
            if (promotion.DiscountPercentage < 0 || promotion.DiscountPercentage > 100)
                return false;

            return await _promotionRepository.CreatePromotionAsync(promotion);
        }

        public async Task<bool> UpdatePromotionAsync(Promotion promotion)
        {
            // Validate promotion dates
            if (promotion.StartDate >= promotion.EndDate)
                return false;

            // Validate discount percentage
            if (promotion.DiscountPercentage < 0 || promotion.DiscountPercentage > 100)
                return false;

            return await _promotionRepository.UpdatePromotionAsync(promotion);
        }

        public async Task<bool> DeletePromotionAsync(int id)
        {
            return await _promotionRepository.DeletePromotionAsync(id);
        }

        public async Task<bool> ValidatePromotionAsync(string code, int eventId)
        {
            return await _promotionRepository.ValidatePromotionCodeAsync(code, eventId);
        }

        public async Task<decimal> CalculateDiscountAsync(string code, decimal originalAmount)
        {
            var promotion = await _promotionRepository.GetPromotionByCodeAsync(code);
            if (promotion == null || !promotion.IsActive)
                return 0;

            var currentDate = DateTime.UtcNow;
            if (currentDate < promotion.StartDate || currentDate > promotion.EndDate)
                return 0;

            return originalAmount * (promotion.DiscountPercentage / 100);
        }

        public async Task<IEnumerable<Promotion>> GetPromotionsByOrganizerIdAsync(string organizerId)
        {
            return await _promotionRepository.GetPromotionsByOrganizerIdAsync(organizerId);
        }

        public async Task<bool> IsPromotionOwnedByOrganizerAsync(int promotionId, string organizerId)
        {
            return await _promotionRepository.IsPromotionOwnedByOrganizerAsync(promotionId, organizerId);
        }
    }
}