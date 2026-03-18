using System.Text;

namespace Realtime.Sse.Formatters;

internal abstract class SseEventFormatter<T>
{
    internal string Format(string eventName, T data) => BuildEventPayload(eventName, FormatData(data));
    internal string Format(T data) => BuildEventPayload(string.Empty, FormatData(data));

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