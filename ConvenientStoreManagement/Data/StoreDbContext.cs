using Microsoft.EntityFrameworkCore;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Data
{
    public class StoreDbContext : DbContext
    {
        public StoreDbContext(DbContextOptions<StoreDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasMany(e => e.Products)
                    .WithOne(p => p.Category)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Price).HasPrecision(10, 2);
                entity.Property(e => e.CostPrice).HasPrecision(10, 2);
                entity.HasMany(e => e.SaleItems)
                    .WithOne(si => si.Product)
                    .HasForeignKey(si => si.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(150);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.HasMany(e => e.Sales)
                    .WithOne(s => s.Customer)
                    .HasForeignKey(s => s.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Sale
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.SaleId);
                entity.Property(e => e.TotalAmount).HasPrecision(12, 2);
                entity.Property(e => e.PaidAmount).HasPrecision(12, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(12, 2);
                entity.HasMany(e => e.SaleItems)
                    .WithOne(si => si.Sale)
                    .HasForeignKey(si => si.SaleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SaleItem
            modelBuilder.Entity<SaleItem>(entity =>
            {
                entity.HasKey(e => e.SaleItemId);
                entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
                entity.Property(e => e.Discount).HasPrecision(10, 2);
                entity.Property(e => e.LineTotal).HasPrecision(12, 2);
            });
        }
    }
}
