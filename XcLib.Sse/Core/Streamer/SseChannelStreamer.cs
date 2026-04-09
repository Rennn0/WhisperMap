using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using XcLib.Sse.Formatters;

namespace XcLib.Sse.Core.Streamer;

public class SseChannelStreamer : SseStreamer
{
    public SseChannelStreamer(IHttpContextAccessor context, ILoggerFactory factory)
        : base(context, factory, CancellationToken.None) => Logger = LogFactory.CreateLogger<SseChannelStreamer>();

    public SseChannelStreamer(IHttpContextAccessor context, ILoggerFactory factory, CancellationToken cancellationToken)
        : base(context, factory, cancellationToken) => Logger = LogFactory.CreateLogger<SseChannelStreamer>();

    public override async Task StreamAsync<T>(ChannelReader<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter)
    {
        Logger.LogDebug(new EventId((int)SseLogs.StartStream, nameof(SseLogs.StartStream)),
            "Starting SSE streaming for {RequestPath}, event {EventName}", Context.Request.Path, eventName);

        try
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                Task<bool> waitForDataTask = source.WaitToReadAsync(CancellationToken).AsTask();
                Task pingTask = Task.Delay(heartbeatInterval, CancellationToken);
                Task completed = await Task.WhenAny(waitForDataTask, pingTask);

                if (completed == waitForDataTask)
                {
                    if (!await waitForDataTask)
                    {
                        Logger.LogDebug(new EventId((int)SseLogs.EndStream, nameof(SseLogs.EndStream)),
                            "End channel streaming for {RequestPath}, event {EventName}", Context.Request.Path,
                            eventName);
                        break;
                    }

                    while (source.TryRead(out T? update))
                        await SafeWriteAsync(() => WriteEventAsync(eventName, update, formatter));
                }
                else
                {
                    await SafeWriteAsync(() => WriteCommentAsync("ping"));
                }
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug(new EventId((int)SseLogs.ExcStreamCancelled, nameof(SseLogs.ExcStreamCancelled)),
                "Streaming for {RequestPath}, event {EventName} cancelled", Context.Request.Path, eventName);
        }
        catch (Exception e)
        {
            Logger.LogError(new EventId((int)SseLogs.ExcStreamDestroyed, nameof(SseLogs.ExcStreamDestroyed)), e,
                "Streaming for {RequestPath}, event {EventName} destroyed, exception {Exception}",
                Context.Request.Path, eventName, e.Message);
        }
    }
}