using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Repository
{
    public interface ITicketRepository
    {
        Task<IEnumerable<Ticket>> GetAllTicketsAsync();
        Task<Ticket?> GetTicketByIdAsync(int id);
        Task<IEnumerable<Ticket>> GetTicketsByCustomerIdAsync(string customerId);
        Task<IEnumerable<Ticket>> GetTicketsByEventIdAsync(int eventId);
        Task<Ticket?> GetTicketByQrCodeAsync(string qrCode);
        Task<bool> CreateTicketAsync(Ticket ticket);
        Task<bool> UpdateTicketAsync(Ticket ticket);
        Task<bool> UpdateTicketStatusAsync(int ticketId, TicketStatus status);
        Task<bool> DeleteTicketAsync(int id);
        Task<int> GetAvailableTicketsCountAsync(int eventId);
    }
}