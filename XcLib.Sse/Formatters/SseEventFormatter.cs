using System.Text;

namespace XcLib.Sse.Formatters;

public abstract class SseEventFormatter<T>
{
    public string Format(string eventName, T data) => BuildEventPayload(eventName, FormatData(data));
    public string Format(T data) => BuildEventPayload(string.Empty, FormatData(data));

    protected abstract string FormatData(T data);

    private static string BuildEventPayload(string eventName, string data)
    {
        StringBuilder builder = new StringBuilder();
        if (!string.IsNullOrEmpty(eventName))
            builder.Append("event: ").Append(eventName).Append('\n');
        
        builder.Append("data: ").Append(data).Append('\n');
        builder.Append('\n');
        return builder.ToString();
    }
}