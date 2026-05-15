using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using XcLib.Sse.Options;

namespace XcLib.Sse.Core.Streamer;

internal static partial class StreamerLogs
{
    private enum StreamerLogId
    {
        StartStream = 10,
        EndStream,
        StartSignal,
        EndSignal,
        ExcStreamCancelled,
        ExcStreamDestroyed,
        ExcSignalCancelled,
        ExcSignalDestroyed,
        OptionsChanged,
        PingInterval
    }

    [LoggerMessage(LogLevel.Error, "signaling for {RequestPath}, event {EventName} destroyed, exception {Exception}",
        EventId = (int)StreamerLogId.ExcSignalDestroyed)]
    internal static partial void LogSignalingForRequestpathEventEventnameDestroyedExceptionException(
        this ILogger logger,
        PathString requestPath, string eventName, string exception, Exception exception1);

    [LoggerMessage(LogLevel.Error, "streaming for {RequestPath}, event {EventName} destroyed, exception {Exception}",
        EventId = (int)StreamerLogId.ExcStreamDestroyed)]
    internal static partial void LogStreamingForRequestpathEventEventnameDestroyedExceptionException(
        this ILogger logger,
        PathString requestPath, string eventName, string exception, Exception exception1);

    [LoggerMessage(LogLevel.Information, "options changed {a} {b}", EventId = (int)StreamerLogId.OptionsChanged)]
    internal static partial void LogOptionsChangedAb(this ILogger logger, SseOptions a, string? b);

    [LoggerMessage(LogLevel.Information, "ping interval {a}", EventId = (int)StreamerLogId.PingInterval)]
    internal static partial void LogIntervalA(this ILogger logger, TimeSpan a);

    [LoggerMessage(LogLevel.Debug, "start streaming for {a}, event {EventName}",
        EventId = (int)StreamerLogId.StartStream)]
    internal static partial void LogStartStreamingForAEventEventname(this ILogger logger, string a, string eventName);

    [LoggerMessage(LogLevel.Debug, "start signal for {a}, event {b}", EventId = (int)StreamerLogId.StartSignal)]
    internal static partial void LogStartingSseSignalForAEventB(this ILogger logger, string a, string b);

    [LoggerMessage(LogLevel.Debug, "end signal for {RequestPath}, event {EventName}",
        EventId = (int)StreamerLogId.EndSignal)]
    internal static partial void LogEndSignalForRequestpathEventEventname(this ILogger logger, PathString requestPath,
        string eventName);

    [LoggerMessage(LogLevel.Debug, "signaling for {RequestPath}, event {EventName} cancelled",
        EventId = (int)StreamerLogId.ExcSignalCancelled)]
    internal static partial void LogSignalingForRequestpathEventEventnameCancelled(this ILogger logger,
        PathString requestPath,
        string eventName);

    [LoggerMessage(LogLevel.Debug, "end streaming for {RequestPath}, event {EventName}",
        EventId = (int)StreamerLogId.EndStream)]
    internal static partial void LogEndStreamingForRequestpathEventEventname(this ILogger logger,
        PathString requestPath,
        string eventName);

    [LoggerMessage(LogLevel.Debug, "streaming for {RequestPath}, event {EventName} cancelled",
        EventId = (int)StreamerLogId.ExcStreamCancelled)]
    internal static partial void LogStreamingForRequestpathEventEventnameCancelled(this ILogger logger,
        PathString requestPath,
        string eventName);

    [LoggerMessage(LogLevel.Trace, "sending data {a}")]
    internal static partial void LogSendingDataA(this ILogger logger, string a);

    [LoggerMessage(LogLevel.Trace, "sending comment {a}: {b}")]
    internal static partial void LogSendingCommentAb(this ILogger logger, string a, string b);
}