using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Authorization;

namespace ConvenientStoreManagement.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;

        public IndexModel(IUserService userService)
        {
            _userService = userService;
        }

        public PaginatedList<UserViewModel> Users { get; set; } = default!;

        public string? CurrentSearch { get; set; }
        public string? CurrentRole { get; set; }

        public async Task OnGetAsync(string? search, string? role, int? pageIndex)
        {
            CurrentSearch = search;
            CurrentRole = role;

            int pageSize = 10;
            int pageNumber = pageIndex ?? 1;

            Users = await _userService.GetUsersAsync(search, role, pageNumber, pageSize);
        }
    }
}
