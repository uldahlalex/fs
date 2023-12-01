

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using core.Exceptions;
using Json.Net;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace core.ExtensionMethods;

public static class SerializerAndDeserializerExtensions
{
    public static string ToJsonString(this object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public static string ToJsonStringAlternative(this object obj)
    {
        return System.Text.Json.JsonSerializer.Serialize(obj);
    }
    

    public static T FromJson<T>(this string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
/*
    public static void Dump(this object o)
    {
        ObjectDumper.Dump(o);
    }*/

    public static string DumpNoCircularReferences(this object o)
    {
        //var dump = ObjectDumper.Dump(o);
      /*  var jsonSerializerSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            
        };

        return JsonConvert.SerializeObject(o, 
            Formatting.Indented, 
            jsonSerializerSettings);*/
      return JsonNet.Serialize(o);
    }
    
    /**
     * throws DeserializationException
     */
    public static T Deserialize<T>(this string message)
    {
        return JsonSerializer.Deserialize<T>(message,
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new DeserializationException($"Failed to deserialize message: {message}");
    }

    public static T DeserializeToModelAndValidate<T>(this string message)
    {
        var deserialized = Deserialize<T>(message)!;
        var context = new ValidationContext(deserialized, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(deserialized, context, validationResults, true);
        if (isValid) return deserialized;
        var errors = string.Join(", ", validationResults.Select(rv => rv.ErrorMessage));
        throw new DeserializationException($"Failed to validate message: {message}. Errors: {errors}");
    }
}

