using System.ComponentModel.DataAnnotations;

namespace ConvenientStoreManagement.ViewModels
{
    public class UserInputModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(150, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(256)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Staff";
    }
}
