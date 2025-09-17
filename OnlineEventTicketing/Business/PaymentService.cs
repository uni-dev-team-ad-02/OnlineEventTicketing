using OnlineEventTicketing.Data.Entity;
using OnlineEventTicketing.Repository;

namespace OnlineEventTicketing.Business
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IStripeService _stripeService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IPaymentRepository paymentRepository, ITicketRepository ticketRepository,
            IStripeService stripeService, IConfiguration configuration, ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _ticketRepository = ticketRepository;
            _stripeService = stripeService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _paymentRepository.GetAllPaymentsAsync();
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _paymentRepository.GetPaymentByIdAsync(id);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByCustomerIdAsync(string customerId)
        {
            return await _paymentRepository.GetPaymentsByCustomerIdAsync(customerId);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByTicketIdAsync(int ticketId)
        {
            return await _paymentRepository.GetPaymentsByTicketIdAsync(ticketId);
        }

        public async Task<Payment?> ProcessPaymentAsync(int ticketId, string customerId, PaymentMethod paymentMethod, decimal amount)
        {
            try
            {
                _logger.LogInformation("Processing payment for ticket {TicketId}, customer {CustomerId}, method {PaymentMethod}, amount {Amount:C}",
                    ticketId, customerId, paymentMethod, amount);

                // Validate ticket exists
                var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
                if (ticket == null)
                {
                    _logger.LogWarning("Payment failed: Ticket {TicketId} not found", ticketId);
                    return null;
                }

                // Create payment record
                var payment = new Payment
                {
                    TicketId = ticketId,
                    CustomerId = customerId,
                    Amount = amount,
                    PaymentMethod = paymentMethod,
                    Status = PaymentStatus.Pending,
                    PaymentDate = DateTime.UtcNow,
                    TransactionId = Guid.NewGuid().ToString()
                };

                var success = await _paymentRepository.CreatePaymentAsync(payment);
                if (!success)
                {
                    _logger.LogError("Failed to create payment record for ticket {TicketId}", ticketId);
                    return null;
                }

                // All payments are Stripe-based and start as pending until webhook confirms
                payment.Status = PaymentStatus.Pending;
                await _paymentRepository.UpdatePaymentAsync(payment);

                _logger.LogInformation("Successfully created payment {PaymentId} for ticket {TicketId}",
                    payment.Id, ticketId);

                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for ticket {TicketId}, customer {CustomerId}",
                    ticketId, customerId);
                throw;
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status)
        {
            return await _paymentRepository.UpdatePaymentStatusAsync(paymentId, status);
        }

        public async Task<bool> ProcessRefundAsync(int ticketId)
        {
            var payments = await _paymentRepository.GetPaymentsByTicketIdAsync(ticketId);
            var completedPayment = payments.FirstOrDefault(p => p.Status == PaymentStatus.Completed);

            if (completedPayment == null) return false;

            // Create refund payment record
            var refundPayment = new Payment
            {
                TicketId = ticketId,
                CustomerId = completedPayment.CustomerId,
                Amount = -completedPayment.Amount, // Negative amount for refund
                PaymentMethod = completedPayment.PaymentMethod,
                Status = PaymentStatus.Completed,
                PaymentDate = DateTime.UtcNow
            };

            var success = await _paymentRepository.CreatePaymentAsync(refundPayment);
            if (success)
            {
                // Mark original payment as refunded
                await _paymentRepository.UpdatePaymentStatusAsync(completedPayment.Id, PaymentStatus.Refunded);
            }

            return success;
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _paymentRepository.GetTotalRevenueAsync();
        }

        public async Task<decimal> GetRevenueByOrganizerAsync(string organizerId)
        {
            return await _paymentRepository.GetRevenueByOrganizerAsync(organizerId);
        }

        public async Task<bool> ValidatePaymentAsync(string transactionId)
        {
            var payment = await _paymentRepository.GetPaymentByTransactionIdAsync(transactionId);
            return payment != null && payment.Status == PaymentStatus.Completed;
        }

        public async Task<bool> UpdatePaymentAsync(Payment payment)
        {
            return await _paymentRepository.UpdatePaymentAsync(payment);
        }

        public async Task<Payment?> GetPendingPaymentByCustomerAndAmountAsync(string customerId, decimal amount)
        {
            var payments = await _paymentRepository.GetPaymentsByCustomerIdAsync(customerId);
            return payments.FirstOrDefault(p => p.Status == PaymentStatus.Pending && p.Amount == amount);
        }

        public async Task<Payment?> GetPaymentByStripePaymentIntentIdAsync(string paymentIntentId)
        {
            if (string.IsNullOrEmpty(paymentIntentId)) return null;

            var allPayments = await _paymentRepository.GetAllPaymentsAsync();
            return allPayments.FirstOrDefault(p => p.StripePaymentIntentId == paymentIntentId);
        }
    }
}