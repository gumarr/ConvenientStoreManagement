using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.ViewModels
{
    /// <summary>
    /// Data transfer object for the Product Detail page.
    /// Assembled by <see cref="Services.Interfaces.IProductService.GetProductDetailAsync"/>.
    /// </summary>
    public class ProductDetailDto
    {
        /// <summary>The product entity (includes Category navigation property).</summary>
        public Product Product { get; set; } = null!;

        /// <summary>
        /// Latest import price from InventoryReceiptDetails.
        /// 0 when no import records exist.
        /// </summary>
        public decimal AvgImportPrice { get; set; }

        /// <summary>
        /// Active selling price from ProductPrices (EndDate == null).
        /// Null when no active price record exists.
        /// </summary>
        public decimal? CurrentSellingPrice { get; set; }

        /// <summary>True when at least one InventoryReceiptDetail exists for this product.</summary>
        public bool HasImportData => AvgImportPrice > 0;

        /// <summary>True when an active ProductPrice record exists.</summary>
        public bool HasActivePrice => CurrentSellingPrice.HasValue;
    }
}
