using System.Collections.Generic;

namespace ConvenientStoreManagement.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TodayRevenue { get; set; }
        public decimal TodayProfit { get; set; }
        
        public decimal ThisWeekRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }

        public string TodayStatsSource { get; set; }
        public string TodayStatsCreatedBy { get; set; }

        public int LowStockProductCount { get; set; }

        // Chart Data: Last 30 days
        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> ChartRevenue { get; set; } = new();
        public List<decimal> ChartProfit { get; set; } = new();

        // Top 5 Products by Quantity Sold
        public List<TopProductDto> TopProducts { get; set; } = new();
        
        // Low Stock Products
        public List<LowStockProductDto> LowStockProducts { get; set; } = new();
    }

    public class TopProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
    }

    public class LowStockProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
    }
}
