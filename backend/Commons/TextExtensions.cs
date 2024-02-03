using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Commons;

public static class TextExtensions
{
    public static T Deserialize<T>(this string str)
    {
        return JsonSerializer.Deserialize<T>(str, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new ArgumentException("Could not deserialize string: " + str);
    }
    
}