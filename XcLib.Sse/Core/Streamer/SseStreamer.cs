using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using XcLib.Sse.Core.Signal;
using XcLib.Sse.Formatters;

namespace XcLib.Sse.Core.Streamer;

public abstract class SseStreamer
{
    protected readonly CancellationTokenSource CancellationSource;
    protected readonly HttpContext Context;
    protected readonly ILoggerFactory LogFactory;

    protected SseStreamer(IHttpContextAccessor context, ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context.HttpContext);

        Context = context.HttpContext;
        CancellationSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, Context.RequestAborted);
        LogFactory = loggerFactory;
        Logger = LogFactory.CreateLogger<SseStreamer>();
        InitResponse();
    }

    [Flags]
    public enum StreamerType
    {
        Signal,
        Enumerable,
        Channel
    }

    public delegate SseStreamer StreamerFactory(StreamerType streamerType);

    public virtual Task StreamAsync<T>(SseSignal<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter) => throw new NotImplementedException();

    public virtual Task StreamAsync<T>(IAsyncEnumerable<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter, T initialValue = default!) => throw new NotImplementedException();

    public virtual Task StreamAsync<T>(ChannelReader<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter) => throw new NotImplementedException();
    
    protected ILogger<SseStreamer> Logger { get; init; }

    protected CancellationToken CancellationToken => CancellationSource.Token;

    protected void InitResponse()
    {
        Context.Response.Headers.ContentType = new StringValues("text/event-stream");
        Context.Response.Headers.CacheControl = new StringValues("no-cache");
        Context.Response.Headers.Connection = new StringValues("keep-alive");
        Context.Response.Headers["X-Accel-Buffering"] = new StringValues("no");
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