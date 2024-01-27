using System.Net.Http.Json;
using Commons;
using Externalities._3rdPartyTransferModels;

namespace Externalities;

public class AzureCognitiveServices
{
    public async Task<string> GetToxicityAnalysis(string? message)
    {
        const string URL = "https://toxicityfilter.cognitiveservices.azure.com/contentsafety/text:analyze?api-version=2023-10-01";
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key",
            Environment.GetEnvironmentVariable("FULLSTACK_AZURE_COGNITIVE_SERVICES"));
        var request = new ToxicityRequest
        {
            categories = new List<string> { "Hate", "SelfHarm", "Sexual", "Violence" },
            outputType = "FourSeverityLevels",
            text = message!
        };
        return await (await httpClient.PostAsJsonAsync(URL, request)).Content.ReadAsStringAsync();
    }
}