using Microsoft.EntityFrameworkCore;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;

public class StatsService : IStatsService
{
  private readonly StoreDbContext _context;

  public StatsService(StoreDbContext context)
  {
    _context = context;
  }

  // ========================
  // WEEK
  // ========================
  public async Task<StatsDto> GetWeeklyStatsAsync()
  {
    var startDate = DateTime.Today.AddDays(-7).Date;
    return await BuildStats(startDate);
  }

  // ========================
  // MONTH
  // ========================
  public async Task<StatsDto> GetMonthlyStatsAsync()
  {
    var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).Date;
    return await BuildStats(startDate);
  }

  // ========================
  // CORE BUILDER
  // ========================
  private async Task<StatsDto> BuildStats(DateTime fromDate)
  {
    // 1. Tổng doanh thu + lợi nhuận
    var summary = await _context.DailySummaryStats
        .Where(x => x.Date.Date >= fromDate)
        .GroupBy(x => 1)
        .Select(g => new
        {
          TotalRevenue = g.Sum(x => x.TotalRevenue),
          TotalProfit = g.Sum(x => x.TotalProfit)
        })
        .FirstOrDefaultAsync();

    // 2. Top sản phẩm bán chạy
    var topProducts = await _context.DailyProductStats
        .Where(x => x.Date.Date >= fromDate)
        .GroupBy(x => new { x.ProductId, x.Product.Name })
        .Select(g => new
        {
          g.Key.Name,
          TotalQty = g.Sum(x => x.TotalQuantity)
        })
        .OrderByDescending(x => x.TotalQty)
        .Take(5)
        .Select(x => x.Name)
        .ToListAsync();

    // 3. Sản phẩm tồn kho cao
    var highStock = await _context.Products
        .OrderByDescending(p => p.Stock)
        .Take(5)
        .Select(p => p.Name)
        .ToListAsync();

    return new StatsDto
    {
      TotalRevenue = summary?.TotalRevenue ?? 0,
      TotalProfit = summary?.TotalProfit ?? 0,
      TopProducts = topProducts,
      HighStockProducts = highStock
    };
  }
  public async Task<List<DailyProductStatDto>> GetDailyProductStatsAsync()
  {
    var fromDate = DateTime.Today.AddDays(-7);

    return await _context.DailyProductStats
        .Where(x => x.Date.Date >= fromDate)
        .Select(x => new DailyProductStatDto
        {
          ProductName = x.Product.Name,
          TotalQuantity = x.TotalQuantity,
          TotalRevenue = x.TotalRevenue,
          TotalProfit = x.TotalProfit
        })
        .ToListAsync();
  }
}
