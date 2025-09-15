using OnlineEventTicketing.Data.Entity;
using OnlineEventTicketing.Repository;

namespace OnlineEventTicketing.Business
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IPromotionRepository _promotionRepository;
        private readonly ILogger<TicketService> _logger;

        public TicketService(ITicketRepository ticketRepository, IEventRepository eventRepository, IPromotionRepository promotionRepository, ILogger<TicketService> logger)
        {
            _ticketRepository = ticketRepository;
            _eventRepository = eventRepository;
            _promotionRepository = promotionRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Ticket>> GetAllTicketsAsync()
        {
            return await _ticketRepository.GetAllTicketsAsync();
        }

        public async Task<Ticket?> GetTicketByIdAsync(int id)
        {
            return await _ticketRepository.GetTicketByIdAsync(id);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByCustomerIdAsync(string customerId)
        {
            return await _ticketRepository.GetTicketsByCustomerIdAsync(customerId);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByEventIdAsync(int eventId)
        {
            return await _ticketRepository.GetTicketsByEventIdAsync(eventId);
        }

        public async Task<Ticket?> GetTicketByQrCodeAsync(string qrCode)
        {
            return await _ticketRepository.GetTicketByQrCodeAsync(qrCode);
        }

        public async Task<Ticket?> PurchaseTicketAsync(int eventId, string customerId, string? promotionCode)
        {
            // Check event availability
            var eventItem = await _eventRepository.GetEventByIdAsync(eventId);
            if (eventItem == null || !eventItem.IsActive || eventItem.AvailableTickets < 1) 
                return null;

            // Calculate price with promotion
            decimal price = eventItem.BasePrice;
            if (!string.IsNullOrEmpty(promotionCode))
            {
                var isValidPromotion = await _promotionRepository.ValidatePromotionCodeAsync(promotionCode, eventId);
                if (isValidPromotion)
                {
                    var promotion = await _promotionRepository.GetPromotionByCodeAsync(promotionCode);
                    if (promotion != null)
                    {
                        decimal discount = price * (promotion.DiscountPercentage / 100);
                        price = price - discount;
                    }
                }
            }

            // Create ticket
            var ticket = new Ticket
            {
                EventId = eventId,
                CustomerId = customerId,
                Price = price,
                Status = TicketStatus.Active,
                PurchaseDate = DateTime.UtcNow
            };

            var success = await _ticketRepository.CreateTicketAsync(ticket);
            if (!success) return null;

            // Reserve tickets in event
            int newAvailableTickets = eventItem.AvailableTickets - 1;
            await _eventRepository.UpdateAvailableTicketsAsync(eventId, newAvailableTickets);

            return ticket;
        }

        public async Task<bool> UpdateTicketStatusAsync(int ticketId, TicketStatus status)
        {
            return await _ticketRepository.UpdateTicketStatusAsync(ticketId, status);
        }

        public async Task<bool> ValidateTicketAsync(string qrCode)
        {
            var ticket = await _ticketRepository.GetTicketByQrCodeAsync(qrCode);
            if (ticket == null) return false;

            return ticket.Status == TicketStatus.Active && ticket.Event.Date > DateTime.UtcNow;
        }

        public async Task<bool> CancelTicketAsync(int ticketId)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null || ticket.Status != TicketStatus.Active) return false;

            // Update ticket status
            var success = await _ticketRepository.UpdateTicketStatusAsync(ticketId, TicketStatus.Cancelled);
            
            if (success)
            {
                // Return available tickets to event
                var eventItem = await _eventRepository.GetEventByIdAsync(ticket.EventId);
                if (eventItem != null)
                {
                    int newAvailableTickets = eventItem.AvailableTickets + 1;
                    await _eventRepository.UpdateAvailableTicketsAsync(ticket.EventId, newAvailableTickets);
                }
            }

            return success;
        }

        public async Task<bool> RefundTicketAsync(int ticketId)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null) return false;

            // Update ticket status
            var success = await _ticketRepository.UpdateTicketStatusAsync(ticketId, TicketStatus.Refunded);

            if (success)
            {
                // Return available tickets to event
                var eventItem = await _eventRepository.GetEventByIdAsync(ticket.EventId);
                if (eventItem != null)
                {
                    int newAvailableTickets = eventItem.AvailableTickets + 1;
                    await _eventRepository.UpdateAvailableTicketsAsync(ticket.EventId, newAvailableTickets);
                }
            }

            return success;
        }

        public async Task<string> GenerateTicketQrCodeAsync(int ticketId)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            return ticket?.QrCode ?? string.Empty;
        }
    }
}