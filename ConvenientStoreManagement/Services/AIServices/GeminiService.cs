using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

public class GeminiService : IAIService
{
  private readonly HttpClient _http;
  private readonly string _apiKey;
  private readonly string _model;


  public GeminiService(HttpClient http, IConfiguration config)
  {
    _http = http;
    _apiKey = config["Gemini:ApiKey"];
    _model = config["Gemini:Model"];
  }

  public async Task<string> AskAsync(string prompt)
  {
    var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

    var body = new
    {
      contents = new[]
        {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
    };

    var response = await _http.PostAsync(
        url,
        new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
    );

    var json = await response.Content.ReadAsStringAsync();

    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    if (root.TryGetProperty("error", out var error))
    {
      return "❌ AI Error: " + error.GetProperty("message").GetString();
    }

    var text = root
        .GetProperty("candidates")[0]
        .GetProperty("content")
        .GetProperty("parts")[0]
        .GetProperty("text")
        .GetString();

    return text ?? "❌ AI rỗng";
  }
}
