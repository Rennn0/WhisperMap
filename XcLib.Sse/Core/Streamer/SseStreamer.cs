using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using XcLib.Sse.Core.Signal;
using XcLib.Sse.Formatters;
using XcLib.Sse.Options;

namespace XcLib.Sse.Core.Streamer;

public abstract class SseStreamer<T>
{
    protected readonly CancellationTokenSource CancellationSource;
    protected readonly HttpContext Context;
    protected readonly SseEventFormatter<T> Formatter;
    protected readonly SseOptions DefaultOptions;

    protected SseStreamer(IHttpContextAccessor context,
        IOptionsMonitor<SseOptions> defaultOptions,
        SseEventFormatter<T> formatter,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context.HttpContext);

        DefaultOptions = defaultOptions.CurrentValue;
        Formatter = formatter;
        Context = context.HttpContext;
        CancellationSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, Context.RequestAborted);
        Logger = loggerFactory.CreateLogger($"XcLib.Streamer.{nameof(SseStreamer<T>)}<{typeof(T).Name}>");
        InitResponse();
    }

    public Task StreamAsync(SseSignal<T> source, string eventName) =>
        StreamAsync(source, eventName, TimeSpan.FromSeconds(DefaultOptions.PingInterval), Formatter);

    public Task StreamAsync(IAsyncEnumerable<T> source, string eventName,
        T initialValue = default!) => StreamAsync(source, eventName, TimeSpan.FromSeconds(DefaultOptions.PingInterval),
        Formatter, initialValue);

    public Task StreamAsync(ChannelReader<T> source, string eventName) =>
        StreamAsync(source, eventName, TimeSpan.FromSeconds(DefaultOptions.PingInterval), Formatter);

    public Task StreamAsync(SseSignal<T> source, string eventName, TimeSpan heartbeatInterval) =>
        StreamAsync(source, eventName, heartbeatInterval, Formatter);

    public Task StreamAsync(IAsyncEnumerable<T> source, string eventName, TimeSpan heartbeatInterval,
        T initialValue = default!) => StreamAsync(source, eventName, heartbeatInterval, Formatter, initialValue);

    public Task StreamAsync(ChannelReader<T> source, string eventName, TimeSpan heartbeatInterval) =>
        StreamAsync(source, eventName, heartbeatInterval, Formatter);

    public abstract Task StreamAsync(SseSignal<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter);

    public abstract Task StreamAsync(IAsyncEnumerable<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter, T initialValue = default!);

    public abstract Task StreamAsync(ChannelReader<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter);

    protected ILogger Logger { get; init; }

    protected CancellationToken CancellationToken => CancellationSource.Token;

    protected void InitResponse()
    {
        Context.Response.Headers.ContentType = new StringValues("text/event-stream");
        Context.Response.Headers.CacheControl = new StringValues("no-cache");
        Context.Response.Headers.Connection = new StringValues("keep-alive");
        Context.Response.Headers["X-Accel-Buffering"] = new StringValues("no");
    }

    protected async Task WriteEventAsync(string eventName, T data, SseEventFormatter<T> formatter)
    {
        CancellationToken.ThrowIfCancellationRequested();

        string payload = formatter.Format(eventName, data);
        await Context.Response.WriteAsync(payload, CancellationToken);
        await Context.Response.Body.FlushAsync(CancellationToken);
    }

    protected async Task WriteCommentAsync(string comment)
    {
        CancellationToken.ThrowIfCancellationRequested();

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
        ExcSignalDestroyed
    }
}