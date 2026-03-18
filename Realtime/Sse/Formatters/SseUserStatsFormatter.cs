namespace Realtime.Sse.Formatters;

internal class SseUserStatsFormatter : SseEventFormatter<SseUserStatsFormatter.UserStats>
{
    internal struct UserStats
    {
        internal long Online { get; init; }
        internal long Offline { get; init; }
    }

    protected override string FormatData(UserStats data) => $"{data.Online};{data.Offline}";
}