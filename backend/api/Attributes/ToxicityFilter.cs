using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using api.Externalities;

namespace api.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ToxicityFilter : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(
                Environment.GetEnvironmentVariable(
                    "AZURE_COGNITIVE_SERVICES"))) //todo always run filter in testing and prod, but for quickstart in development its okay to leave out
            return ValidationResult.Success!;
        var task = ValidateAsync(value);
        task.Wait();
        return task.Result;
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