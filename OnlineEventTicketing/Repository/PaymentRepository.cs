using Microsoft.EntityFrameworkCore;
using OnlineEventTicketing.Data;
using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentRepository> _logger;

        public PaymentRepository(ApplicationDbContext context, ILogger<PaymentRepository> logger)
        {
            _context = context;
            _logger = logger;
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
                _logger.LogDebug("Retrieving payment with ID {PaymentId}", id);
                var payment = await _context.Payments
                    .Include(p => p.Ticket)
                    .ThenInclude(t => t.Event)
                    .Include(p => p.Customer)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (payment == null)
                {
                    _logger.LogWarning("Payment with ID {PaymentId} not found", id);
                }
                else
                {
                    _logger.LogDebug("Successfully retrieved payment {PaymentId} for ticket {TicketId}", id, payment.TicketId);
                }

                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment with ID {PaymentId}", id);
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
                _logger.LogInformation("Creating new payment for ticket {TicketId}, customer {CustomerId}, amount {Amount:C}",
                    payment.TicketId, payment.CustomerId, payment.Amount);

                payment.TransactionId = GenerateTransactionId();
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created payment {PaymentId} with transaction ID {TransactionId}",
                    payment.Id, payment.TransactionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment for ticket {TicketId}, customer {CustomerId}",
                    payment.TicketId, payment.CustomerId);
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
                _logger.LogDebug("Updating payment {PaymentId} status to {Status}", paymentId, status);
                var payment = await _context.Payments.FindAsync(paymentId);
                if (payment != null)
                {
                    payment.Status = status;
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully updated payment {PaymentId} status to {Status}", paymentId, status);
                    return true;
                }
                _logger.LogWarning("Payment {PaymentId} not found when updating status", paymentId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment {PaymentId} status to {Status}", paymentId, status);
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