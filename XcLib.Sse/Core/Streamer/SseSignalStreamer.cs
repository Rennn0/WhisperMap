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
        IOptionsMonitor<SseSignalOptions> signalOptions,
        SseEventFormatter<T> formatter,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken = default)
        : base(context, signalOptions, formatter, loggerFactory, cancellationToken)
    {
        Logger = loggerFactory.CreateLogger($"XcLib.Streamer.{nameof(SseSignalStreamer<T>)}<{typeof(T).Name}>");
        Logger.LogIntervalA(PingInterval);
    }

    public override async Task StreamAsync(SseSignal<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter)
    {
        Logger.LogStartingSseSignalForAEventB(Url, eventName);

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

                    Logger.LogEndSignalForRequestpathEventEventname(Url, eventName);
                    break;
                }

                await SafeWriteAsync(() => WriteCommentAsync("ping"));
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogSignalingForRequestpathEventEventnameCancelled(Url, eventName);
        }
        catch (Exception e)
        {
            Logger.LogSignalingForRequestpathEventEventnameDestroyedExceptionException(Url, eventName,
                e.Message, e);
        }
    }

    public override Task StreamAsync(IAsyncEnumerable<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter,
        T initialValue = default!) =>
        throw new NotImplementedException();

    public override Task StreamAsync(ChannelReader<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter) => throw new NotImplementedException();
}