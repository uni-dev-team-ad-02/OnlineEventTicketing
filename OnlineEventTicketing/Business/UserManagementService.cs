using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineEventTicketing.Data;
using OnlineEventTicketing.Data.Entity;
using OnlineEventTicketing.Models.ViewModels;

namespace OnlineEventTicketing.Business
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IEnumerable<UserDisplayViewModel>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserDisplayViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var totalTickets = await _context.Tickets.CountAsync(t => t.CustomerId == user.Id);
                var totalSpent = await _context.Tickets
                    .Where(t => t.CustomerId == user.Id)
                    .SumAsync(t => t.Price);

                userViewModels.Add(new UserDisplayViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnd = user.LockoutEnd,
                    IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.Now,
                    LoyaltyPoints = user.LoyaltyPoints,
                    Roles = roles.ToList(),
                    RegistrationDate = user.CreatedAt,
                    TotalTicketsPurchased = totalTickets,
                    TotalAmountSpent = totalSpent
                });
            }

            return userViewModels.OrderByDescending(u => u.RegistrationDate);
        }

        public async Task<UserDisplayViewModel?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            var totalTickets = await _context.Tickets.CountAsync(t => t.CustomerId == user.Id);
            var totalSpent = await _context.Tickets
                .Where(t => t.CustomerId == user.Id)
                .SumAsync(t => t.Price);

            return new UserDisplayViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnd = user.LockoutEnd,
                IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.Now,
                LoyaltyPoints = user.LoyaltyPoints,
                Roles = roles.ToList(),
                RegistrationDate = user.CreatedAt,
                TotalTicketsPurchased = totalTickets,
                TotalAmountSpent = totalSpent
            };
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<string>();

            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IEnumerable<IdentityRole>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.ToListAsync();
        }

        public async Task<bool> AssignRoleToUserAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task<bool> RemoveRoleFromUserAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task<bool> CreateUserAsync(CreateUserViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded && !string.IsNullOrEmpty(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            return result.Succeeded;
        }

        public async Task<bool> UpdateUserAsync(string userId, EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.LoyaltyPoints = model.LoyaltyPoints;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ToggleUserStatusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.Now)
            {
                // Lock the user
                var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                return result.Succeeded;
            }
            else
            {
                // Unlock the user
                var result = await _userManager.SetLockoutEndDateAsync(user, null);
                return result.Succeeded;
            }
        }

        public async Task<UserStatsViewModel> GetUserStatsAsync()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var activeUsers = await _userManager.Users
                .CountAsync(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.Now);

            var customerRole = await _roleManager.FindByNameAsync("Customer");
            var organizerRole = await _roleManager.FindByNameAsync("EventOrganizer");
            var adminRole = await _roleManager.FindByNameAsync("Admin");

            var totalCustomers = customerRole != null ? 
                await _context.UserRoles.CountAsync(ur => ur.RoleId == customerRole.Id) : 0;
            var totalOrganizers = organizerRole != null ? 
                await _context.UserRoles.CountAsync(ur => ur.RoleId == organizerRole.Id) : 0;
            var totalAdmins = adminRole != null ? 
                await _context.UserRoles.CountAsync(ur => ur.RoleId == adminRole.Id) : 0;

            return new UserStatsViewModel
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = totalUsers - activeUsers,
                TotalCustomers = totalCustomers,
                TotalOrganizers = totalOrganizers,
                TotalAdmins = totalAdmins
            };
        }
    }
}