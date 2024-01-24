using System.Net.Http.Json;
using Commons;
using Externalities._3rdPartyTransferModels;

namespace Externalities;

public class AzureCognitiveServices
{
    public async Task<List<CategoriesAnalysis>> IsToxic(string? message)
    {
        const string BaseUrl = "https://toxicityfilter.cognitiveservices.azure.com/";


        var _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key",
            Environment.GetEnvironmentVariable("FULLSTACK_AZURE_COGNITIVE_SERVICES"));

        var request = new ToxicityRequest
        {
            categories = new List<string> { "Hate", "SelfHarm", "Sexual", "Violence" },
            outputType = "FourSeverityLevels",
            text = message!
        };
        var stringResponse =
            await _httpClient.PostAsJsonAsync(BaseUrl + "contentsafety/text:analyze?api-version=2023-10-01", request);
        var response = await stringResponse.Content.ReadAsStringAsync();
        return response.Deserialize<ToxicityResponse>().categoriesAnalysis;
    }
}