using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OnlineEventTicketing.Models;
using OnlineEventTicketing.Business;
using OnlineEventTicketing.Models.ViewModels;

namespace OnlineEventTicketing.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEventService _eventService;

    public HomeController(ILogger<HomeController> logger, IEventService eventService)
    {
        _logger = logger;
        _eventService = eventService;
    }

    public async Task<IActionResult> Index()
    {
        // Redirect authenticated users to their appropriate dashboards
        if (User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            else if (User.IsInRole("EventOrganizer"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Organizer" });
            }
            else if (User.IsInRole("Customer"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Customer" });
            }
        }

        // Show featured events for non-authenticated users
        var upcomingEvents = await _eventService.GetUpcomingEventsAsync();
        var featuredEvents = upcomingEvents.Take(6).Select(e => new EventDisplayViewModel
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Date = e.Date,
            Location = e.Location,
            Category = e.Category,
            Capacity = e.Capacity,
            AvailableTickets = e.AvailableTickets,
            BasePrice = e.BasePrice,
            ImageUrl = e.ImageUrl,
            IsActive = e.IsActive,
            OrganizerId = e.OrganizerId,
            OrganizerName = e.Organizer != null ? $"{e.Organizer.FirstName} {e.Organizer.LastName}" : "Unknown",
            CreatedAt = e.CreatedAt
        }).ToList();

        return View(featuredEvents);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
