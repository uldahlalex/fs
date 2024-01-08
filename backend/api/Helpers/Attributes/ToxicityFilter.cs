using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using api.Externalities;

namespace api.Helpers.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ToxicityFilter : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
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
            var response = JsonSerializer.Serialize(dict, new JsonSerializerOptions()
            {
                WriteIndented = true
            });
            throw new ValidationException(response);
        }

        return ValidationResult.Success;
    }
}