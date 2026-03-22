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

            var totalRevenue = await _context.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => o.TotalAmount);

            var todayRevenue = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt.Date == today)
                .SumAsync(o => o.TotalAmount);

            var totalOrders = await _context.Orders.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalUsers = await _context.Users.CountAsync();

            // Calculate revenue for the last 7 days
            var weeklyRevenue = new Dictionary<string, decimal>();
            var startDate = today.AddDays(-6);
            
            var ordersLast7Days = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt.Date >= startDate)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(o => o.TotalAmount) })
                .ToListAsync();

            for (int i = 0; i < 7; i++)
            {
                var date = startDate.AddDays(i);
                var revenue = ordersLast7Days.FirstOrDefault(o => o.Date == date)?.Total ?? 0;
                weeklyRevenue.Add(date.ToString("ddd"), revenue);
            }

            return new DashboardViewModel
            {
                TotalRevenue = totalRevenue,
                TodayRevenue = todayRevenue,
                TotalOrders = totalOrders,
                TotalProducts = totalProducts,
                TotalUsers = totalUsers,
                WeeklyRevenue = weeklyRevenue
            };
        }
    }
}
