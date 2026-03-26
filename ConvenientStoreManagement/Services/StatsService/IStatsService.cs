using ConvenientStoreManagement.Models;

public interface IStatsService
{
  Task<StatsDto> GetWeeklyStatsAsync();
  Task<StatsDto> GetMonthlyStatsAsync();
  Task<List<DailyProductStatDto>> GetDailyProductStatsAsync();
}
