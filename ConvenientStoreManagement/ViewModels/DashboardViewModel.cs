using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }
        
        // Simple data for basic chart if needed
        public Dictionary<string, decimal> WeeklyRevenue { get; set; } = new Dictionary<string, decimal>();
    }
}
