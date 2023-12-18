using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using api.Exceptions;

namespace api.ExtensionMethods;

public static class SerializerAndDeserializerExtensions
{
    public static string ToJsonString(this object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        });
    }

    public static T Deserialize<T>(this string message)
    {
        return JsonSerializer.Deserialize<T>(message,
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new DeserializationException($"Failed to deserialize message: {message}");
    }

    public static T DeserializeToModelAndValidate<T>(this string message)
    {
        var deserialized = Deserialize<T>(message)!;
        var context = new ValidationContext(deserialized, null, null);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(deserialized, context, validationResults, true);
        if (isValid) return deserialized;
        var errors = string.Join(", ", validationResults.Select(rv => rv.ErrorMessage));
        throw new DeserializationException($"Failed to validate message: {message}. Errors: {errors}");
    }
}