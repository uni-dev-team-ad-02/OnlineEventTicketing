using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Repository
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task<Payment?> GetPaymentByIdAsync(int id);
        Task<IEnumerable<Payment>> GetPaymentsByCustomerIdAsync(string customerId);
        Task<IEnumerable<Payment>> GetPaymentsByTicketIdAsync(int ticketId);
        Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId);
        Task<bool> CreatePaymentAsync(Payment payment);
        Task<bool> UpdatePaymentAsync(Payment payment);
        Task<bool> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status);
        Task<bool> DeletePaymentAsync(int id);
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetRevenueByOrganizerAsync(string organizerId);
    }
}