using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using XcLib.Sse.Core.Signal;
using XcLib.Sse.Formatters;
using XcLib.Sse.Options;

namespace XcLib.Sse.Core.Streamer;

public abstract partial class SseStreamer<T>
{
    private uint _optChanged;
    private readonly IOptionsMonitor<SseOptions> _sseOptions;
    protected readonly CancellationTokenSource CancellationSource;
    protected readonly HttpContext Context;
    protected readonly SseEventFormatter<T> Formatter;
    protected bool OptionsChanged => Volatile.Read(ref _optChanged) != 0;
    protected ILogger Logger { get; init; }
    protected SseOptions SseOptions => _sseOptions.CurrentValue;
    protected TimeSpan PingInterval => TimeSpan.FromSeconds(SseOptions.PingInterval);
    protected string Url => Context.Request.Path + Context.Request.QueryString;
    protected CancellationToken CancellationToken => CancellationSource.Token;
    protected SseStreamer(IHttpContextAccessor context,
        IOptionsMonitor<SseOptions> sseOptions,
        SseEventFormatter<T> formatter,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context.HttpContext);

        _sseOptions = sseOptions;
        _optChanged = 0;
        Formatter = formatter;
        Context = context.HttpContext;
        CancellationSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, Context.RequestAborted);
        Logger = loggerFactory.CreateLogger($"XcLib.Streamer.{nameof(SseStreamer<T>)}<{typeof(T).Name}>");
        _sseOptions.OnChange((options, s) =>
        {
            Volatile.Write(ref _optChanged, 1);
            LogOptionsChangedAb(Logger, options, s);
        });
        LogIntervalA(Logger, PingInterval);
        InitResponse();
    }

    public Task StreamAsync(SseSignal<T> source, string? eventName = null, TimeSpan? pingInterval = null,
        SseEventFormatter<T>? formatter = null) =>
        StreamAsync(source, eventName ?? string.Empty, pingInterval ?? PingInterval, formatter ?? Formatter);

    public Task StreamAsync(IAsyncEnumerable<T> source, string? eventName = null, TimeSpan? pingInterval = null,
        SseEventFormatter<T>? formatter = null, T? initialValue = default) =>
        StreamAsync(source, eventName ?? string.Empty, pingInterval ?? PingInterval, formatter ?? Formatter,
            initialValue ?? default!);

    public Task StreamAsync(ChannelReader<T> source, string? eventName = null, TimeSpan? pingInterval = null,
        SseEventFormatter<T>? formatter = null) =>
        StreamAsync(source, eventName ?? string.Empty, pingInterval ?? PingInterval, formatter ?? Formatter);

    public abstract Task StreamAsync(SseSignal<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter);

    public abstract Task StreamAsync(IAsyncEnumerable<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter, T initialValue = default!);

    public abstract Task StreamAsync(ChannelReader<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter);


    protected void InitResponse()
    {
        Context.Response.Headers["content-type"] = new StringValues("text/event-stream");
        Context.Response.Headers["cache-control"] = new StringValues("no-cache");
        Context.Response.Headers["connection"] = new StringValues("keep-alive");
        Context.Response.Headers["x-accel-buffering"] = new StringValues("no");
    }

    protected async Task WriteEventAsync(string eventName, T data, SseEventFormatter<T> formatter)
    {
        CancellationToken.ThrowIfCancellationRequested();
        LogSendingDataA(Logger, Url);
        string payload = formatter.Format(eventName, data);
        await Context.Response.WriteAsync(payload, CancellationToken);
        await Context.Response.Body.FlushAsync(CancellationToken);
    }

    protected async Task WriteCommentAsync(string comment)
    {
        CancellationToken.ThrowIfCancellationRequested();
        LogSendingCommentAb(Logger, Url, comment);
        string payload = $": {comment}\n\n";
        await Context.Response.WriteAsync(payload, CancellationToken);
        await Context.Response.Body.FlushAsync(CancellationToken);
    }

    protected async Task SafeWriteAsync(Func<Task> write)
    {
        try
        {
            CancellationToken.ThrowIfCancellationRequested();
            await write();
        }
        catch (Exception ex) when (IsClientDisconnect(ex))
        {
            throw new OperationCanceledException(CancellationToken);
        }
    }

    protected bool IsClientDisconnect(Exception ex)
    {
        if (CancellationToken.IsCancellationRequested)
            return true;

        return ex is OperationCanceledException or IOException or ObjectDisposedException or TaskCanceledException
            or NotSupportedException;
    }

    protected enum StreamerLogs
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
        EventId = (int)StreamerLogs.ExcSignalDestroyed)]
    protected static partial void LogSignalingForRequestpathEventEventnameDestroyedExceptionException(ILogger logger,
        PathString requestPath, string eventName, string exception, Exception exception1);

    [LoggerMessage(LogLevel.Error, "streaming for {RequestPath}, event {EventName} destroyed, exception {Exception}",
        EventId = (int)StreamerLogs.ExcStreamDestroyed)]
    protected static partial void LogStreamingForRequestpathEventEventnameDestroyedExceptionException(ILogger logger,
        PathString requestPath, string eventName, string exception, Exception exception1);

    [LoggerMessage(LogLevel.Information, "options changed {a} {b}", EventId = (int)StreamerLogs.OptionsChanged)]
    protected static partial void LogOptionsChangedAb(ILogger logger, SseOptions a, string? b);

    [LoggerMessage(LogLevel.Information, "ping interval {a}", EventId = (int)StreamerLogs.PingInterval)]
    protected static partial void LogIntervalA(ILogger logger, TimeSpan a);

    [LoggerMessage(LogLevel.Debug, "start streaming for {a}, event {EventName}",
        EventId = (int)StreamerLogs.StartStream)]
    protected static partial void LogStartStreamingForAEventEventname(ILogger logger, string a, string eventName);

    [LoggerMessage(LogLevel.Debug, "start signal for {a}, event {b}", EventId = (int)StreamerLogs.StartSignal)]
    protected static partial void LogStartingSseSignalForAEventB(ILogger logger, string a, string b);

    [LoggerMessage(LogLevel.Debug, "end signal for {RequestPath}, event {EventName}",
        EventId = (int)StreamerLogs.EndSignal)]
    protected static partial void LogEndSignalForRequestpathEventEventname(ILogger logger, PathString requestPath,
        string eventName);

    [LoggerMessage(LogLevel.Debug, "signaling for {RequestPath}, event {EventName} cancelled",
        EventId = (int)StreamerLogs.ExcSignalCancelled)]
    protected static partial void LogSignalingForRequestpathEventEventnameCancelled(ILogger logger,
        PathString requestPath,
        string eventName);

    [LoggerMessage(LogLevel.Debug, "streaming for {RequestPath}, event {EventName} cancelled",
        EventId = (int)StreamerLogs.ExcStreamCancelled)]
    protected static partial void LogStreamingForRequestpathEventEventnameCancelled(ILogger logger,
        PathString requestPath,
        string eventName);

    [LoggerMessage(LogLevel.Debug, "end streaming for {RequestPath}, event {EventName}",
        EventId = (int)StreamerLogs.EndStream)]
    protected static partial void LogEndStreamingForRequestpathEventEventname(ILogger logger, PathString requestPath,
        string eventName);

    [LoggerMessage(LogLevel.Trace, "sending data {a}")]
    protected static partial void LogSendingDataA(ILogger logger, string a);

    [LoggerMessage(LogLevel.Trace, "sending comment {a}: {b}")]
    protected static partial void LogSendingCommentAb(ILogger logger, string a, string b);

}