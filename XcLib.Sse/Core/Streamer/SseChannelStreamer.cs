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
        IOptionsMonitor<SseStreamOptions> streamOptions,
        SseEventFormatter<T> formatter,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken = default)
        : base(context, streamOptions, formatter, loggerFactory, cancellationToken)
    {
        Logger = loggerFactory.CreateLogger($"XcLib.Streamer.{nameof(SseChannelStreamer<T>)}<{typeof(T).Name}>");
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
        Logger.LogStartStreamingForAEventEventname(Url, eventName);

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
                        Logger.LogEndStreamingForRequestpathEventEventname(Url, eventName);
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
            Logger.LogStreamingForRequestpathEventEventnameCancelled(Url, eventName);
        }
        catch (Exception e)
        {
            Logger.LogStreamingForRequestpathEventEventnameDestroyedExceptionException(Url, eventName,
                e.Message, e);
        }
    }
}