using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvenientStoreManagement.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly StoreDbContext _context;

        public DashboardService(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardStatsAsync()
        {
            var today = DateTime.Today;
            
            // 1. Summary Cards
            var todayStats = await _context.DailySummaryStats
                .FirstOrDefaultAsync(s => s.Date == today);
                
            var todayRevenue = todayStats?.TotalRevenue ?? 0;
            var todayProfit = todayStats?.TotalProfit ?? 0;

            // This Week (last 7 days including today)
            var weekStart = today.AddDays(-6);
            var thisWeekRevenue = await _context.DailySummaryStats
                .Where(s => s.Date >= weekStart && s.Date <= today)
                .SumAsync(s => s.TotalRevenue);

            // This Month (1st to current)
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var thisMonthRevenue = await _context.DailySummaryStats
                .Where(s => s.Date >= monthStart && s.Date <= today)
                .SumAsync(s => s.TotalRevenue);

            // 2. Chart logic: Last 30 days
            var chartStart = today.AddDays(-29);
            var monthStats = await _context.DailySummaryStats
                .Where(s => s.Date >= chartStart && s.Date <= today)
                .OrderBy(s => s.Date)
                .ToListAsync();

            var labels = new List<string>();
            var chartRev = new List<decimal>();
            var chartProf = new List<decimal>();

            // Fill all 30 days even if missing
            for (int i = 0; i <= 29; i++)
            {
                var d = chartStart.AddDays(i);
                labels.Add(d.ToString("dd/MM"));
                
                var stat = monthStats.FirstOrDefault(s => s.Date == d);
                chartRev.Add(stat?.TotalRevenue ?? 0);
                chartProf.Add(stat?.TotalProfit ?? 0);
            }

            // 3. Top 5 Products (Last 30 days)
            var topProductsQuery = await _context.DailyProductStats
                .Include(p => p.Product)
                .Where(s => s.Date >= chartStart && s.Date <= today)
                .GroupBy(s => s.Product.Name)
                .Select(g => new TopProductDto
                {
                    ProductName = g.Key,
                    TotalQuantitySold = g.Sum(x => x.TotalQuantity)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(5)
                .ToListAsync();

            // 4. Low stock products (Threshold 5)
            var lowStock = await _context.Products
                .Where(p => p.Stock <= 5)
                .Select(p => new LowStockProductDto
                {
                    ProductName = p.Name,
                    CurrentStock = p.Stock
                })
                .OrderBy(p => p.CurrentStock)
                .Take(10)
                .ToListAsync();

            return new DashboardViewModel
            {
                TodayRevenue = todayRevenue,
                TodayProfit = todayProfit,
                ThisWeekRevenue = thisWeekRevenue,
                ThisMonthRevenue = thisMonthRevenue,
                ChartLabels = labels,
                ChartRevenue = chartRev,
                ChartProfit = chartProf,
                TopProducts = topProductsQuery,
                LowStockProducts = lowStock,
                LowStockProductCount = lowStock.Count
            };
        }
    }
}
