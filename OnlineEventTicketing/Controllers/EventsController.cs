using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineEventTicketing.Business;
using OnlineEventTicketing.Data.Entity;
using OnlineEventTicketing.Models.ViewModels;

namespace OnlineEventTicketing.Controllers
{
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IEventService eventService, UserManager<ApplicationUser> userManager, ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var events = await _eventService.GetUpcomingEventsAsync();
            var viewModel = events.Select(e => new EventDisplayViewModel
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
                OrganizerName = $"{e.Organizer.FirstName} {e.Organizer.LastName}",
                CreatedAt = e.CreatedAt
            }).ToList();

            return View(viewModel);
        }

        // GET: Events/Search
        public async Task<IActionResult> Search(EventSearchViewModel model)
        {
            if (ModelState.IsValid)
            {
                var events = await _eventService.SearchEventsAsync(
                    model.Category, 
                    model.Date, 
                    model.Location, 
                    model.SearchTerm);

                model.Results = events.Select(e => new EventDisplayViewModel
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
                    OrganizerName = $"{e.Organizer.FirstName} {e.Organizer.LastName}",
                    CreatedAt = e.CreatedAt
                }).ToList();
            }

            return View(model);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogDebug("Displaying event details for event {EventId}", id);
                var eventItem = await _eventService.GetEventByIdAsync(id);
                if (eventItem == null)
                {
                    _logger.LogWarning("Event {EventId} not found", id);
                    return NotFound();
                }

                var viewModel = new EventDisplayViewModel
                {
                    Id = eventItem.Id,
                    Title = eventItem.Title,
                    Description = eventItem.Description,
                    Date = eventItem.Date,
                    Location = eventItem.Location,
                    Category = eventItem.Category,
                    Capacity = eventItem.Capacity,
                    AvailableTickets = eventItem.AvailableTickets,
                    BasePrice = eventItem.BasePrice,
                    ImageUrl = eventItem.ImageUrl,
                    IsActive = eventItem.IsActive,
                    OrganizerId = eventItem.OrganizerId,
                    OrganizerName = $"{eventItem.Organizer.FirstName} {eventItem.Organizer.LastName}",
                    CreatedAt = eventItem.CreatedAt
                };

                _logger.LogInformation("Successfully displayed event details for {EventTitle} (ID: {EventId})", eventItem.Title, id);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error displaying event details for event {EventId}", id);
                return StatusCode(500, "An error occurred while retrieving event details.");
            }
        }

        // GET: Events/Create
        [Authorize(Roles = "EventOrganizer")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "EventOrganizer")]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        _logger.LogWarning("User not found when creating event");
                        return RedirectToAction("Login", "Account");
                    }

                    _logger.LogInformation("Creating new event {EventTitle} by organizer {OrganizerId}", model.Title, user.Id);

                    var eventItem = new Event
                    {
                        Title = model.Title,
                        Description = model.Description,
                        Date = model.Date,
                        Location = model.Location,
                        Category = model.Category,
                        Capacity = model.Capacity,
                        BasePrice = model.BasePrice,
                        ImageUrl = model.ImageUrl,
                        IsActive = model.IsActive,
                        OrganizerId = user.Id
                    };

                    var success = await _eventService.CreateEventAsync(eventItem);
                    if (success)
                    {
                        _logger.LogInformation("Successfully created event {EventTitle} (ID: {EventId})", model.Title, eventItem.Id);
                        TempData["Success"] = "Event created successfully!";
                        return RedirectToAction(nameof(MyEvents));
                    }
                    else
                    {
                        _logger.LogWarning("Failed to create event {EventTitle}", model.Title);
                        ModelState.AddModelError("", "Failed to create event. Please try again.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event {EventTitle}", model.Title);
                ModelState.AddModelError("", "An error occurred while creating the event. Please try again.");
                return View(model);
            }
        }

        // GET: Events/Edit/5
        [Authorize(Roles = "EventOrganizer")]
        public async Task<IActionResult> Edit(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || eventItem.OrganizerId != user.Id)
            {
                return Forbid();
            }

            var viewModel = new CreateEventViewModel
            {
                Title = eventItem.Title,
                Description = eventItem.Description,
                Date = eventItem.Date,
                Location = eventItem.Location,
                Category = eventItem.Category,
                Capacity = eventItem.Capacity,
                BasePrice = eventItem.BasePrice,
                ImageUrl = eventItem.ImageUrl,
                IsActive = eventItem.IsActive
            };

            return View(viewModel);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "EventOrganizer")]
        public async Task<IActionResult> Edit(int id, CreateEventViewModel model)
        {
            if (ModelState.IsValid)
            {
                var eventItem = await _eventService.GetEventByIdAsync(id);
                if (eventItem == null)
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null || eventItem.OrganizerId != user.Id)
                {
                    return Forbid();
                }

                eventItem.Title = model.Title;
                eventItem.Description = model.Description;
                eventItem.Date = model.Date;
                eventItem.Location = model.Location;
                eventItem.Category = model.Category;
                eventItem.Capacity = model.Capacity;
                eventItem.BasePrice = model.BasePrice;
                eventItem.ImageUrl = model.ImageUrl;
                eventItem.IsActive = model.IsActive;

                var success = await _eventService.UpdateEventAsync(eventItem);
                if (success)
                {
                    TempData["Success"] = "Event updated successfully!";
                    return RedirectToAction(nameof(MyEvents));
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update event. Please try again.");
                }
            }

            return View(model);
        }

        // GET: Events/Delete/5
        [Authorize(Roles = "EventOrganizer")]
        public async Task<IActionResult> Delete(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || eventItem.OrganizerId != user.Id)
            {
                return Forbid();
            }

            var viewModel = new EventDisplayViewModel
            {
                Id = eventItem.Id,
                Title = eventItem.Title,
                Description = eventItem.Description,
                Date = eventItem.Date,
                Location = eventItem.Location,
                Category = eventItem.Category,
                Capacity = eventItem.Capacity,
                AvailableTickets = eventItem.AvailableTickets,
                BasePrice = eventItem.BasePrice,
                ImageUrl = eventItem.ImageUrl,
                IsActive = eventItem.IsActive,
                OrganizerId = eventItem.OrganizerId,
                OrganizerName = $"{eventItem.Organizer.FirstName} {eventItem.Organizer.LastName}",
                CreatedAt = eventItem.CreatedAt
            };

            return View(viewModel);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "EventOrganizer")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || eventItem.OrganizerId != user.Id)
            {
                return Forbid();
            }

            var success = await _eventService.DeleteEventAsync(id);
            if (success)
            {
                TempData["Success"] = "Event deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete event. Please try again.";
            }

            return RedirectToAction(nameof(MyEvents));
        }

        // GET: Events/MyEvents
        [Authorize(Roles = "EventOrganizer")]
        public async Task<IActionResult> MyEvents()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var events = await _eventService.GetEventsByOrganizerIdAsync(user.Id);
            var viewModel = events.Select(e => new EventDisplayViewModel
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
                OrganizerName = $"{e.Organizer.FirstName} {e.Organizer.LastName}",
                CreatedAt = e.CreatedAt
            }).ToList();

            return View(viewModel);
        }
    }
}