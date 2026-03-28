using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ConvenientStoreManagement.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly StoreDbContext _context;

        public UserService(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<UserViewModel>> GetUsersAsync(string? search, string? role, int pageIndex, int pageSize)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Username.Contains(search) || u.FullName.Contains(search));
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.Role == role);
            }

            query = query.OrderByDescending(u => u.CreatedAt);

            var viewModelQuery = query.Select(u => new UserViewModel
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            });

            return await PaginatedList<UserViewModel>.CreateAsync(viewModelQuery.AsNoTracking(), pageIndex, pageSize);
        }

        public async Task<UserInputModel?> GetUserForEditAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            return new UserInputModel
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role
                // Do not load password for editing
            };
        }

        public async Task<bool> CreateUserAsync(UserInputModel input)
        {
            var user = new User
            {
                Username = input.Username,
                FullName = input.FullName,
                Role = input.Role,
                Password = input.Password ?? "123456", // Default password if empty
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateUserAsync(UserInputModel input)
        {
            var user = await _context.Users.FindAsync(input.UserId);
            if (user == null) return false;

            user.Username = input.Username;
            user.FullName = input.FullName;
            user.Role = input.Role;

            // Only update password if provided
            if (!string.IsNullOrEmpty(input.Password))
            {
                user.Password = input.Password;
            }

            // EF Core will automatically track the changes
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            // Optional: Check if user has orders
            var hasOrders = await _context.Orders.AnyAsync(o => o.UserId == id);
            if (hasOrders) return false; // Or handle deletion strategy

            _context.Users.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null)
        {
            if (excludeUserId.HasValue)
            {
                return await _context.Users.AnyAsync(u => u.Username == username && u.UserId != excludeUserId.Value);
            }
            return await _context.Users.AnyAsync(u => u.Username == username);
        }
    }
}
