using OnlineEventTicketing.Data.Entity;
using OnlineEventTicketing.Repository;

namespace OnlineEventTicketing.Business
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITicketRepository _ticketRepository;

        public PaymentService(IPaymentRepository paymentRepository, ITicketRepository ticketRepository)
        {
            _paymentRepository = paymentRepository;
            _ticketRepository = ticketRepository;
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

        public async Task<Payment?> ProcessPaymentAsync(int ticketId, string customerId, PaymentMethod paymentMethod, decimal amount)
        {
            // Validate ticket exists
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null) return null;

            // Create payment record
            var payment = new Payment
            {
                TicketId = ticketId,
                CustomerId = customerId,
                Amount = amount,
                PaymentMethod = paymentMethod,
                Status = PaymentStatus.Pending,
                PaymentDate = DateTime.UtcNow
            };

            var success = await _paymentRepository.CreatePaymentAsync(payment);
            if (!success) return null;

            // Simulate payment processing
            await Task.Delay(1000); // Simulate processing time

            // Update payment status to completed
            payment.Status = PaymentStatus.Completed;
            await _paymentRepository.UpdatePaymentAsync(payment);

            return payment;
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
    }
}