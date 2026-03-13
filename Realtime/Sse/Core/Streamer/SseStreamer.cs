using Realtime.Sse.Formatters;

namespace Realtime.Sse.Core.Streamer;

internal abstract class SseStreamer
{
    protected readonly CancellationTokenSource CancellationSource;

    protected readonly HttpContext Context;
    protected readonly ILoggerFactory LogFactory;

    internal SseStreamer(HttpContext context, CancellationToken cancellationToken)
    {
        Context = context;
        CancellationSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, context.RequestAborted);
        LogFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug).AddSimpleConsole(opt =>
            {
                opt.IncludeScopes = true;
                opt.SingleLine = true;
                opt.TimestampFormat = "[HH:mm:ss] ";
            });
        });
        Logger = LogFactory.CreateLogger<SseStreamer>();
        InitResponse();
    }

    protected ILogger<SseStreamer> Logger { get; init; }

    internal CancellationToken CancellationToken => CancellationSource.Token;

    internal void InitResponse()
    {
        Context.Response.Headers.ContentType = "text/event-stream";
        Context.Response.Headers.CacheControl = "no-cache";
        Context.Response.Headers["X-Accel-Buffering"] = "no";
    }

    protected async Task WriteEventAsync<T>(string eventName, T data, SseEventFormatter<T> formatter)
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

    protected enum SseLogs
    {
        StartStream,
        EndStream,
        StartSignal,
        EndSignal,
        ExcStreamCancelled,
        ExcStreamDestroyed,
        ExcSignalCancelled,
        ExcSignalDestroyed
    }
}