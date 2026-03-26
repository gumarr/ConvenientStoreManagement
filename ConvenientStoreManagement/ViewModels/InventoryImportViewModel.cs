using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ConvenientStoreManagement.ViewModels
{
    public class InventoryImportRequest
    {
        public List<InventoryImportItem> Items { get; set; } = new List<InventoryImportItem>();
    }

    public class InventoryImportItem
    {
        public int? ProductId { get; set; }

        public string? Name { get; set; }

        public int? CategoryId { get; set; }

        public string? Unit { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Import Price must be greater than 0")]
        public decimal ImportPrice { get; set; }

        /// <summary>
        /// Optional image upload for NEW products only.
        /// Max 3MB; allowed types: jpg, jpeg, png, webp.
        /// </summary>
        public IFormFile? ImageFile { get; set; }
    }
}
