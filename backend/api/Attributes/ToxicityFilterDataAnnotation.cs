using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using api.Externalities;
using Serilog;

namespace api.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ToxicityFilterDataAnnotation : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var azKey = Environment.GetEnvironmentVariable("AZURE_COGNITIVE_SERVICES")!;
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;

        if (env.ToLower().Equals("production"))
        {
            var task = ValidateAsync(value);
            task.Wait();
            return task.Result;
        } 
        if (env.ToLower().Equals("development") && string.IsNullOrEmpty(azKey))
        {
            Log.Information("Skipping toxicity filter in development mode when no API key is provided");
            return ValidationResult.Success!;
        }
        return ValidationResult.Success!;
        
    }

    private async Task<ValidationResult> ValidateAsync(object value)
    {
        var result = await new AzureCognitiveServices().IsToxic(value.ToString());
        if (result.Any(x => x.severity > 1))
        {
            var dict = result.ToDictionary(x => x.category, x => x.severity);
            var response = JsonSerializer.Serialize(dict, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            throw new ValidationException(response);
        }

        return ValidationResult.Success!;
    }
}