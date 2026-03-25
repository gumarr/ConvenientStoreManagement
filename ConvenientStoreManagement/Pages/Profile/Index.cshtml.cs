using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ConvenientStoreManagement.Pages.Profile
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly StoreDbContext _context;

        public IndexModel(StoreDbContext context)
        {
            _context = context;
        }

        public User CurrentUser { get; set; } = null!;
        public bool IsGoogleAccount { get; set; }

        [TempData] public string? ActiveTab { get; set; }
        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        [BindProperty]
        public EditInfoModel EditInfo { get; set; } = new();

        public class EditInfoModel
        {
            [Required(ErrorMessage = "Full name is required.")]
            [StringLength(150, MinimumLength = 2)]
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = string.Empty;
            public string? Email { get; set; }
        }

        [BindProperty]
        public ChangePasswordModel ChangePassword { get; set; } = new();

        public class ChangePasswordModel
        {
            [Display(Name = "Current Password")]
            [DataType(DataType.Password)]
            public string? CurrentPassword { get; set; }

            [Required(ErrorMessage = "New password is required.")]
            [StringLength(256, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
            [Display(Name = "New Password")]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please confirm your new password.")]
            [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
            [Display(Name = "Confirm New Password")]
            [DataType(DataType.Password)]
            public string ConfirmNewPassword { get; set; } = string.Empty;
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private async Task LoadUserAsync()
        {
            CurrentUser = await _context.Users.FindAsync(GetCurrentUserId())
                ?? throw new InvalidOperationException("User not found.");
            IsGoogleAccount = CurrentUser.Password == null;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadUserAsync();
            EditInfo.FullName = CurrentUser.FullName;
            EditInfo.Email = CurrentUser.Email;
            return Page();
        }

        public async Task<IActionResult> OnPostEditInfoAsync()
        {
            await LoadUserAsync();
            ActiveTab = "info";

            ModelState.ClearValidationState(nameof(ChangePassword));
            ChangePassword = new ChangePasswordModel();

            if (!TryValidateModel(EditInfo, nameof(EditInfo)))
                return Page();

            CurrentUser.FullName = EditInfo.FullName;
            await _context.SaveChangesAsync();
            await RefreshClaimsAsync(CurrentUser);

            SuccessMessage = "Profile updated successfully!";
            ActiveTab = "info";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            await LoadUserAsync();
            ActiveTab = "password";

            // Xóa lỗi validation của EditInfo — form ChangePassword không submit các field đó
            foreach (var key in ModelState.Keys
                .Where(k => k == "FullName" || k == "Email" || k.StartsWith("EditInfo"))
                .ToList())
            {
                ModelState.Remove(key);
            }
            EditInfo = new EditInfoModel { FullName = CurrentUser.FullName };

            if (!TryValidateModel(ChangePassword, nameof(ChangePassword)))
                return Page();

            bool wasGoogleAccount = IsGoogleAccount;

            if (!wasGoogleAccount)
            {
                if (string.IsNullOrEmpty(ChangePassword.CurrentPassword) ||
                    CurrentUser.Password != ChangePassword.CurrentPassword)
                {
                    ModelState.AddModelError("ChangePassword.CurrentPassword", "Current password is incorrect.");
                    return Page();
                }
            }

            CurrentUser.Password = ChangePassword.NewPassword;
            await _context.SaveChangesAsync();

            SuccessMessage = wasGoogleAccount
                ? "Password set! You can now log in with username & password."
                : "Password changed successfully!";
            ActiveTab = "password";
            return RedirectToPage();
        }

        private async Task RefreshClaimsAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name,           user.Username),
                new Claim("FullName",                user.FullName),
                new Claim(ClaimTypes.Role,           user.Role),
            };
            if (user.Email != null)
                claims.Add(new Claim(ClaimTypes.Email, user.Email));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProps = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProps);
        }
    }
}
