using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ConvenientStoreManagement.ViewModels
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;
        public AllowedExtensionsAttribute(string[] extensions) => _extensions = extensions;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var extension = System.IO.Path.GetExtension(file.FileName);
                if (!_extensions.Contains(extension.ToLower()))
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }
            return ValidationResult.Success;
        }
        public string GetErrorMessage() => $"Only {string.Join(", ", _extensions)} files are allowed.";
    }

    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;
        public MaxFileSizeAttribute(int maxFileSize) => _maxFileSize = maxFileSize;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }
            return ValidationResult.Success;
        }
        public string GetErrorMessage() => $"Maximum allowed file size is { _maxFileSize / (1024 * 1024)} MB.";
    }

    public class ProductInputModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Display(Name = "Product Image")]
        [AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png" })]
        [MaxFileSize(2 * 1024 * 1024)]
        public IFormFile? ImageFile { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active";
    }
}
