using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Serilog;

namespace core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ToxicityFilter : ValidationAttribute
{

    private readonly HttpClient _httpClient;
    public ToxicityFilter()
    {
        _httpClient = new HttpClient();
        //_httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("tox"));
        
    }
    private const string BaseUrl = "https://toxicityfilter.cognitiveservices.azure.com/";

  /*  protected override async Task<ValidationResult> IsValid(object? givenString, ValidationContext validationContext)
    {
        if (await IsToxic((string?)givenString)) return new ValidationResult("Message is toxic.");

        return ValidationResult.Success!;
    }*/

    public async Task<bool> IsToxic(string? message)
    {
        var request = new ToxicityRequest()
        {
            categories = new List<string>() {"Hate", "SelfHarm", "Sexual", "Violence"},
            outputType = "FourSeverityLevels",
            text =message,
        };
        var stringResponse = await _httpClient.PostAsJsonAsync<ToxicityRequest>(BaseUrl + "contentsafety/text:analyze?api-version=2023-10-01", request);
        var response = await stringResponse.Content.ReadAsStringAsync();
        Log.Information(response);
        Console.WriteLine(response);
        var toxicityResponse = JsonConvert.DeserializeObject<ToxicityResponse>(response);
        toxicityResponse.categoriesAnalysis.ForEach(x =>
        {
            Console.WriteLine(x.category);
            Console.WriteLine(x.severity);
            
        });

        if(toxicityResponse.categoriesAnalysis.Any(x => x.severity > 1))
            return true;
        return false;
    }
}


