using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineEventTicketing.Business;
using OnlineEventTicketing.Data.Entity;
using OnlineEventTicketing.Models.ViewModels;

namespace OnlineEventTicketing.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventsController(IEventService eventService, UserManager<ApplicationUser> userManager)
        {
            _eventService = eventService;
            _userManager = userManager;
        }

        // GET: /Admin/Events
        public async Task<IActionResult> Index()
        {
            var events = await _eventService.GetAllEventsIncludingInactiveAsync();
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
                OrganizerName = e.Organizer != null ? $"{e.Organizer.FirstName} {e.Organizer.LastName}" : "Unknown",
                CreatedAt = e.CreatedAt
            }).OrderByDescending(e => e.CreatedAt).ToList();

            return View(viewModel);
        }

        // POST: /Admin/Events/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            eventItem.IsActive = !eventItem.IsActive;
            var success = await _eventService.UpdateEventAsync(eventItem);

            if (success)
            {
                TempData["Success"] = $"Event {(eventItem.IsActive ? "activated" : "deactivated")} successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to update event status.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _eventService.DeleteEventAsync(id);
            if (success)
            {
                TempData["Success"] = "Event deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete event.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}