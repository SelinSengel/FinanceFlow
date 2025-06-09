using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

public class OpenAIService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public OpenAIService(IConfiguration config, IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
        _apiKey = config["OpenAI:ApiKey"]!;
        _baseUrl = config["OpenAI:BaseUrl"]!;
    }

    public async Task<string> AskAsync(string prompt, string? systemMessageOverride = null)
    {
        var http = _httpFactory.CreateClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var systemMessage = systemMessageOverride ?? "Sen kişisel harcama verileri üzerinden analiz yapan bir bütçe danışmanısın. Samimi ve kısa net yönlendirmeler yapıyorsun. En fazla 200 kelime konuşabilirsin.Her yenilendiğinde daha farklı cevaplar ver.";

        var body = new
        {
            model = "gpt-4-turbo",
            max_tokens = 512,
            temperature = 0.7,
            messages = new[]
            {
                new { role = "system", content = systemMessage },
                new { role = "user", content = prompt }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await http.PostAsync($"{_baseUrl}/chat/completions", content);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return $"[OpenAI HATASI] {response.StatusCode}: {json}";

        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString() ?? "[YANIT BOŞ GELDİ]";
        }
        catch (Exception ex)
        {
            return $"[JSON HATASI] {ex.Message}";
        }
    }
}
