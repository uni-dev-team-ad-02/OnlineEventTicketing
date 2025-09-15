using Microsoft.EntityFrameworkCore;
using OnlineEventTicketing.Data;
using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Repository
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TicketRepository> _logger;

        public TicketRepository(ApplicationDbContext context, ILogger<TicketRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Ticket>> GetAllTicketsAsync()
        {
            try
            {
                return await _context.Tickets
                    .Include(t => t.Event)
                    .Include(t => t.Customer)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Ticket>();
            }
        }

        public async Task<Ticket?> GetTicketByIdAsync(int id)
        {
            try
            {
                _logger.LogDebug("Retrieving ticket with ID {TicketId}", id);
                var ticket = await _context.Tickets
                    .Include(t => t.Event)
                    .Include(t => t.Customer)
                    .Include(t => t.Payments)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (ticket == null)
                {
                    _logger.LogWarning("Ticket with ID {TicketId} not found", id);
                }
                else
                {
                    _logger.LogDebug("Successfully retrieved ticket {TicketId} for event {EventId}", id, ticket.EventId);
                }

                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket with ID {TicketId}", id);
                return null;
            }
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByCustomerIdAsync(string customerId)
        {
            try
            {
                return await _context.Tickets
                    .Include(t => t.Event)
                    .Include(t => t.Customer)
                    .Where(t => t.CustomerId == customerId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Ticket>();
            }
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByEventIdAsync(int eventId)
        {
            try
            {
                return await _context.Tickets
                    .Include(t => t.Event)
                    .Include(t => t.Customer)
                    .Where(t => t.EventId == eventId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Ticket>();
            }
        }

        public async Task<Ticket?> GetTicketByQrCodeAsync(string qrCode)
        {
            try
            {
                return await _context.Tickets
                    .Include(t => t.Event)
                    .Include(t => t.Customer)
                    .FirstOrDefaultAsync(t => t.QrCode == qrCode);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> CreateTicketAsync(Ticket ticket)
        {
            try
            {
                _logger.LogInformation("Creating new ticket for event {EventId}, customer {CustomerId}",
                    ticket.EventId, ticket.CustomerId);

                ticket.QrCode = GenerateQrCode();
                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created ticket {TicketId} with QR code {QrCode}",
                    ticket.Id, ticket.QrCode);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket for event {EventId}, customer {CustomerId}",
                    ticket.EventId, ticket.CustomerId);
                return false;
            }
        }

        public async Task<bool> UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.UpdatedAt = DateTime.UtcNow;
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateTicketStatusAsync(int ticketId, TicketStatus status)
        {
            try
            {
                _logger.LogDebug("Updating ticket {TicketId} status to {Status}", ticketId, status);
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket != null)
                {
                    ticket.Status = status;
                    ticket.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully updated ticket {TicketId} status to {Status}", ticketId, status);
                    return true;
                }
                _logger.LogWarning("Ticket {TicketId} not found when updating status", ticketId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket {TicketId} status to {Status}", ticketId, status);
                return false;
            }
        }

        public async Task<bool> DeleteTicketAsync(int id)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(id);
                if (ticket != null)
                {
                    ticket.DeletedAt = DateTime.UtcNow;
                    ticket.UpdatedAt = DateTime.UtcNow;
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

        public async Task<int> GetAvailableTicketsCountAsync(int eventId)
        {
            try
            {
                var eventItem = await _context.Events.FindAsync(eventId);
                return eventItem?.AvailableTickets ?? 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private string GenerateQrCode()
        {
            return $"TKT-{Guid.NewGuid().ToString().ToUpper().Replace("-", "")}";
        }
    }
}