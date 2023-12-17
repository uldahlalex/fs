using System.ComponentModel.DataAnnotations;
using api._3rdPartyTransferModels;
using api.ExtensionMethods;
using Serilog;

namespace api.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ToxicityFilter : ValidationAttribute
{
    private const string BaseUrl = "https://toxicityfilter.cognitiveservices.azure.com/";

    private readonly HttpClient _httpClient;

    public ToxicityFilter()
    {
        _httpClient = new HttpClient();
        //_httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("tox"));
    }

    /*  protected override async Task<ValidationResult> IsValid(object? givenString, ValidationContext validationContext)
      {
          if (await IsToxic((string?)givenString)) return new ValidationResult("Message is toxic.");

          return ValidationResult.Success!;
      }*/

    public async Task<bool> IsToxic(string? message)
    {
        var request = new ToxicityRequest
        {
            categories = new List<string> { "Hate", "SelfHarm", "Sexual", "Violence" },
            outputType = "FourSeverityLevels",
            text = message
        };
        var stringResponse =
            await _httpClient.PostAsJsonAsync(BaseUrl + "contentsafety/text:analyze?api-version=2023-10-01", request);
        var response = await stringResponse.Content.ReadAsStringAsync();
        Log.Information(response);
        Console.WriteLine(response);
        var toxicityResponse = response.Deserialize<ToxicityResponse>();
        toxicityResponse.categoriesAnalysis.ForEach(x =>
        {
            Console.WriteLine(x.category);
            Console.WriteLine(x.severity);
            //Få styr på loggeren i test runneren (både riders og dotnets)
        });

        if (toxicityResponse.categoriesAnalysis.Any(x => x.severity > 1))
            return true;
        return false;
    }
}