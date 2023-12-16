using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using core.Exceptions;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace core.ExtensionMethods;

public static class SerializerAndDeserializerExtensions
{
    public static string ToJsonString(this object obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
    }


    /* //newtonsoft implementation
    public static T FromJson<T>(this string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }*/

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
    
    public static object DeserializeToType(string message, string eventType)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var type = assembly.GetTypes().FirstOrDefault(t => t.Name == eventType);
        if (type == null)
        {
            throw new InvalidOperationException($"Type not found for event type: {eventType}");
        }

        MethodInfo deserializeMethod = typeof(SerializerAndDeserializerExtensions)
            .GetMethod(nameof(SerializerAndDeserializerExtensions.Deserialize))
            ?.MakeGenericMethod(type);

        if (deserializeMethod == null)
        {
            throw new InvalidOperationException("Deserialize method not found.");
        }

        return deserializeMethod.Invoke(null, new object[] { message });
    }
}