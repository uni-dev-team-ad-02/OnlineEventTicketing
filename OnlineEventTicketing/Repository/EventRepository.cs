using Microsoft.EntityFrameworkCore;
using OnlineEventTicketing.Data;
using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EventRepository> _logger;

        public EventRepository(ApplicationDbContext context, ILogger<EventRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving all active events");
                var events = await _context.Events
                    .Include(e => e.Organizer)
                    .Where(e => e.IsActive)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();
                _logger.LogInformation("Successfully retrieved {EventCount} active events", events.Count());
                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all events");
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
                _logger.LogDebug("Retrieving event with ID {EventId}", id);
                var eventItem = await _context.Events
                    .Include(e => e.Organizer)
                    .Include(e => e.Tickets)
                    .Include(e => e.Promotions)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (eventItem == null)
                {
                    _logger.LogWarning("Event with ID {EventId} not found", id);
                }
                else
                {
                    _logger.LogDebug("Successfully retrieved event {EventId}: {EventTitle}", id, eventItem.Title);
                }

                return eventItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving event with ID {EventId}", id);
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
                _logger.LogInformation("Creating new event: {EventTitle} by organizer {OrganizerId}",
                    eventItem.Title, eventItem.OrganizerId);

                eventItem.AvailableTickets = eventItem.Capacity;
                _context.Events.Add(eventItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created event {EventId}: {EventTitle}",
                    eventItem.Id, eventItem.Title);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event: {EventTitle}", eventItem.Title);
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
                _logger.LogDebug("Updating available tickets for event {EventId} to {TicketCount}", eventId, ticketCount);
                var eventItem = await _context.Events.FindAsync(eventId);
                if (eventItem != null)
                {
                    eventItem.AvailableTickets = ticketCount;
                    eventItem.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully updated available tickets for event {EventId} to {TicketCount}", eventId, ticketCount);
                    return true;
                }
                _logger.LogWarning("Event {EventId} not found when updating available tickets", eventId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating available tickets for event {EventId}", eventId);
                return false;
            }
        }
    }
}