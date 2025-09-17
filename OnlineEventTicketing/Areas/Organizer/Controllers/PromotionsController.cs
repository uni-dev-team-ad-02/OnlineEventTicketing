using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineEventTicketing.Business;
using OnlineEventTicketing.Data.Entity;
using OnlineEventTicketing.Models.ViewModels;

namespace OnlineEventTicketing.Areas.Organizer.Controllers
{
    [Area("Organizer")]
    [Authorize(Roles = "EventOrganizer")]
    public class PromotionsController : Controller
    {
        private readonly IPromotionService _promotionService;
        private readonly IEventService _eventService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PromotionsController(
            IPromotionService promotionService,
            IEventService eventService,
            UserManager<ApplicationUser> userManager)
        {
            _promotionService = promotionService;
            _eventService = eventService;
            _userManager = userManager;
        }

        // GET: /Organizer/Promotions
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var promotions = await _promotionService.GetPromotionsByOrganizerIdAsync(user.Id);
            var viewModel = promotions.Select(p => new PromotionDisplayViewModel
            {
                Id = p.Id,
                Code = p.Code,
                Description = p.Description,
                DiscountPercentage = p.DiscountPercentage,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive,
                EventId = p.EventId,
                EventTitle = p.Event?.Title ?? "Unknown Event",
                EventDate = p.Event?.Date ?? DateTime.MinValue,
                EventLocation = p.Event?.Location ?? "Unknown Location",
                CreatedAt = p.CreatedAt
            }).ToList();

            return View(viewModel);
        }

        // GET: /Organizer/Promotions/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var isOwned = await _promotionService.IsPromotionOwnedByOrganizerAsync(id, user.Id);
            if (!isOwned)
            {
                return NotFound();
            }

            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }

            var viewModel = new PromotionDisplayViewModel
            {
                Id = promotion.Id,
                Code = promotion.Code,
                Description = promotion.Description,
                DiscountPercentage = promotion.DiscountPercentage,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                IsActive = promotion.IsActive,
                EventId = promotion.EventId,
                EventTitle = promotion.Event?.Title ?? "Unknown Event",
                EventDate = promotion.Event?.Date ?? DateTime.MinValue,
                EventLocation = promotion.Event?.Location ?? "Unknown Location",
                CreatedAt = promotion.CreatedAt
            };

            return View(viewModel);
        }

        // GET: /Organizer/Promotions/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var organizerEvents = await _eventService.GetEventsByOrganizerIdAsync(user.Id);
            var availableEvents = organizerEvents.Where(e => e.IsActive && e.Date > DateTime.Now)
                .Select(e => new EventDisplayViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Date = e.Date,
                    Location = e.Location
                }).ToList();

            var model = new CreatePromotionViewModel
            {
                AvailableEvents = availableEvents,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30)
            };

            return View(model);
        }

        // POST: /Organizer/Promotions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePromotionViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (ModelState.IsValid)
            {
                // Validate that the event belongs to the organizer
                var organizerEvents = await _eventService.GetEventsByOrganizerIdAsync(user.Id);
                if (!organizerEvents.Any(e => e.Id == model.EventId))
                {
                    ModelState.AddModelError("EventId", "You can only create promotions for your own events.");
                }

                // Validate dates
                if (model.StartDate >= model.EndDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after start date.");
                }

                // Check if promotion code already exists for this event
                var existingPromotion = await _promotionService.GetPromotionByCodeAsync(model.Code);
                if (existingPromotion != null && existingPromotion.EventId == model.EventId)
                {
                    ModelState.AddModelError("Code", "A promotion with this code already exists for this event.");
                }

                if (ModelState.IsValid)
                {
                    var promotion = new Promotion
                    {
                        Code = model.Code.ToUpper(),
                        Description = model.Description,
                        DiscountPercentage = model.DiscountPercentage,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        IsActive = model.IsActive,
                        EventId = model.EventId,
                        CreatedAt = DateTime.Now
                    };

                    var success = await _promotionService.CreatePromotionAsync(promotion);
                    if (success)
                    {
                        TempData["Success"] = "Promotion created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Error"] = "Failed to create promotion. Please try again.";
                    }
                }
            }

            // Reload events if model is invalid
            var organizerEventsReload = await _eventService.GetEventsByOrganizerIdAsync(user.Id);
            model.AvailableEvents = organizerEventsReload.Where(e => e.IsActive && e.Date > DateTime.Now)
                .Select(e => new EventDisplayViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Date = e.Date,
                    Location = e.Location
                }).ToList();

            return View(model);
        }

        // GET: /Organizer/Promotions/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var isOwned = await _promotionService.IsPromotionOwnedByOrganizerAsync(id, user.Id);
            if (!isOwned)
            {
                return NotFound();
            }

            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }

            var model = new EditPromotionViewModel
            {
                Id = promotion.Id,
                Code = promotion.Code,
                Description = promotion.Description,
                DiscountPercentage = promotion.DiscountPercentage,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                IsActive = promotion.IsActive,
                EventId = promotion.EventId,
                EventTitle = promotion.Event?.Title ?? "Unknown Event",
                EventDate = promotion.Event?.Date ?? DateTime.MinValue,
                EventLocation = promotion.Event?.Location ?? "Unknown Location"
            };

            return View(model);
        }

        // POST: /Organizer/Promotions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditPromotionViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var isOwned = await _promotionService.IsPromotionOwnedByOrganizerAsync(id, user.Id);
            if (!isOwned)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Validate dates
                if (model.StartDate >= model.EndDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after start date.");
                }

                if (ModelState.IsValid)
                {
                    var promotion = await _promotionService.GetPromotionByIdAsync(id);
                    if (promotion != null)
                    {
                        promotion.Code = model.Code.ToUpper();
                        promotion.Description = model.Description;
                        promotion.DiscountPercentage = model.DiscountPercentage;
                        promotion.StartDate = model.StartDate;
                        promotion.EndDate = model.EndDate;
                        promotion.IsActive = model.IsActive;

                        var success = await _promotionService.UpdatePromotionAsync(promotion);
                        if (success)
                        {
                            TempData["Success"] = "Promotion updated successfully!";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            TempData["Error"] = "Failed to update promotion. Please try again.";
                        }
                    }
                }
            }

            return View(model);
        }

        // POST: /Organizer/Promotions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var isOwned = await _promotionService.IsPromotionOwnedByOrganizerAsync(id, user.Id);
            if (!isOwned)
            {
                return NotFound();
            }

            var success = await _promotionService.DeletePromotionAsync(id);
            if (success)
            {
                TempData["Success"] = "Promotion deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete promotion.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Organizer/Promotions/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var isOwned = await _promotionService.IsPromotionOwnedByOrganizerAsync(id, user.Id);
            if (!isOwned)
            {
                return NotFound();
            }

            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }

            promotion.IsActive = !promotion.IsActive;
            var success = await _promotionService.UpdatePromotionAsync(promotion);

            if (success)
            {
                var status = promotion.IsActive ? "activated" : "deactivated";
                TempData["Success"] = $"Promotion {status} successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to update promotion status.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}