using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Business
{
    public interface IPromotionService
    {
        Task<IEnumerable<Promotion>> GetAllPromotionsAsync();
        Task<Promotion?> GetPromotionByIdAsync(int id);
        Task<IEnumerable<Promotion>> GetPromotionsByEventIdAsync(int eventId);
        Task<Promotion?> GetPromotionByCodeAsync(string code);
        Task<IEnumerable<Promotion>> GetActivePromotionsAsync();
        Task<bool> CreatePromotionAsync(Promotion promotion);
        Task<bool> UpdatePromotionAsync(Promotion promotion);
        Task<bool> DeletePromotionAsync(int id);
        Task<bool> ValidatePromotionAsync(string code, int eventId);
        Task<decimal> CalculateDiscountAsync(string code, decimal originalAmount);
        Task<IEnumerable<Promotion>> GetPromotionsByOrganizerIdAsync(string organizerId);
        Task<bool> IsPromotionOwnedByOrganizerAsync(int promotionId, string organizerId);
    }
}