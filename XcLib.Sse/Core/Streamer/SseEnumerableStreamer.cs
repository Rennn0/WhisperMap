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
        IOptionsMonitor<SseStreamOptions> streamOptions,
        SseEventFormatter<T> formatter,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken = default) :
        base(context, streamOptions, formatter, loggerFactory, cancellationToken)
    {
        Logger = loggerFactory.CreateLogger($"XcLib.Streamer.{nameof(SseEnumerableStreamer<T>)}<{typeof(T).Name}>");
        Logger.LogIntervalA(PingInterval);
    }

    public override Task StreamAsync(SseSignal<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter) => throw new NotImplementedException();

    public override async Task StreamAsync(IAsyncEnumerable<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter, T initialValue = default!)
    {
        Logger.LogStartStreamingForAEventEventname(Url, eventName);

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
                heartbeatInterval = OptionsChanged ? PingInterval : heartbeatInterval;
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
            Logger.LogStreamingForRequestpathEventEventnameCancelled(Url, eventName);
        }
        catch (Exception e)
        {
            Logger.LogStreamingForRequestpathEventEventnameDestroyedExceptionException(Url, eventName,
                e.Message, e);
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

            Logger.LogEndStreamingForRequestpathEventEventname(Url, eventName);
        }
    }

    public override Task StreamAsync(ChannelReader<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter) => throw new NotImplementedException();
}