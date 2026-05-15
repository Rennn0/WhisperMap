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
            Logger.LogOptionsChangedAb(options, s);
        });
        Logger.LogIntervalA(PingInterval);
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
        Logger.LogSendingDataA(Url);
        string payload = formatter.Format(eventName, data);
        await Context.Response.WriteAsync(payload, CancellationToken);
        await Context.Response.Body.FlushAsync(CancellationToken);
    }

    protected async Task WriteCommentAsync(string comment)
    {
        CancellationToken.ThrowIfCancellationRequested();
        Logger.LogSendingCommentAb(Url, comment);
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
}