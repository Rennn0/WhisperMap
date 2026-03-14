using Realtime.Sse.Core.Signal;
using Realtime.Sse.Formatters;

namespace Realtime.Sse.Core.Streamer;

internal class SseSignalStreamer : SseStreamer
{
    internal SseSignalStreamer(HttpContext context)
        : base(context, context.RequestAborted) => Logger = LogFactory.CreateLogger<SseSignalStreamer>();

    internal SseSignalStreamer(HttpContext context, CancellationToken cancellationToken)
        : base(context, cancellationToken) => Logger = LogFactory.CreateLogger<SseSignalStreamer>();

    internal async Task StreamAsync<T>(SseSignal<T> source, string eventName, TimeSpan heartbeatInterval,
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
                    await WriteEventAsync(eventName, data, formatter);

                    Logger.LogDebug(new EventId((int)SseLogs.EndSignal, nameof(SseLogs.EndSignal)),
                        "End signal for {RequestPath}, event {EventName}", Context.Request.Path,
                        eventName);
                    break;
                }

                await WriteCommentAsync("ping");
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug(new EventId((int)SseLogs.ExcSignalCancelled, nameof(SseLogs.ExcSignalCancelled)),
                "Signaling for {RequestPath}, event {EventName} cancelled", Context.Request.Path, eventName);
        }
        catch (Exception e)
        {
            Logger.LogError(new EventId((int)SseLogs.ExcSignalDestroyed, nameof(SseLogs.ExcSignalDestroyed)),
                "Signaling for {RequestPath}, event {EventName} destroyed, exception {Exception}",
                Context.Request.Path, eventName, e.Message);
        }
    }
}