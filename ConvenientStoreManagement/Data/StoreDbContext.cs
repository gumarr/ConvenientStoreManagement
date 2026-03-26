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
        public DbSet<AIRecommendation> AIRecommendations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductPrice> ProductPrices { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<InventoryReceipt> InventoryReceipts { get; set; }
        public DbSet<InventoryReceiptDetail> InventoryReceiptDetails { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        // ── New stat tables ─────────────────────────────────────────────────
        public DbSet<DailyProductStats> DailyProductStats { get; set; }
        public DbSet<DailySummaryStats> DailySummaryStats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();

                entity.HasMany(e => e.Orders)
                    .WithOne(o => o.User)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasMany(e => e.Products)
                    .WithOne(p => p.Category)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasMany(e => e.OrderDetails)
                    .WithOne(od => od.Product)
                    .HasForeignKey(od => od.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.ProductPrices)
                    .WithOne(pp => pp.Product)
                    .HasForeignKey(pp => pp.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Promotions)
                    .WithOne(p => p.Product)
                    .HasForeignKey(p => p.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.InventoryReceiptDetails)
                    .WithOne(ird => ird.Product)
                    .HasForeignKey(ird => ird.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // DailyProductStats → Product
                entity.HasMany<DailyProductStats>()
                    .WithOne(d => d.Product)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasMany(e => e.OrderDetails)
                    .WithOne(od => od.Order)
                    .HasForeignKey(od => od.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure InventoryReceipt
            modelBuilder.Entity<InventoryReceipt>(entity =>
            {
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.ReceiptDetails)
                    .WithOne(rd => rd.Receipt)
                    .HasForeignKey(rd => rd.ReceiptId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure DailyProductStats — composite unique index (ProductId, Date)
            modelBuilder.Entity<DailyProductStats>(entity =>
            {
                entity.HasIndex(e => new { e.ProductId, e.Date }).IsUnique();
            });

            // Configure DailySummaryStats — unique Date index
            modelBuilder.Entity<DailySummaryStats>(entity =>
            {
                entity.HasIndex(e => e.Date).IsUnique();
            });
        }
    }
}
