using Microsoft.EntityFrameworkCore;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Pages.Dashboard;

public class AIRecommendationService : IAIRecommendationService
{
  private readonly StoreDbContext _context;
  private readonly IStatsService _statsService;
  private readonly IAIService _aiService;

  public AIRecommendationService(
      StoreDbContext context,
      IStatsService statsService,
      IAIService aiService)
  {
    _context = context;
    _statsService = statsService;
    _aiService = aiService;
  }

  public async Task<string> GetOrCreateAIAsync()
  {
    var today = DateTime.Today;

    var existing = await _context.AIRecommendations
        .Where(x => x.Date == today)
        .OrderByDescending(x => x.Id)
        .FirstOrDefaultAsync();

    if (existing != null && existing.IsSuccess)
    {
      return existing.Content;
    }

    var stats = await _statsService.GetWeeklyStatsAsync();
    var dailyStats = await _statsService.GetDailyProductStatsAsync();

    var prompt = PromptBuilder.Build(stats, dailyStats);

    try
    {
      var result = await _aiService.AskAsync(prompt);

      if (result.StartsWith("❌"))
      {
        throw new Exception(result);
      }

      var clean = AIFormatter.Clean(result);

      _context.AIRecommendations.Add(new AIRecommendation
      {
        Date = today,
        Content = clean,
        IsSuccess = true
      });

      await _context.SaveChangesAsync();

      return clean;
    }
    catch
    {
      _context.AIRecommendations.Add(new AIRecommendation
      {
        Date = today,
        Content = null,
        IsSuccess = false
      });

      await _context.SaveChangesAsync();

      return "❌ AI hiện tại không khả dụng";
    }
  }
}
