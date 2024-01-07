using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using ArgumentException = System.ArgumentException;

namespace api.Extensions;

//3
public static class Extensions
{
    public static T DeserializeAndValidate<T>(this string str)
    {
        var obj = JsonSerializer.Deserialize<T>(str, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new ArgumentException("Could not deserialize string: " + str);
        Validator.ValidateObject(obj, new ValidationContext(obj));
        return obj;
    }
}