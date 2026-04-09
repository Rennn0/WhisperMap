using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using XcLib.Sse.Core.Signal;
using XcLib.Sse.Formatters;

namespace XcLib.Sse.Core.Streamer;

public class SseSignalStreamer : SseStreamer
{
    public SseSignalStreamer(IHttpContextAccessor context, ILoggerFactory factory)
        : base(context, factory, CancellationToken.None) => Logger = LogFactory.CreateLogger<SseSignalStreamer>();

    public SseSignalStreamer(IHttpContextAccessor context, ILoggerFactory factory, CancellationToken cancellationToken)
        : base(context, factory, cancellationToken) => Logger = LogFactory.CreateLogger<SseSignalStreamer>();

    public override async Task StreamAsync<T>(SseSignal<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter)
    {
        Logger.LogDebug(new EventId((int)SseLogs.StartSignal, nameof(SseLogs.StartSignal)),
            "Starting SSE signal for {RequestPath}, event {EventName}", Context.Request.Path, eventName);

        try
        {
            Task<T> signalTask = source.WaitAsync(CancellationToken).AsTask();

            while (!CancellationToken.IsCancellationRequested)
            {
                Task pingTask = Task.Delay(heartbeatInterval, CancellationToken);
                Task completed = await Task.WhenAny(signalTask, pingTask);

                if (completed == signalTask)
                {
                    T data = await signalTask;
                    await SafeWriteAsync(() => WriteEventAsync(eventName, data, formatter));

                    Logger.LogDebug(new EventId((int)SseLogs.EndSignal, nameof(SseLogs.EndSignal)),
                        "End signal for {RequestPath}, event {EventName}", Context.Request.Path,
                        eventName);
                    break;
                }

                await SafeWriteAsync(() => WriteCommentAsync("ping"));
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug(new EventId((int)SseLogs.ExcSignalCancelled, nameof(SseLogs.ExcSignalCancelled)),
                "Signaling for {RequestPath}, event {EventName} cancelled", Context.Request.Path, eventName);
        }
        catch (Exception e)
        {
            Logger.LogError(new EventId((int)SseLogs.ExcSignalDestroyed, nameof(SseLogs.ExcSignalDestroyed)), e,
                "Signaling for {RequestPath}, event {EventName} destroyed, exception {Exception}",
                Context.Request.Path, eventName, e.Message);
        }
    }
}