using XcLib.Sse.Formatters;

namespace Realtime.SseFeatures.Formatters;

public class SseUserStatsFormatter : SseEventFormatter<UserStats>
{
    protected override string FormatData(UserStats data) => $"{data.Online};{data.Offline}";
}