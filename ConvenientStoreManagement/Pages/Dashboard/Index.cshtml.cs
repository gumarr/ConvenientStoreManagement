using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace ConvenientStoreManagement.Pages.Dashboard
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IDashboardService _dashboardService;

        public IndexModel(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public DashboardViewModel Stats { get; set; } = new DashboardViewModel();

        public async Task OnGetAsync()
        {
            Stats = await _dashboardService.GetDashboardStatsAsync();
        }
    }
}
