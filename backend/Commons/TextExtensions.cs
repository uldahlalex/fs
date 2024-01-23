using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Commons;

public static class TextExtensions
{
    public static T DeserializeAndValidate<T>(this string str)
    {
        var obj = JsonSerializer.Deserialize<T>(str, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new ArgumentException("Could not deserialize string: " + str);
        Validator.ValidateObject(obj, new ValidationContext(obj), true);
        return obj;
    }
    
}