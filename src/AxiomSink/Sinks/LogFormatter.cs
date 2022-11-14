using Serilog.Formatting.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text;
using Serilog.Events;

namespace Serilog.Sinks.Axiom;

public class LogFormatter
{
    private static readonly JsonFormatter formatter = new JsonFormatter(renderMessage: true);
    private static readonly JsonSerializerOptions settings = new JsonSerializerOptions
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private string ExtractProperties(string s)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(s) ?? new();
        if (!dict.TryGetValue("Properties", out JsonElement props))
        {
            return s;
        }

        var dict2 = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(props) ?? new();
        foreach (var (key, val) in dict2)
        {
            dict[key] = val;
        }

        return JsonSerializer.Serialize(dict);
    }

    public string FixLevel(string level)
    {
        if (string.IsNullOrEmpty(level)) return "info";

        var l = level.ToLower();

        return l switch
        {
            "warning" => "warn",
            "information" => "info",
            _ => l,
        };
    }

    public object FormatMessage(LogEvent logEvent)
    {
        var payload = new StringBuilder();
        var writer = new StringWriter(payload);

        formatter.Format(logEvent, writer);


        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(ExtractProperties(writer.ToString())) ?? new();

        RenameKey(dict, "Level", "level");
        dict["level"] = FixLevel(dict["level"].ToString() ?? "");

        RenameKey(dict, "Application", "logger");
        RenameKey(dict, "Timestamp", "timestamp");
        RenameKey(dict, "RenderedMessage", "msg");
        RenameKey(dict, "Exception", "error");

        RemoveKey(dict, "MessageTemplate");
        RemoveKey(dict, "Renderings");
        RemoveKey(dict, "Properties");
        
        foreach (var key in dict.Keys.ToList())
            RenameKey(dict, key, key.ToLower());

        return dict;
    }

    public void RenameKey<TKey, TValue>(IDictionary<TKey, TValue> dict,
                                           TKey oldKey, TKey newKey)
    {
        if (dict.TryGetValue(oldKey, out TValue value))
        {
            dict.Remove(oldKey);
            dict.Add(newKey, value);
        }
    }

    public void RemoveKey<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey key) => dict.Remove(key);
}
