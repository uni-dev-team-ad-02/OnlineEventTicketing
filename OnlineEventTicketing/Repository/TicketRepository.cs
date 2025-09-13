using Microsoft.EntityFrameworkCore;
using OnlineEventTicketing.Data;
using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Repository
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketRepository(ApplicationDbContext context)
        {
            _context = context;
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
                return await _context.Tickets
                    .Include(t => t.Event)
                    .Include(t => t.Customer)
                    .Include(t => t.Payments)
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception)
            {
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
                ticket.QrCode = GenerateQrCode();
                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
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
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket != null)
                {
                    ticket.Status = status;
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