using System.Threading.Tasks;
using ConvenientStoreManagement.Models;

public interface IAIRecommendationService
{
  Task<AIRecommendation> GetOrCreateAIAsync(int? userId);
}
