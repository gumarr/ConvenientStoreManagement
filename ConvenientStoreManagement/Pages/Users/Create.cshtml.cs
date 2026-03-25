using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace ConvenientStoreManagement.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IUserService _userService;

        public CreateModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public UserInputModel Input { get; set; } = new UserInputModel();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (await _userService.UsernameExistsAsync(Input.Username))
            {
                ModelState.AddModelError("Input.Username", "Username is already taken.");
                return Page();
            }

            var result = await _userService.CreateUserAsync(Input);
            if (result)
            {
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToPage("./Index");
            }

            ModelState.AddModelError(string.Empty, "Failed to create user.");
            return Page();
        }
    }
}
