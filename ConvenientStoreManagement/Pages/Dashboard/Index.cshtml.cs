using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ConvenientStoreManagement.Pages.Dashboard
{
    public static class AIFormatter
    {
        public static string Clean(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            return input
                .Replace("**", "")
                .Replace("###", "")
                .Replace("* ", "- ")
                .Replace("\n\n", "\n")
                .Trim();
        }
    }

    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IDashboardService _dashboardService;
        private readonly IStatsService _statsService;
        private readonly IAIRecommendationService _aiRecommendationService;

        public string AIResult { get; set; }

        public DashboardViewModel Stats { get; set; } = new();
        public StatsDto ExtraStats { get; set; } = new();

        public IndexModel(
            IDashboardService dashboardService,
            IStatsService statsService,
            IAIRecommendationService aiRecommendationService)
        {
            _dashboardService = dashboardService;
            _statsService = statsService;
            _aiRecommendationService = aiRecommendationService;
        }

        public async Task OnGetAsync()
        {
            Stats = await _dashboardService.GetDashboardStatsAsync();
            ExtraStats = await _statsService.GetWeeklyStatsAsync();
        }

        public async Task<IActionResult> OnPostAISuggestAsync()
        {
            Stats = await _dashboardService.GetDashboardStatsAsync();

            AIResult = await _aiRecommendationService.GetOrCreateAIAsync();

            return Page();
        }
    }
}
