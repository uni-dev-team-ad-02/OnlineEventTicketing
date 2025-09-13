using Microsoft.EntityFrameworkCore;
using OnlineEventTicketing.Data;
using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Repository
{
    public class PromotionRepository : IPromotionRepository
    {
        private readonly ApplicationDbContext _context;

        public PromotionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Promotion>> GetAllPromotionsAsync()
        {
            try
            {
                return await _context.Promotions
                    .Include(p => p.Event)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Promotion>();
            }
        }

        public async Task<Promotion?> GetPromotionByIdAsync(int id)
        {
            try
            {
                return await _context.Promotions
                    .Include(p => p.Event)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<Promotion>> GetPromotionsByEventIdAsync(int eventId)
        {
            try
            {
                return await _context.Promotions
                    .Include(p => p.Event)
                    .Where(p => p.EventId == eventId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Promotion>();
            }
        }

        public async Task<Promotion?> GetPromotionByCodeAsync(string code)
        {
            try
            {
                return await _context.Promotions
                    .Include(p => p.Event)
                    .FirstOrDefaultAsync(p => p.Code == code);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync()
        {
            try
            {
                var currentDate = DateTime.UtcNow;
                return await _context.Promotions
                    .Include(p => p.Event)
                    .Where(p => p.IsActive && p.StartDate <= currentDate && p.EndDate >= currentDate)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Promotion>();
            }
        }

        public async Task<bool> CreatePromotionAsync(Promotion promotion)
        {
            try
            {
                _context.Promotions.Add(promotion);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdatePromotionAsync(Promotion promotion)
        {
            try
            {
                promotion.UpdatedAt = DateTime.UtcNow;
                _context.Promotions.Update(promotion);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeletePromotionAsync(int id)
        {
            try
            {
                var promotion = await _context.Promotions.FindAsync(id);
                if (promotion != null)
                {
                    promotion.DeletedAt = DateTime.UtcNow;
                    promotion.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ValidatePromotionCodeAsync(string code, int eventId)
        {
            try
            {
                var currentDate = DateTime.UtcNow;
                var promotion = await _context.Promotions
                    .FirstOrDefaultAsync(p => p.Code == code && 
                                            p.EventId == eventId && 
                                            p.IsActive && 
                                            p.StartDate <= currentDate && 
                                            p.EndDate >= currentDate);
                return promotion != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<Promotion>> GetPromotionsByOrganizerIdAsync(string organizerId)
        {
            try
            {
                return await _context.Promotions
                    .Include(p => p.Event)
                    .Where(p => p.Event.OrganizerId == organizerId && p.DeletedAt == null)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Promotion>();
            }
        }

        public async Task<bool> IsPromotionOwnedByOrganizerAsync(int promotionId, string organizerId)
        {
            try
            {
                return await _context.Promotions
                    .Include(p => p.Event)
                    .AnyAsync(p => p.Id == promotionId && 
                                 p.Event.OrganizerId == organizerId && 
                                 p.DeletedAt == null);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}