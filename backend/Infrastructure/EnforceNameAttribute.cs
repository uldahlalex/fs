using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Infrastructure;

[AttributeUsage(AttributeTargets.All)]
public class EnforceNameAttribute : ValidationAttribute
{
    public EnforceNameAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
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
                throw new Exception(
                    $"Property named '{propertyInfo.Name}' violated the naming rule. It should be named '{enforceNameAttribute.Name}'.");
        }
    }
}