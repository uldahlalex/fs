using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using api.Abstractions;
using api.ClientEventHandlers;
using api.Externalities;
using api.Models._3rdPartyTransferModels;
using Fleck;
using Serilog;

namespace api.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ToxicityFilterDataAnnotation : ValidationAttribute
{

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var azKey = Environment.GetEnvironmentVariable("FULLSTACK_AZURE_COGNITIVE_SERVICES")!;
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
        if (env.ToLower().Equals("development") && string.IsNullOrEmpty(azKey))
        {
            Log.Information("Skipping toxicity filter in development mode when no API key is provided");
            return ValidationResult.Success;
        }

        if (Environment.GetEnvironmentVariable("FULLSTACK_SKIP_TOXICITY_FILTER")?.ToLower().Equals("true") ?? false)
            return ValidationResult.Success;

        var result = new AzureCognitiveServices().IsToxic(value.ToString());
        result.Wait();
 
        if (result.Result.Any(x => x.severity > 0.5))
        {
            var dict = result.Result.ToDictionary(x => x.category, x => x.severity);
            var response = JsonSerializer.Serialize(dict, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            throw new ValidationException(response);
        }
        return ValidationResult.Success;
    }

}