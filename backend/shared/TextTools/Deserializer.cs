using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using core.Exceptions;

namespace core.TextTools;

public static class Deserializer<T>
{
    public static T Deserialize(string message)
    {
        return JsonSerializer.Deserialize<T>(message,
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new DeserializationException($"Failed to deserialize message: {message}");
    }

    public static T DeserializeAndValidate(string message)
    {
        T deserialized = Deserialize(message)!;
        var context = new ValidationContext(deserialized, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(deserialized, context, validationResults, true);

        if (!isValid)
        {
            var errors = string.Join(", ", validationResults.Select(rv => rv.ErrorMessage));
            throw new DeserializationException($"Failed to validate message: {message}. Errors: {errors}");
        }

        return deserialized;
    }
}