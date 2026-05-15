using Microsoft.Extensions.Logging;

namespace XcLib.Sse.Core.Stream;

internal static partial class StreamLogs
{
    private enum StreamLogId
    {
        Subscribe = 100,
        Unsubscribe,
        Publish,
        BrokenSub,
        BrokenSubInfo,
        Disposed
    }

    [LoggerMessage(LogLevel.Warning, "broken subs {Count}, dropping", EventId = (int)StreamLogId.BrokenSubInfo)]
    internal static partial void LogBrokenSubsCountDropping(this ILogger logger, int count);

    [LoggerMessage(LogLevel.Information, "deliver {Delivered}, subs {Subs}", EventId = (int)StreamLogId.Publish)]
    internal static partial void LogDeliverDeliveredSubsSubs(this ILogger logger, int delivered, int subs);

    [LoggerMessage(LogLevel.Debug, "disposed stream, subscribers {SubscribersCount}",
        EventId = (int)StreamLogId.Disposed)]
    internal static partial void
        LogDisposedStreamSubscribersSubscriberscount(this ILogger logger, int subscribersCount);

    [LoggerMessage(LogLevel.Debug, "new subscriber {Id}, subs {Count}", EventId = (int)StreamLogId.Subscribe)]
    internal static partial void LogNewSubscriberIdSubsCount(this ILogger logger, string id, int count);

    [LoggerMessage(LogLevel.Debug, "removed sub {Id}", EventId = (int)StreamLogId.Unsubscribe)]
    internal static partial void LogRemovedSubId(this ILogger logger, string id);
}