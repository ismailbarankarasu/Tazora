using System.Text;
using System.Text.Json;
using Tazora.Models;

namespace Tazora.Services;

public class AiRecipeResult
{
    public string AssistantMessage { get; set; } = string.Empty;
    public List<Product> MatchedProducts { get; set; } = new();
}

public class AiService
{
    private readonly DatabaseService _databaseService;
    private readonly HttpClient _httpClient = new();

    private const string ApiKey = "";

    private const string ApiUrl =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent";

    public AiService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<AiRecipeResult> GetRecipeAndIngredientsAsync(string userPrompt)
    {
        var allProducts = await _databaseService.GetProductsAsync();

        try
        {
            var productListJson = JsonSerializer.Serialize(
                allProducts.Select(p => new { p.Id, p.Name, p.Price, p.Unit }));

            var prompt = $@"
Kullanıcı bir yemek tarifi istedi: '{userPrompt}'.

Elimizdeki ürünler:
{productListJson}

Görevin:
1. Kullanıcının istediği yemeği kısaca tarif et.
2. Tarifte kullanılabilecek ve elimizdeki listede bulunan ürünleri seç.
3. SADECE aşağıdaki JSON formatında, başka hiçbir şey eklemeden cevap ver:

{{
  ""AssistantMessage"": ""Kullanıcıya tarif ve malzemeler hakkında samimi bir mesaj"",
  ""MatchedProductIds"": [1, 3, 5]
}}

MatchedProductIds yalnızca yukarıdaki listedeki gerçek Id değerlerinden oluşmalı.
Markdown veya kod bloğu (```) kullanma.";

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            request.Headers.Add("x-goog-api-key", ApiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return GetFallbackResult($"AI servisine ulaşılamadı ({(int)response.StatusCode}).", allProducts);

            using var jsonResponse = JsonDocument.Parse(responseText);
            var aiTextResponse = jsonResponse.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            aiTextResponse = aiTextResponse.Replace("```json", "").Replace("```", "").Trim();

            using var doc = JsonDocument.Parse(aiTextResponse);
            var root = doc.RootElement;

            var message = root.GetProperty("AssistantMessage").GetString() ?? string.Empty;
            var matchedIds = JsonSerializer.Deserialize<List<int>>(
                root.GetProperty("MatchedProductIds").GetRawText()) ?? new List<int>();

            var matchedProducts = allProducts.Where(p => matchedIds.Contains(p.Id)).ToList();

            return new AiRecipeResult { AssistantMessage = message, MatchedProducts = matchedProducts };
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"AiService Exception: {exception}");
            return GetFallbackResult("Tarifiniz analiz edilirken bir hata oluştu. Lütfen tekrar deneyin.", allProducts);
        }
    }

    private static AiRecipeResult GetFallbackResult(string message, List<Product> allProducts) =>
        new() { AssistantMessage = message, MatchedProducts = allProducts.Take(3).ToList() };
}