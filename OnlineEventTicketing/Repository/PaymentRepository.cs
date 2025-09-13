using Microsoft.EntityFrameworkCore;
using OnlineEventTicketing.Data;
using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.Ticket)
                    .ThenInclude(t => t.Event)
                    .Include(p => p.Customer)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Payment>();
            }
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.Ticket)
                    .ThenInclude(t => t.Event)
                    .Include(p => p.Customer)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByCustomerIdAsync(string customerId)
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.Ticket)
                    .ThenInclude(t => t.Event)
                    .Include(p => p.Customer)
                    .Where(p => p.CustomerId == customerId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Payment>();
            }
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByTicketIdAsync(int ticketId)
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.Ticket)
                    .Include(p => p.Customer)
                    .Where(p => p.TicketId == ticketId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Payment>();
            }
        }

        public async Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId)
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.Ticket)
                    .ThenInclude(t => t.Event)
                    .Include(p => p.Customer)
                    .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> CreatePaymentAsync(Payment payment)
        {
            try
            {
                payment.TransactionId = GenerateTransactionId();
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdatePaymentAsync(Payment payment)
        {
            try
            {
                payment.UpdatedAt = DateTime.UtcNow;
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(paymentId);
                if (payment != null)
                {
                    payment.Status = status;
                    payment.UpdatedAt = DateTime.UtcNow;
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

        public async Task<bool> DeletePaymentAsync(int id)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(id);
                if (payment != null)
                {
                    payment.DeletedAt = DateTime.UtcNow;
                    payment.UpdatedAt = DateTime.UtcNow;
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

        public async Task<decimal> GetTotalRevenueAsync()
        {
            try
            {
                return await _context.Payments
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .SumAsync(p => p.Amount);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<decimal> GetRevenueByOrganizerAsync(string organizerId)
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.Ticket)
                    .ThenInclude(t => t.Event)
                    .Where(p => p.Status == PaymentStatus.Completed && p.Ticket.Event.OrganizerId == organizerId)
                    .SumAsync(p => p.Amount);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private string GenerateTransactionId()
        {
            return $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}