using OnlineEventTicketing.Data.Entity;
using OnlineEventTicketing.Repository;

namespace OnlineEventTicketing.Business
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IPromotionRepository _promotionRepository;
        private readonly ILogger<EventService> _logger;

        public EventService(IEventRepository eventRepository, IPromotionRepository promotionRepository, ILogger<EventService> logger)
        {
            _eventRepository = eventRepository;
            _promotionRepository = promotionRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            try
            {
                _logger.LogDebug("Retrieving all active events");
                var events = await _eventRepository.GetAllEventsAsync();
                _logger.LogInformation("Successfully retrieved {EventCount} active events", events.Count());
                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all events");
                throw;
            }
        }

        public async Task<IEnumerable<Event>> GetAllEventsIncludingInactiveAsync()
        {
            return await _eventRepository.GetAllEventsIncludingInactiveAsync();
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _eventRepository.GetEventByIdAsync(id);
        }

        public async Task<IEnumerable<Event>> GetEventsByOrganizerIdAsync(string organizerId)
        {
            return await _eventRepository.GetEventsByOrganizerIdAsync(organizerId);
        }

        public async Task<IEnumerable<Event>> SearchEventsAsync(string? category, DateTime? date, string? location, string? searchTerm)
        {
            return await _eventRepository.SearchEventsAsync(category, date, location, searchTerm);
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            return await _eventRepository.GetUpcomingEventsAsync();
        }

        public async Task<bool> CreateEventAsync(Event eventItem)
        {
            try
            {
                _logger.LogInformation("Creating new event: {EventTitle} by organizer {OrganizerId}",
                    eventItem.Title, eventItem.OrganizerId);

                var result = await _eventRepository.CreateEventAsync(eventItem);

                if (result)
                {
                    _logger.LogInformation("Successfully created event {EventId}: {EventTitle}",
                        eventItem.Id, eventItem.Title);
                }
                else
                {
                    _logger.LogWarning("Failed to create event: {EventTitle}", eventItem.Title);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event: {EventTitle}", eventItem.Title);
                throw;
            }
        }

        public async Task<bool> UpdateEventAsync(Event eventItem)
        {
            return await _eventRepository.UpdateEventAsync(eventItem);
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            return await _eventRepository.DeleteEventAsync(id);
        }

        public async Task<bool> CheckEventAvailabilityAsync(int eventId, int requestedTickets)
        {
            var eventItem = await _eventRepository.GetEventByIdAsync(eventId);
            if (eventItem == null) return false;

            return eventItem.AvailableTickets >= requestedTickets && eventItem.IsActive;
        }

        public async Task<decimal> CalculateTicketPriceAsync(int eventId, string? promotionCode)
        {
            var eventItem = await _eventRepository.GetEventByIdAsync(eventId);
            if (eventItem == null) return 0;

            decimal basePrice = eventItem.BasePrice;

            if (!string.IsNullOrEmpty(promotionCode))
            {
                var isValidPromotion = await _promotionRepository.ValidatePromotionCodeAsync(promotionCode, eventId);
                if (isValidPromotion)
                {
                    var promotion = await _promotionRepository.GetPromotionByCodeAsync(promotionCode);
                    if (promotion != null)
                    {
                        decimal discount = basePrice * (promotion.DiscountPercentage / 100);
                        return basePrice - discount;
                    }
                }
            }

            return basePrice;
        }

        public async Task<bool> ReserveTicketsAsync(int eventId, int ticketCount)
        {
            var eventItem = await _eventRepository.GetEventByIdAsync(eventId);
            if (eventItem == null || eventItem.AvailableTickets < ticketCount)
                return false;

            int newAvailableTickets = eventItem.AvailableTickets - ticketCount;
            return await _eventRepository.UpdateAvailableTicketsAsync(eventId, newAvailableTickets);
        }
    }
}