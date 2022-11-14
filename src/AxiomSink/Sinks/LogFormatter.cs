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

    private string RemoveProperties(string s) => s.Replace("Properties.", "");

    public string FixLevel(string level) => level.ToLower() switch
    {
        "warning" => "warn",
        "information" => "info",
        _ => level.ToLower(),
    };

    public Dictionary<string, object> ParseProperty(JsonProperty prop)
    {
        var dict = new Dictionary<string, object>();

        if (prop.Value.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in prop.Value.EnumerateObject())
            {
                var innerDict = ParseProperty(property);
                foreach (var (k, v) in innerDict)
                {
                    var key = RemoveProperties($"{prop.Name}.{k}");
                    dict[key] = v;
                }
            }
        }
        else
        {
            dict[prop.Name] = prop.Value.Clone();
        }

        return dict;
    }

    public object FormatMessage(LogEvent logEvent)
    {
        var payload = new StringBuilder();
        var writer = new StringWriter(payload);

        formatter.Format(logEvent, writer);

        var msg = writer.ToString();
        var dict = new Dictionary<string, object>();

        using var doc = JsonDocument.Parse(msg);

        foreach (var property in doc.RootElement.EnumerateObject())
        {
            foreach (var (k, v) in ParseProperty(property))
            {
                dict[k] = v;
            }
        }


        // var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(ExtractProperties(writer.ToString())) ?? new();

        RenameKey(dict, "Level", "level");
        dict["level"] = FixLevel(dict["level"].ToString() ?? "");

        RenameKey(dict, "Application", "logger");
        RenameKey(dict, "Timestamp", "timestamp");
        RenameKey(dict, "RenderedMessage", "msg");
        RenameKey(dict, "Exception", "error");

        RemoveKey(dict, "MessageTemplate");
        RemoveKey(dict, "Renderings");
        RemoveKey(dict, "Properties");

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
