using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Business
{
    public interface IPaymentService
    {
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task<Payment?> GetPaymentByIdAsync(int id);
        Task<IEnumerable<Payment>> GetPaymentsByCustomerIdAsync(string customerId);
        Task<Payment?> ProcessPaymentAsync(int ticketId, string customerId, PaymentMethod paymentMethod, decimal amount);
        Task<bool> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status);
        Task<bool> ProcessRefundAsync(int ticketId);
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetRevenueByOrganizerAsync(string organizerId);
        Task<bool> ValidatePaymentAsync(string transactionId);
    }
}