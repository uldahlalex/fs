namespace core.ExtensionMethods;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public class SafeConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        // Determine if the converter can handle the type of object
        return true; // Or more specific logic
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        JObject obj = new JObject();

        foreach (var prop in value.GetType().GetProperties())
        {
            try
            {
                var propVal = prop.GetValue(value);
                obj.Add(prop.Name, JToken.FromObject(propVal, serializer));
            }
            catch (Exception)
            {
                // Failed to serialize the property, so skip it
                // Optionally log the error
            }
        }

        obj.WriteTo(writer);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Handle deserialization if necessary
        throw new NotImplementedException();
    }
}
