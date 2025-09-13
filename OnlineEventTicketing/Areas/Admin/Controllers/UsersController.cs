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
    public class UsersController : Controller
    {
        private readonly IUserManagementService _userManagementService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(IUserManagementService userManagementService, UserManager<ApplicationUser> userManager)
        {
            _userManagementService = userManagementService;
            _userManager = userManager;
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManagementService.GetAllUsersAsync();
            var stats = await _userManagementService.GetUserStatsAsync();
            
            ViewBag.UserStats = stats;
            return View(users);
        }

        // GET: /Admin/Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: /Admin/Users/Create
        public async Task<IActionResult> Create()
        {
            var roles = await _userManagementService.GetAllRolesAsync();
            ViewBag.Roles = roles.Select(r => r.Name).ToList();
            return View();
        }

        // POST: /Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _userManagementService.CreateUserAsync(model);
                if (success)
                {
                    TempData["Success"] = "User created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Failed to create user. Please check the details and try again.";
                }
            }

            var roles = await _userManagementService.GetAllRolesAsync();
            ViewBag.Roles = roles.Select(r => r.Name).ToList();
            return View(model);
        }

        // GET: /Admin/Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var allRoles = await _userManagementService.GetAllRolesAsync();
            var userRoles = await _userManagementService.GetUserRolesAsync(id);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                LoyaltyPoints = user.LoyaltyPoints,
                Email = user.Email,
                CurrentRoles = userRoles.ToList(),
                AvailableRoles = allRoles.Select(r => r.Name!).ToList(),
                SelectedRoles = userRoles.ToList()
            };

            return View(model);
        }

        // POST: /Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var success = await _userManagementService.UpdateUserAsync(id, model);
                if (success)
                {
                    // Update roles
                    var currentRoles = await _userManagementService.GetUserRolesAsync(id);
                    var selectedRoles = model.SelectedRoles ?? new List<string>();

                    // Remove roles that are no longer selected
                    foreach (var role in currentRoles)
                    {
                        if (!selectedRoles.Contains(role))
                        {
                            await _userManagementService.RemoveRoleFromUserAsync(id, role);
                        }
                    }

                    // Add new roles
                    foreach (var role in selectedRoles)
                    {
                        if (!currentRoles.Contains(role))
                        {
                            await _userManagementService.AssignRoleToUserAsync(id, role);
                        }
                    }

                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Failed to update user. Please try again.";
                }
            }

            var allRoles = await _userManagementService.GetAllRolesAsync();
            var userRoles = await _userManagementService.GetUserRolesAsync(id);
            
            model.CurrentRoles = userRoles.ToList();
            model.AvailableRoles = allRoles.Select(r => r.Name!).ToList();
            
            return View(model);
        }

        // POST: /Admin/Users/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Prevent admin from locking themselves out
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["Error"] = "You cannot change your own status.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var success = await _userManagementService.ToggleUserStatusAsync(id);
            if (success)
            {
                var status = user.IsActive ? "deactivated" : "activated";
                TempData["Success"] = $"User {status} successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to update user status.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Prevent admin from deleting themselves
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var success = await _userManagementService.DeleteUserAsync(id);
            if (success)
            {
                TempData["Success"] = "User deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete user.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Users/AssignRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                return BadRequest();
            }

            var success = await _userManagementService.AssignRoleToUserAsync(userId, role);
            if (success)
            {
                TempData["Success"] = $"Role '{role}' assigned successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to assign role.";
            }

            return RedirectToAction(nameof(Details), new { id = userId });
        }

        // POST: /Admin/Users/RemoveRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                return BadRequest();
            }

            var success = await _userManagementService.RemoveRoleFromUserAsync(userId, role);
            if (success)
            {
                TempData["Success"] = $"Role '{role}' removed successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to remove role.";
            }

            return RedirectToAction(nameof(Details), new { id = userId });
        }
    }
}