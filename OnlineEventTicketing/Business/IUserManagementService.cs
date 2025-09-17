using Microsoft.AspNetCore.Identity;
using OnlineEventTicketing.Data.Entity;
using OnlineEventTicketing.Models.ViewModels;

namespace OnlineEventTicketing.Business
{
    public interface IUserManagementService
    {
        Task<IEnumerable<UserDisplayViewModel>> GetAllUsersAsync();
        Task<UserDisplayViewModel?> GetUserByIdAsync(string userId);
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
        Task<IEnumerable<IdentityRole>> GetAllRolesAsync();
        Task<bool> AssignRoleToUserAsync(string userId, string role);
        Task<bool> RemoveRoleFromUserAsync(string userId, string role);
        Task<bool> CreateUserAsync(CreateUserViewModel model);
        Task<bool> UpdateUserAsync(string userId, EditUserViewModel model);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> ToggleUserStatusAsync(string userId);
        Task<UserStatsViewModel> GetUserStatsAsync();
    }
}