using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Business
{
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<IEnumerable<Event>> GetAllEventsIncludingInactiveAsync();
        Task<Event?> GetEventByIdAsync(int id);
        Task<IEnumerable<Event>> GetEventsByOrganizerIdAsync(string organizerId);
        Task<IEnumerable<Event>> SearchEventsAsync(string? category, DateTime? date, string? location, string? searchTerm);
        Task<IEnumerable<Event>> GetUpcomingEventsAsync();
        Task<bool> CreateEventAsync(Event eventItem);
        Task<bool> UpdateEventAsync(Event eventItem);
        Task<bool> DeleteEventAsync(int id);
        Task<bool> CheckEventAvailabilityAsync(int eventId, int requestedTickets);
        Task<decimal> CalculateTicketPriceAsync(int eventId, string? promotionCode);
        Task<bool> ReserveTicketsAsync(int eventId, int ticketCount);
    }
}