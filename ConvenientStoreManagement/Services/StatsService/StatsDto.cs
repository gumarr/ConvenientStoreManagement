public class StatsDto
{
  public decimal TotalRevenue { get; set; }
  public decimal TotalProfit { get; set; }

  public List<string> TopProducts { get; set; } = new();
  public List<string> HighStockProducts { get; set; } = new();
}
