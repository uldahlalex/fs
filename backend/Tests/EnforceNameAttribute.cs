using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Tests;

[AttributeUsage(AttributeTargets.All)]
public class EnforceNameAttribute(string name) : ValidationAttribute
{
    public string Name { get; } = name;
}

public static class EnforceNameCheck
{
    public static void CheckPropertyNames<T>()
    {
        var typeInfo = typeof(T).GetTypeInfo();
        foreach (var propertyInfo in typeInfo.DeclaredProperties)
        {
            var enforceNameAttribute = propertyInfo.GetCustomAttribute<EnforceNameAttribute>();
            if (enforceNameAttribute != null && enforceNameAttribute.Name != propertyInfo.Name)
                throw new ValidationException(
                    $"Property named '{propertyInfo.Name}' violated the naming rule. It should be named '{enforceNameAttribute.Name}'.");
        }
    }
}