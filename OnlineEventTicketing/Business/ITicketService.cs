using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Business
{
    public interface ITicketService
    {
        Task<IEnumerable<Ticket>> GetAllTicketsAsync();
        Task<Ticket?> GetTicketByIdAsync(int id);
        Task<IEnumerable<Ticket>> GetTicketsByCustomerIdAsync(string customerId);
        Task<IEnumerable<Ticket>> GetTicketsByEventIdAsync(int eventId);
        Task<Ticket?> GetTicketByQrCodeAsync(string qrCode);
        Task<Ticket?> PurchaseTicketAsync(int eventId, string customerId, string? promotionCode);
        Task<bool> UpdateTicketStatusAsync(int ticketId, TicketStatus status);
        Task<bool> ValidateTicketAsync(string qrCode);
        Task<bool> CancelTicketAsync(int ticketId);
        Task<bool> RefundTicketAsync(int ticketId);
        Task<string> GenerateTicketQrCodeAsync(int ticketId);
    }
}