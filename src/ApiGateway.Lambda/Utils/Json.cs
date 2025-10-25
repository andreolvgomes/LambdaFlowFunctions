using System.Text.Json;

namespace ApiGateway.Lambda.Utils;

public class Json
{
    public static object Deserialize<T>(string value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        return JsonSerializer.Deserialize<T>(value);
    }

    public static string Deserialize(object value)
    {
        if (value is null) return string.Empty;
        return JsonSerializer.Serialize(value);
    }
}