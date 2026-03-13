using System.Globalization;
using Realtime.Sse.Core;

namespace Realtime.Sse.Formatters;

internal class SseDefaultStringFormatter : SseEventFormatter<string>
{
    protected override string FormatData(string data)
    {
        return data.ToString(CultureInfo.InvariantCulture);
    }
}