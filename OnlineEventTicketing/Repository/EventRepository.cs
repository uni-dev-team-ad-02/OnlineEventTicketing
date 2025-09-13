using Microsoft.EntityFrameworkCore;
using OnlineEventTicketing.Data;
using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;

        public EventRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            try
            {
                return await _context.Events
                    .Include(e => e.Organizer)
                    .Where(e => e.IsActive)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Event>();
            }
        }

        public async Task<IEnumerable<Event>> GetAllEventsIncludingInactiveAsync()
        {
            try
            {
                return await _context.Events
                    .Include(e => e.Organizer)
                    .Where(e => e.DeletedAt == null)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Event>();
            }
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            try
            {
                return await _context.Events
                    .Include(e => e.Organizer)
                    .Include(e => e.Tickets)
                    .Include(e => e.Promotions)
                    .FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<Event>> GetEventsByOrganizerIdAsync(string organizerId)
        {
            try
            {
                return await _context.Events
                    .Include(e => e.Organizer)
                    .Where(e => e.OrganizerId == organizerId)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Event>();
            }
        }

        public async Task<IEnumerable<Event>> SearchEventsAsync(string? category, DateTime? date, string? location, string? searchTerm)
        {
            try
            {
                var query = _context.Events
                    .Include(e => e.Organizer)
                    .Where(e => e.IsActive)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(category))
                    query = query.Where(e => e.Category.Contains(category));

                if (date.HasValue)
                    query = query.Where(e => e.Date.Date == date.Value.Date);

                if (!string.IsNullOrEmpty(location))
                    query = query.Where(e => e.Location.Contains(location));

                if (!string.IsNullOrEmpty(searchTerm))
                    query = query.Where(e => e.Title.Contains(searchTerm) || e.Description.Contains(searchTerm));

                return await query.OrderByDescending(e => e.Date).ToListAsync();
            }
            catch (Exception)
            {
                return new List<Event>();
            }
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            try
            {
                return await _context.Events
                    .Include(e => e.Organizer)
                    .Where(e => e.IsActive && e.Date > DateTime.UtcNow)
                    .OrderBy(e => e.Date)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Event>();
            }
        }

        public async Task<bool> CreateEventAsync(Event eventItem)
        {
            try
            {
                eventItem.AvailableTickets = eventItem.Capacity;
                _context.Events.Add(eventItem);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateEventAsync(Event eventItem)
        {
            try
            {
                eventItem.UpdatedAt = DateTime.UtcNow;
                _context.Events.Update(eventItem);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            try
            {
                var eventItem = await _context.Events.FindAsync(id);
                if (eventItem != null)
                {
                    eventItem.DeletedAt = DateTime.UtcNow;
                    eventItem.UpdatedAt = DateTime.UtcNow;
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

        public async Task<bool> UpdateAvailableTicketsAsync(int eventId, int ticketCount)
        {
            try
            {
                var eventItem = await _context.Events.FindAsync(eventId);
                if (eventItem != null)
                {
                    eventItem.AvailableTickets = ticketCount;
                    eventItem.UpdatedAt = DateTime.UtcNow;
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
    }
}