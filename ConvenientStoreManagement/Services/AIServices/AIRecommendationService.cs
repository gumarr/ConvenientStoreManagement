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

  public async Task<AIRecommendation> GetOrCreateAIAsync(int? userId)
  {
    var today = DateTime.Today;

    var existing = await _context.AIRecommendations
        .Include(x => x.User)
        .Where(x => x.Date == today)
        .OrderByDescending(x => x.Id)
        .FirstOrDefaultAsync();

    if (existing != null && existing.IsSuccess)
    {
      return existing;
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
      
      var recommendation = new AIRecommendation
      {
        Date = today,
        Content = clean,
        IsSuccess = true,
        UserId = userId,
        CreatedAt = DateTime.Now
      };

      _context.AIRecommendations.Add(recommendation);
      await _context.SaveChangesAsync();
      
      return recommendation;
    }
    catch
    {
      var recommendation = new AIRecommendation
      {
        Date = today,
        Content = null,
        IsSuccess = false,
        UserId = userId,
        CreatedAt = DateTime.Now
      };
      
      _context.AIRecommendations.Add(recommendation);
      await _context.SaveChangesAsync();

      return recommendation;
    }
  }
}
