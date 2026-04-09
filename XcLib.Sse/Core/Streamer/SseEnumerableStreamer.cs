using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XcLib.Sse.Core.Signal;
using XcLib.Sse.Formatters;
using XcLib.Sse.Options;

namespace XcLib.Sse.Core.Streamer;

public class SseEnumerableStreamer<T> : SseStreamer<T>
{
    public SseEnumerableStreamer(IHttpContextAccessor context,
        IOptionsMonitor<SseOptions> defaultOptions,
        IOptionsMonitor<SseStreamOptions> streamOptions,
        SseEventFormatter<T> formatter,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken = default) :
        base(context, defaultOptions, formatter, loggerFactory, cancellationToken)
    {
        Logger = loggerFactory.CreateLogger($"XcLib.Streamer.{nameof(SseEnumerableStreamer<T>)}<{typeof(T).Name}>");
        PingInterval = TimeSpan.FromSeconds(streamOptions.CurrentValue.PingInterval);

        Logger.LogTrace("ping interval {X}", PingInterval);
    }

    public override Task StreamAsync(SseSignal<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter) => throw new NotImplementedException();

    public override async Task StreamAsync(IAsyncEnumerable<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter, T initialValue = default!)
    {
        Logger.LogDebug(new EventId((int)StreamerLogs.StartStream, nameof(StreamerLogs.StartStream)),
            "Starting SSE streaming for {a}, event {EventName}", Url, eventName);

        IAsyncEnumerator<T>? enumerator = null;
        Task<bool>? moveNextTask = null;

        try
        {
            if (initialValue is not null)
                await SafeWriteAsync(() => WriteEventAsync(eventName, initialValue, formatter));
            
            enumerator = source.GetAsyncEnumerator(CancellationToken);
            moveNextTask = enumerator.MoveNextAsync().AsTask();

            while (!CancellationToken.IsCancellationRequested)
            {
                Task pingTask = Task.Delay(heartbeatInterval, CancellationToken);
                Task completed = await Task.WhenAny(moveNextTask, pingTask);
                CancellationToken.ThrowIfCancellationRequested();

                if (completed == moveNextTask)
                {
                    if (!await moveNextTask) break;

                    await SafeWriteAsync(() => WriteEventAsync(eventName, enumerator.Current, formatter));
                    moveNextTask = enumerator.MoveNextAsync().AsTask();
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
        finally
        {
            if (moveNextTask is { IsCompleted: false })
                try
                {
                    await moveNextTask;
                }
                catch
                {
                    // ignored
                }

            if (enumerator is not null)
                try
                {
                    await enumerator.DisposeAsync();
                }
                catch
                {
                    // ignored
                }
        }
    }

    public override Task StreamAsync(ChannelReader<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter) => throw new NotImplementedException();
}