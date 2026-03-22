using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.ViewModels;

namespace ConvenientStoreManagement.Services.Interfaces
{
    public interface IUserService
    {
        Task<PaginatedList<UserViewModel>> GetUsersAsync(string? search, string? role, int pageIndex, int pageSize);
        Task<UserInputModel?> GetUserForEditAsync(int id);
        Task<bool> CreateUserAsync(UserInputModel input);
        Task<bool> UpdateUserAsync(UserInputModel input);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null);
    }
}
