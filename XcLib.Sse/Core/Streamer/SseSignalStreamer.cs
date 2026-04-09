using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XcLib.Sse.Core.Signal;
using XcLib.Sse.Formatters;
using XcLib.Sse.Options;

namespace XcLib.Sse.Core.Streamer;

public class SseSignalStreamer<T> : SseStreamer<T>
{
    public SseSignalStreamer(IHttpContextAccessor context,
        IOptionsMonitor<SseOptions> defaultOptions,
        IOptionsMonitor<SseSignalOptions> signalOptions,
        SseEventFormatter<T> formatter,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken = default)
        : base(context, defaultOptions, formatter, loggerFactory, cancellationToken)
    {
        Logger = loggerFactory.CreateLogger($"XcLib.Streamer.{nameof(SseSignalStreamer<T>)}<{typeof(T).Name}>");
        PingInterval = TimeSpan.FromSeconds(signalOptions.CurrentValue.PingInterval);

        Logger.LogTrace("ping interval {X}", PingInterval);
    }

    public override async Task StreamAsync(SseSignal<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter)
    {
        Logger.LogDebug(new EventId((int)StreamerLogs.StartSignal, nameof(StreamerLogs.StartSignal)),
            "Starting SSE signal for {a}, event {b}", Url, eventName);

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

                    Logger.LogDebug(new EventId((int)StreamerLogs.EndSignal, nameof(StreamerLogs.EndSignal)),
                        "End signal for {RequestPath}, event {EventName}", Context.Request.Path,
                        eventName);
                    break;
                }

                await SafeWriteAsync(() => WriteCommentAsync("ping"));
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug(new EventId((int)StreamerLogs.ExcSignalCancelled, nameof(StreamerLogs.ExcSignalCancelled)),
                "Signaling for {RequestPath}, event {EventName} cancelled", Context.Request.Path, eventName);
        }
        catch (Exception e)
        {
            Logger.LogError(new EventId((int)StreamerLogs.ExcSignalDestroyed, nameof(StreamerLogs.ExcSignalDestroyed)),
                e,
                "Signaling for {RequestPath}, event {EventName} destroyed, exception {Exception}",
                Context.Request.Path, eventName, e.Message);
        }
    }

    public override Task StreamAsync(IAsyncEnumerable<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter,
        T initialValue = default!) =>
        throw new NotImplementedException();

    public override Task StreamAsync(ChannelReader<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter) => throw new NotImplementedException();
}