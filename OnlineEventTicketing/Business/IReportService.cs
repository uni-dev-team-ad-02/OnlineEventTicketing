using OnlineEventTicketing.Models.ViewModels;

namespace OnlineEventTicketing.Business
{
    public interface IReportService
    {
        // Organizer Reports
        Task<OrganizerRevenueReportViewModel> GetOrganizerRevenueReportAsync(string organizerId, DateTime? startDate = null, DateTime? endDate = null);
        Task<OrganizerSalesReportViewModel> GetOrganizerSalesReportAsync(string organizerId, DateTime? startDate = null, DateTime? endDate = null);
        Task<EventPerformanceReportViewModel> GetEventPerformanceReportAsync(int eventId, string organizerId);
        Task<IEnumerable<EventSalesReportViewModel>> GetOrganizerEventSalesAsync(string organizerId);
        Task<PromotionEffectivenessReportViewModel> GetPromotionEffectivenessReportAsync(string organizerId);
        
        // Admin Reports
        Task<SystemOverviewReportViewModel> GetSystemOverviewReportAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<AdminRevenueReportViewModel> GetAdminRevenueReportAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<UserActivityReportViewModel> GetUserActivityReportAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<TopPerformingEventsReportViewModel> GetTopPerformingEventsReportAsync(int topCount = 10);
        Task<TopOrganizersReportViewModel> GetTopOrganizersReportAsync(int topCount = 10);

        // New Admin Reports
        Task<AdminSalesReportViewModel> GetAdminSalesReportAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<AdminUsersReportViewModel> GetAdminUsersReportAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<AdminEventsReportViewModel> GetAdminEventsReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}