using System;
using System.Linq;
using System.Threading.Tasks;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ConvenientStoreManagement.Services.Implementations
{
    public class PricingService : IPricingService
    {
        private readonly StoreDbContext _context;

        public PricingService(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetCurrentImportPriceAsync(int productId)
        {
            var latestDetail = await _context.InventoryReceiptDetails
                .Include(d => d.Receipt)
                .Where(d => d.ProductId == productId)
                .OrderByDescending(d => d.Receipt.ImportDate)
                .FirstOrDefaultAsync();

            return latestDetail?.ImportPrice ?? 0m;
        }

        public async Task UpdateSellingPriceAsync(int productId, decimal newSellingPrice)
        {
            var currentPrice = await _context.ProductPrices
                .Where(p => p.ProductId == productId && p.EndDate == null)
                .FirstOrDefaultAsync();

            var now = DateTime.Now;

            if (currentPrice != null)
            {
                currentPrice.EndDate = now;
                _context.ProductPrices.Update(currentPrice);
            }

            var newPrice = new ProductPrice
            {
                ProductId = productId,
                Price = newSellingPrice,
                StartDate = now,
                EndDate = null
            };

            _context.ProductPrices.Add(newPrice);
            await _context.SaveChangesAsync();
        }
    }
}
