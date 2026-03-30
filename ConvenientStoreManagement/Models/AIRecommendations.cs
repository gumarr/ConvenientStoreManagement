using System;

namespace ConvenientStoreManagement.Models
{
    public class AIRecommendation
{
  public int Id { get; set; }
  public DateTime Date { get; set; }
  public string? Content { get; set; }
  public bool IsSuccess { get; set; }
  public DateTime CreatedAt { get; set; }

  public int? UserId { get; set; }
  public User User { get; set; }
}
}
