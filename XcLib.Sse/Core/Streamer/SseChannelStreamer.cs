using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XcLib.Sse.Core.Signal;
using XcLib.Sse.Formatters;
using XcLib.Sse.Options;

namespace XcLib.Sse.Core.Streamer;

public class SseChannelStreamer<T> : SseStreamer<T>
{
    public SseChannelStreamer(IHttpContextAccessor context,
        IOptionsMonitor<SseOptions> defaultOptions,
        IOptionsMonitor<SseStreamOptions> streamOptions,
        SseEventFormatter<T> formatter,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken = default)
        : base(context, defaultOptions, formatter, loggerFactory, cancellationToken)
    {
        Logger = loggerFactory.CreateLogger($"XcLib.Streamer.{nameof(SseChannelStreamer<T>)}<{typeof(T).Name}>");
        PingInterval = TimeSpan.FromSeconds(streamOptions.CurrentValue.PingInterval);

        Logger.LogTrace("ping interval {X}", PingInterval);
    }

    public override Task StreamAsync(SseSignal<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter) => throw new NotImplementedException();

    public override Task StreamAsync(IAsyncEnumerable<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter, T initialValue = default!) =>
        throw new NotImplementedException();

    public override async Task StreamAsync(ChannelReader<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter)
    {
        Logger.LogDebug(new EventId((int)StreamerLogs.StartStream, nameof(StreamerLogs.StartStream)),
            "Starting SSE streaming for {a}, event {b}", Url, eventName);

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
                        Logger.LogDebug(new EventId((int)StreamerLogs.EndStream, nameof(StreamerLogs.EndStream)),
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
            Logger.LogDebug(new EventId((int)StreamerLogs.ExcStreamCancelled, nameof(StreamerLogs.ExcStreamCancelled)),
                "Streaming for {RequestPath}, event {EventName} cancelled", Context.Request.Path, eventName);
        }
        catch (Exception e)
        {
            Logger.LogError(new EventId((int)StreamerLogs.ExcStreamDestroyed, nameof(StreamerLogs.ExcStreamDestroyed)),
                e,
                "Streaming for {RequestPath}, event {EventName} destroyed, exception {Exception}",
                Context.Request.Path, eventName, e.Message);
        }
    }
}