using Microsoft.EntityFrameworkCore;
using OnlineEventTicketing.Data;
using OnlineEventTicketing.Data.Entity;
using OnlineEventTicketing.Models.ViewModels;

namespace OnlineEventTicketing.Business
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrganizerRevenueReportViewModel> GetOrganizerRevenueReportAsync(string organizerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddMonths(-12);
            endDate ??= DateTime.Now;

            var ticketsQuery = _context.Tickets
                .Include(t => t.Event)
                .Where(t => t.Event.OrganizerId == organizerId && 
                           t.PurchaseDate >= startDate && 
                           t.PurchaseDate <= endDate);

            var totalRevenue = await ticketsQuery.SumAsync(t => t.Price);
            var totalTicketsSold = await ticketsQuery.CountAsync();
            var totalEvents = await _context.Events.CountAsync(e => e.OrganizerId == organizerId);
            var activeEvents = await _context.Events.CountAsync(e => e.OrganizerId == organizerId && e.IsActive);

            // Monthly breakdown
            var monthlyBreakdownRaw = await ticketsQuery
                .GroupBy(t => new { t.PurchaseDate.Year, t.PurchaseDate.Month })
                .Select(g => new 
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(t => t.Price),
                    TicketsSold = g.Count(),
                    EventsCount = g.Select(t => t.EventId).Distinct().Count()
                })
                .OrderBy(m => m.Year).ThenBy(m => m.Month)
                .ToListAsync();

            // Format month names
            var monthlyBreakdown = monthlyBreakdownRaw.Select(m => new MonthlyRevenueViewModel
            {
                Month = new DateTime(m.Year, m.Month, 1).ToString("MMM yyyy"),
                Revenue = m.Revenue,
                TicketsSold = m.TicketsSold,
                EventsCount = m.EventsCount
            }).ToList();

            // Event breakdown
            var eventBreakdown = await _context.Events
                .Where(e => e.OrganizerId == organizerId)
                .Include(e => e.Tickets)
                .ToListAsync();

            var eventBreakdownResult = eventBreakdown.Select(e => new EventRevenueViewModel
            {
                EventId = e.Id,
                EventTitle = e.Title,
                EventDate = e.Date,
                Revenue = e.Tickets.Where(t => t.PurchaseDate >= startDate && t.PurchaseDate <= endDate).Sum(t => t.Price),
                TicketsSold = e.Tickets.Where(t => t.PurchaseDate >= startDate && t.PurchaseDate <= endDate).Count(),
                Capacity = e.Capacity
            }).OrderByDescending(e => e.Revenue).ToList();

            // Category performance
            var eventsWithTickets = await _context.Events
                .Where(e => e.OrganizerId == organizerId)
                .Include(e => e.Tickets)
                .ToListAsync();

            var categoryPerformance = eventsWithTickets
                .GroupBy(e => e.Category)
                .Select(g => new CategoryPerformanceViewModel
                {
                    Category = g.Key,
                    Revenue = g.SelectMany(e => e.Tickets).Where(t => t.PurchaseDate >= startDate && t.PurchaseDate <= endDate).Sum(t => t.Price),
                    TicketsSold = g.SelectMany(e => e.Tickets).Where(t => t.PurchaseDate >= startDate && t.PurchaseDate <= endDate).Count(),
                    EventsCount = g.Count()
                })
                .OrderByDescending(c => c.Revenue)
                .ToList();

            return new OrganizerRevenueReportViewModel
            {
                TotalRevenue = totalRevenue,
                TotalTicketsSold = totalTicketsSold,
                TotalEvents = totalEvents,
                ActiveEvents = activeEvents,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                MonthlyBreakdown = monthlyBreakdown,
                EventBreakdown = eventBreakdownResult,
                CategoryPerformance = categoryPerformance
            };
        }

        public async Task<OrganizerSalesReportViewModel> GetOrganizerSalesReportAsync(string organizerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddMonths(-3);
            endDate ??= DateTime.Now;

            var ticketsQuery = _context.Tickets
                .Include(t => t.Event)
                .Where(t => t.Event.OrganizerId == organizerId && 
                           t.PurchaseDate >= startDate && 
                           t.PurchaseDate <= endDate);

            var totalTicketsSold = await ticketsQuery.CountAsync();
            var totalRevenue = await ticketsQuery.SumAsync(t => t.Price);
            var totalEvents = await _context.Events.CountAsync(e => e.OrganizerId == organizerId);
            var activeTickets = await ticketsQuery.CountAsync(t => t.Status == TicketStatus.Active);
            var cancelledTickets = await ticketsQuery.CountAsync(t => t.Status == TicketStatus.Cancelled);
            var refundedTickets = await ticketsQuery.CountAsync(t => t.Status == TicketStatus.Refunded);

            // Daily sales
            var dailySales = await ticketsQuery
                .GroupBy(t => t.PurchaseDate.Date)
                .Select(g => new DailySalesViewModel
                {
                    Date = g.Key,
                    TicketsSold = g.Count(),
                    Revenue = g.Sum(t => t.Price)
                })
                .OrderBy(d => d.Date)
                .ToListAsync();

            // Event sales performance
            var eventSalesPerformanceData = await _context.Events
                .Where(e => e.OrganizerId == organizerId)
                .Include(e => e.Tickets)
                .ToListAsync();

            var eventSalesPerformance = eventSalesPerformanceData.Select(e => new EventSalesPerformanceViewModel
            {
                EventId = e.Id,
                EventTitle = e.Title,
                EventDate = e.Date,
                TicketsSold = e.Tickets.Where(t => t.PurchaseDate >= startDate && t.PurchaseDate <= endDate).Count(),
                Capacity = e.Capacity,
                Revenue = e.Tickets.Where(t => t.PurchaseDate >= startDate && t.PurchaseDate <= endDate).Sum(t => t.Price)
            }).OrderByDescending(e => e.TicketsSold).ToList();

            // Peak sales days (top 5)
            var peakSalesDays = dailySales
                .OrderByDescending(d => d.TicketsSold)
                .Take(5)
                .Select(d => new PeakSalesDayViewModel
                {
                    Date = d.Date,
                    TicketsSold = d.TicketsSold,
                    Revenue = d.Revenue,
                    Reason = "High ticket sales volume"
                })
                .ToList();

            return new OrganizerSalesReportViewModel
            {
                TotalTicketsSold = totalTicketsSold,
                TotalRevenue = totalRevenue,
                TotalEvents = totalEvents,
                ActiveTickets = activeTickets,
                CancelledTickets = cancelledTickets,
                RefundedTickets = refundedTickets,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                DailySales = dailySales,
                EventSalesPerformance = eventSalesPerformance,
                PeakSalesDays = peakSalesDays
            };
        }

        public async Task<EventPerformanceReportViewModel> GetEventPerformanceReportAsync(int eventId, string organizerId)
        {
            var eventItem = await _context.Events
                .Include(e => e.Tickets)
                .FirstOrDefaultAsync(e => e.Id == eventId && e.OrganizerId == organizerId);

            if (eventItem == null)
                return new EventPerformanceReportViewModel();

            var totalRevenue = eventItem.Tickets.Sum(t => t.Price);
            var ticketsSold = eventItem.Tickets.Count();
            var activeTickets = eventItem.Tickets.Count(t => t.Status == TicketStatus.Active);
            var cancelledTickets = eventItem.Tickets.Count(t => t.Status == TicketStatus.Cancelled);
            var refundedTickets = eventItem.Tickets.Count(t => t.Status == TicketStatus.Refunded);

            // Sales timeline
            var salesTimeline = eventItem.Tickets
                .GroupBy(t => t.PurchaseDate.Date)
                .Select(g => new SalesTimelineViewModel
                {
                    Date = g.Key,
                    TicketsSold = g.Count(),
                    Revenue = g.Sum(t => t.Price)
                })
                .OrderBy(s => s.Date)
                .ToList();

            // Promotions used (simulated - would need promotion usage tracking)
            var promotionsUsed = new List<PromotionUsageViewModel>();
            var eventPromotions = await _context.Promotions.Where(p => p.EventId == eventId).ToListAsync();
            foreach (var promotion in eventPromotions)
            {
                promotionsUsed.Add(new PromotionUsageViewModel
                {
                    PromotionCode = promotion.Code,
                    DiscountPercentage = promotion.DiscountPercentage,
                    UsageCount = 0, // Would need to track actual usage
                    TotalDiscountGiven = 0 // Would calculate based on usage
                });
            }

            return new EventPerformanceReportViewModel
            {
                EventId = eventItem.Id,
                EventTitle = eventItem.Title,
                EventDate = eventItem.Date,
                Location = eventItem.Location,
                Category = eventItem.Category,
                BasePrice = eventItem.BasePrice,
                Capacity = eventItem.Capacity,
                TicketsSold = ticketsSold,
                AvailableTickets = eventItem.AvailableTickets,
                TotalRevenue = totalRevenue,
                ActiveTickets = activeTickets,
                CancelledTickets = cancelledTickets,
                RefundedTickets = refundedTickets,
                SalesTimeline = salesTimeline,
                PromotionsUsed = promotionsUsed
            };
        }

        public async Task<IEnumerable<EventSalesReportViewModel>> GetOrganizerEventSalesAsync(string organizerId)
        {
            var events = await _context.Events
                .Where(e => e.OrganizerId == organizerId)
                .Include(e => e.Tickets)
                .ToListAsync();

            return events.Select(e => new EventSalesReportViewModel
            {
                EventId = e.Id,
                EventTitle = e.Title,
                EventDate = e.Date,
                Location = e.Location,
                Category = e.Category,
                Capacity = e.Capacity,
                TicketsSold = e.Tickets.Count(),
                Revenue = e.Tickets.Sum(t => t.Price),
                IsActive = e.IsActive
            }).OrderByDescending(e => e.Revenue).ToList();
        }

        public async Task<PromotionEffectivenessReportViewModel> GetPromotionEffectivenessReportAsync(string organizerId)
        {
            var promotions = await _context.Promotions
                .Include(p => p.Event)
                .Where(p => p.Event.OrganizerId == organizerId)
                .ToListAsync();

            var totalPromotions = promotions.Count;
            var activePromotions = promotions.Count(p => p.IsActive);
            var totalDiscountGiven = 0m; // Would need to track actual usage
            var totalPromotionUsage = 0; // Would need to track actual usage
            var averageDiscount = promotions.Any() ? promotions.Average(p => p.DiscountPercentage) : 0;

            var promotionDetails = promotions.Select(p => new PromotionDetailViewModel
            {
                PromotionId = p.Id,
                Code = p.Code,
                EventTitle = p.Event?.Title ?? "Unknown",
                DiscountPercentage = p.DiscountPercentage,
                UsageCount = 0, // Would need to track actual usage
                TotalDiscountGiven = 0, // Would calculate based on usage
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive
            }).ToList();

            return new PromotionEffectivenessReportViewModel
            {
                TotalPromotions = totalPromotions,
                ActivePromotions = activePromotions,
                TotalDiscountGiven = totalDiscountGiven,
                TotalPromotionUsage = totalPromotionUsage,
                AverageDiscount = averageDiscount,
                PromotionDetails = promotionDetails
            };
        }

        // Admin report methods
        public async Task<AdminSalesReportViewModel> GetAdminSalesReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddMonths(-12);
            endDate ??= DateTime.Now;

            var ticketsQuery = _context.Tickets
                .Include(t => t.Event)
                .ThenInclude(e => e.Organizer)
                .Where(t => t.PurchaseDate >= startDate && t.PurchaseDate <= endDate);

            var totalTicketsSold = await ticketsQuery.CountAsync();
            var totalRevenue = await ticketsQuery.SumAsync(t => t.Price);
            var totalEvents = await _context.Events.CountAsync();
            var activeEvents = await _context.Events.CountAsync(e => e.IsActive);
            var totalOrganizers = await _context.Users.CountAsync(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && 
                _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "EventOrganizer")));
            var activeTickets = await ticketsQuery.CountAsync(t => t.Status == TicketStatus.Active);
            var cancelledTickets = await ticketsQuery.CountAsync(t => t.Status == TicketStatus.Cancelled);
            var refundedTickets = await ticketsQuery.CountAsync(t => t.Status == TicketStatus.Refunded);

            // Daily sales
            var dailySalesRaw = await ticketsQuery
                .GroupBy(t => t.PurchaseDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TicketsSold = g.Count(),
                    Revenue = g.Sum(t => t.Price),
                    EventsCount = g.Select(t => t.EventId).Distinct().Count()
                })
                .OrderBy(d => d.Date)
                .ToListAsync();

            var dailySales = dailySalesRaw.Select(d => new AdminDailySalesViewModel
            {
                Date = d.Date,
                TicketsSold = d.TicketsSold,
                Revenue = d.Revenue,
                EventsCount = d.EventsCount
            }).ToList();

            // Top organizers
            var organizerData = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Tickets)
                .GroupBy(e => e.Organizer)
                .ToListAsync();

            var topOrganizers = organizerData
                .Where(g => g.Key != null)
                .Select(g => new TopOrganizerSalesViewModel
                {
                    OrganizerId = g.Key!.Id,
                    OrganizerName = $"{g.Key.FirstName} {g.Key.LastName}",
                    EventsCount = g.Count(),
                    TicketsSold = g.SelectMany(e => e.Tickets).Where(t => t.PurchaseDate >= startDate && t.PurchaseDate <= endDate).Count(),
                    Revenue = g.SelectMany(e => e.Tickets).Where(t => t.PurchaseDate >= startDate && t.PurchaseDate <= endDate).Sum(t => t.Price)
                })
                .OrderByDescending(o => o.Revenue)
                .Take(10)
                .ToList();

            // Category performance
            var categoryData = await _context.Events
                .Include(e => e.Tickets)
                .GroupBy(e => e.Category)
                .ToListAsync();

            var totalRevenueForShare = categoryData.SelectMany(g => g.SelectMany(e => e.Tickets))
                .Where(t => t.PurchaseDate >= startDate && t.PurchaseDate <= endDate).Sum(t => t.Price);

            var categoryPerformance = categoryData.Select(g => new AdminCategoryPerformanceViewModel
            {
                Category = g.Key,
                EventsCount = g.Count(),
                TicketsSold = g.SelectMany(e => e.Tickets).Where(t => t.PurchaseDate >= startDate && t.PurchaseDate <= endDate).Count(),
                Revenue = g.SelectMany(e => e.Tickets).Where(t => t.PurchaseDate >= startDate && t.PurchaseDate <= endDate).Sum(t => t.Price),
                MarketShare = totalRevenueForShare > 0 ? 
                    g.SelectMany(e => e.Tickets).Where(t => t.PurchaseDate >= startDate && t.PurchaseDate <= endDate).Sum(t => t.Price) / totalRevenueForShare * 100 : 0
            }).OrderByDescending(c => c.Revenue).ToList();

            // Monthly trends
            var monthlyTrendsRaw = await ticketsQuery
                .GroupBy(t => new { t.PurchaseDate.Year, t.PurchaseDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TicketsSold = g.Count(),
                    Revenue = g.Sum(t => t.Price),
                    EventsCount = g.Select(t => t.EventId).Distinct().Count(),
                    OrganizersCount = g.Select(t => t.Event.OrganizerId).Distinct().Count()
                })
                .OrderBy(m => m.Year).ThenBy(m => m.Month)
                .ToListAsync();

            var monthlyTrends = monthlyTrendsRaw.Select(m => new AdminMonthlySalesViewModel
            {
                Month = new DateTime(m.Year, m.Month, 1).ToString("MMM yyyy"),
                TicketsSold = m.TicketsSold,
                Revenue = m.Revenue,
                EventsCount = m.EventsCount,
                OrganizersCount = m.OrganizersCount
            }).ToList();

            return new AdminSalesReportViewModel
            {
                TotalTicketsSold = totalTicketsSold,
                TotalRevenue = totalRevenue,
                TotalEvents = totalEvents,
                ActiveEvents = activeEvents,
                TotalOrganizers = totalOrganizers,
                ActiveTickets = activeTickets,
                CancelledTickets = cancelledTickets,
                RefundedTickets = refundedTickets,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                DailySales = dailySales,
                TopOrganizers = topOrganizers,
                CategoryPerformance = categoryPerformance,
                MonthlyTrends = monthlyTrends
            };
        }

        public async Task<AdminUsersReportViewModel> GetAdminUsersReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddMonths(-12);
            endDate ??= DateTime.Now;

            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.Now);
            var inactiveUsers = totalUsers - activeUsers;

            // Role counts
            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
            var organizerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "EventOrganizer");
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");

            var totalCustomers = customerRole != null ? await _context.UserRoles.CountAsync(ur => ur.RoleId == customerRole.Id) : 0;
            var totalOrganizers = organizerRole != null ? await _context.UserRoles.CountAsync(ur => ur.RoleId == organizerRole.Id) : 0;
            var totalAdmins = adminRole != null ? await _context.UserRoles.CountAsync(ur => ur.RoleId == adminRole.Id) : 0;

            var newUsersThisMonth = await _context.Users
                .CountAsync(u => u.CreatedAt >= DateTime.Now.AddMonths(-1));

            // Registration trends
            var registrationTrendsRaw = await _context.Users
                .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    NewUsers = g.Count()
                })
                .OrderBy(m => m.Year).ThenBy(m => m.Month)
                .ToListAsync();

            var registrationTrends = registrationTrendsRaw.Select(r => new UserRegistrationTrendViewModel
            {
                Period = new DateTime(r.Year, r.Month, 1).ToString("MMM yyyy"),
                NewUsers = r.NewUsers,
                TotalUsers = totalUsers, // Simplified - could calculate running total
                GrowthRate = 0 // Simplified - could calculate actual growth rate
            }).ToList();

            // Role distribution
            var roleDistribution = new List<RoleDistributionViewModel>
            {
                new() { Role = "Customer", Count = totalCustomers, Percentage = totalUsers > 0 ? (decimal)totalCustomers / totalUsers * 100 : 0 },
                new() { Role = "EventOrganizer", Count = totalOrganizers, Percentage = totalUsers > 0 ? (decimal)totalOrganizers / totalUsers * 100 : 0 },
                new() { Role = "Admin", Count = totalAdmins, Percentage = totalUsers > 0 ? (decimal)totalAdmins / totalUsers * 100 : 0 }
            };

            // Top customers
            var topCustomersData = await _context.Users
                .Include(u => u.Tickets)
                .Where(u => customerRole != null && _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == customerRole.Id))
                .ToListAsync();

            var topCustomers = topCustomersData
                .Select(u => new TopCustomerViewModel
                {
                    UserId = u.Id,
                    UserName = $"{u.FirstName} {u.LastName}",
                    Email = u.Email ?? "",
                    TicketsPurchased = u.Tickets.Count,
                    TotalSpent = u.Tickets.Sum(t => t.Price),
                    LoyaltyPoints = u.LoyaltyPoints,
                    RegistrationDate = u.CreatedAt
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(10)
                .ToList();

            // Recent users
            var recentUsersData = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .ToListAsync();

            var recentUsers = new List<RecentUserViewModel>();
            foreach (var user in recentUsersData)
            {
                var userRoles = await _context.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync();
                var roleNames = await _context.Roles.Where(r => userRoles.Select(ur => ur.RoleId).Contains(r.Id)).Select(r => r.Name).ToListAsync();
                
                recentUsers.Add(new RecentUserViewModel
                {
                    UserId = user.Id,
                    UserName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email ?? "",
                    Role = roleNames.FirstOrDefault() ?? "Unknown",
                    RegistrationDate = user.CreatedAt,
                    IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.Now
                });
            }

            return new AdminUsersReportViewModel
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = inactiveUsers,
                TotalCustomers = totalCustomers,
                TotalOrganizers = totalOrganizers,
                TotalAdmins = totalAdmins,
                NewUsersThisMonth = newUsersThisMonth,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                RegistrationTrends = registrationTrends,
                RoleDistribution = roleDistribution,
                TopCustomers = topCustomers,
                RecentUsers = recentUsers
            };
        }

        public async Task<AdminEventsReportViewModel> GetAdminEventsReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddMonths(-12);
            endDate ??= DateTime.Now;

            var totalEvents = await _context.Events.CountAsync();
            var activeEvents = await _context.Events.CountAsync(e => e.IsActive);
            var inactiveEvents = totalEvents - activeEvents;
            var upcomingEvents = await _context.Events.CountAsync(e => e.Date > DateTime.Now);
            var pastEvents = await _context.Events.CountAsync(e => e.Date <= DateTime.Now);
            var totalCapacity = await _context.Events.SumAsync(e => e.Capacity);
            var totalTicketsSold = await _context.Tickets.CountAsync();

            // Top performing events
            var topEventsData = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Tickets)
                .ToListAsync();

            var topPerformingEvents = topEventsData
                .Select(e => new TopPerformingEventViewModel
                {
                    EventId = e.Id,
                    EventTitle = e.Title,
                    OrganizerName = e.Organizer != null ? $"{e.Organizer.FirstName} {e.Organizer.LastName}" : "Unknown",
                    EventDate = e.Date,
                    Location = e.Location,
                    Category = e.Category,
                    Capacity = e.Capacity,
                    TicketsSold = e.Tickets.Count,
                    Revenue = e.Tickets.Sum(t => t.Price)
                })
                .OrderByDescending(e => e.Revenue)
                .Take(10)
                .ToList();

            // Category analysis
            var categoryData = await _context.Events
                .Include(e => e.Tickets)
                .GroupBy(e => e.Category)
                .ToListAsync();

            var totalRevenueForShare = categoryData.SelectMany(g => g.SelectMany(e => e.Tickets)).Sum(t => t.Price);

            var categoryAnalysis = categoryData.Select(g => new EventCategoryAnalysisViewModel
            {
                Category = g.Key,
                EventCount = g.Count(),
                TotalCapacity = g.Sum(e => e.Capacity),
                TicketsSold = g.SelectMany(e => e.Tickets).Count(),
                Revenue = g.SelectMany(e => e.Tickets).Sum(t => t.Price),
                MarketShare = totalRevenueForShare > 0 ? g.SelectMany(e => e.Tickets).Sum(t => t.Price) / totalRevenueForShare * 100 : 0
            }).OrderByDescending(c => c.Revenue).ToList();

            // Recent events
            var recentEventsData = await _context.Events
                .Include(e => e.Organizer)
                .OrderByDescending(e => e.CreatedAt)
                .Take(10)
                .ToListAsync();

            var recentEvents = recentEventsData.Select(e => new RecentEventViewModel
            {
                EventId = e.Id,
                EventTitle = e.Title,
                OrganizerName = e.Organizer != null ? $"{e.Organizer.FirstName} {e.Organizer.LastName}" : "Unknown",
                EventDate = e.Date,
                Category = e.Category,
                IsActive = e.IsActive,
                TicketsSold = e.Tickets?.Count ?? 0,
                Capacity = e.Capacity,
                CreatedAt = e.CreatedAt
            }).ToList();

            return new AdminEventsReportViewModel
            {
                TotalEvents = totalEvents,
                ActiveEvents = activeEvents,
                InactiveEvents = inactiveEvents,
                UpcomingEvents = upcomingEvents,
                PastEvents = pastEvents,
                TotalCapacity = totalCapacity,
                TotalTicketsSold = totalTicketsSold,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TopPerformingEvents = topPerformingEvents,
                CategoryAnalysis = categoryAnalysis,
                RecentEvents = recentEvents
            };
        }

        // Legacy stub methods - keeping for backward compatibility
        public async Task<SystemOverviewReportViewModel> GetSystemOverviewReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            await Task.CompletedTask;
            return new SystemOverviewReportViewModel();
        }

        public async Task<AdminRevenueReportViewModel> GetAdminRevenueReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            await Task.CompletedTask;
            return new AdminRevenueReportViewModel();
        }

        public async Task<UserActivityReportViewModel> GetUserActivityReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            await Task.CompletedTask;
            return new UserActivityReportViewModel();
        }

        public async Task<TopPerformingEventsReportViewModel> GetTopPerformingEventsReportAsync(int topCount = 10)
        {
            await Task.CompletedTask;
            return new TopPerformingEventsReportViewModel();
        }

        public async Task<TopOrganizersReportViewModel> GetTopOrganizersReportAsync(int topCount = 10)
        {
            await Task.CompletedTask;
            return new TopOrganizersReportViewModel();
        }
    }

    // Stub ViewModels for Admin reports
    public class SystemOverviewReportViewModel { }
    public class AdminRevenueReportViewModel { }
    public class UserActivityReportViewModel { }
    public class TopPerformingEventsReportViewModel { }
    public class TopOrganizersReportViewModel { }
}