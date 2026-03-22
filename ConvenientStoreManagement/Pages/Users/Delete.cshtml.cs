using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Authorization;

namespace ConvenientStoreManagement.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly IUserService _userService;

        public DeleteModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public UserViewModel UserViewModel { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var result = await _userService.GetUsersAsync(null, null, 1, 100);
            var item = result.FirstOrDefault(u => u.UserId == id);
            
            if (item == null)
            {
                return NotFound();
            }

            UserViewModel = item;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            if (id.Value == 1 || User.Identity?.Name == UserViewModel?.Username)
            {
                // Prevent deleting self or primary admin (simple protection)
                TempData["ErrorMessage"] = "Cannot delete this user.";
                return RedirectToPage("./Index");
            }

            var result = await _userService.DeleteUserAsync(id.Value);
            
            if (result)
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete user. They might have existing orders.";
            }
            
            return RedirectToPage("./Index");
        }
    }
}
