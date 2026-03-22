using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Authorization;

namespace ConvenientStoreManagement.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IUserService _userService;

        public EditModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public UserInputModel Input { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userService.GetUserForEditAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            Input = user;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (await _userService.UsernameExistsAsync(Input.Username, Input.UserId))
            {
                ModelState.AddModelError("Input.Username", "Username is already taken by another user.");
                return Page();
            }

            var result = await _userService.UpdateUserAsync(Input);
            if (result)
            {
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToPage("./Index");
            }

            ModelState.AddModelError(string.Empty, "Failed to update user.");
            return Page();
        }
    }
}
