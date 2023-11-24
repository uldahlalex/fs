using System.Reflection;
using core.Models;

namespace core;

public static class EnforceNameCheck
{
    public static void CheckPropertyNames<T>()
    {
        var typeInfo = typeof(T).GetTypeInfo();
        foreach (var propertyInfo in typeInfo.DeclaredProperties)
        {
            var enforceNameAttribute = propertyInfo.GetCustomAttribute<EnforceNameAttribute>();
            if (enforceNameAttribute != null && enforceNameAttribute.Name != propertyInfo.Name)
            {
                throw new Exception($"Property named '{propertyInfo.Name}' violated the naming rule. It should be named '{enforceNameAttribute.Name}'.");
            }
        }
    }
    
}