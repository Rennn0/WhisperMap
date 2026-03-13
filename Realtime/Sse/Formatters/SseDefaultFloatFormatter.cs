using System.Globalization;
using Realtime.Sse.Core;

namespace Realtime.Sse.Formatters;

internal class SseDefaultFloatFormatter : SseEventFormatter<float>
{
    protected override string FormatData(float data)
    {
        return data.ToString(CultureInfo.InvariantCulture);
    }
}