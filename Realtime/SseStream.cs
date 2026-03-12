using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Realtime;

internal abstract partial class SseStream<T>
{
    private enum StreamLogs
    {
        Subscribe = 100,
        Unsubscribe,
        Publish,
        BrokenSub,
        BrokenSubInfo
    }

    private readonly ConcurrentDictionary<Guid, StreamSubscriber> _subscribers =
        new ConcurrentDictionary<Guid, StreamSubscriber>();

    private readonly ILogger<SseStream<T>> _logger;

    internal SseStream()
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug).AddSimpleConsole(opt =>
            {
                opt.IncludeScopes = true;
                opt.SingleLine = true;
                opt.TimestampFormat = "[HH:mm:ss] ";
            });
        });
        _logger = loggerFactory.CreateLogger<SseStream<T>>();
    }

    internal StreamSubscription Subscribe()
    {
        Guid id = Guid.NewGuid();
        Channel<T> channel = Channel.CreateBounded<T>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropOldest,
            AllowSynchronousContinuations = false
        });
        StreamSubscriber subscriber = new StreamSubscriber(id, channel);
        _subscribers[id] = subscriber;

        _logger.LogDebug(new EventId((int)StreamLogs.Subscribe, nameof(StreamLogs.Subscribe)),
            "New subscriber {Id}, subs {Count}", id,
            _subscribers.Count);
        return new StreamSubscription(this, subscriber);
    }

    internal async ValueTask PublishAsync(T value, CancellationToken cancellationToken = default)
    {
        if (_subscribers.IsEmpty) return;
        _logger.LogDebug(new EventId((int)StreamLogs.Publish, nameof(StreamLogs.Publish)),
            "Pub, subs {Count}", _subscribers.Count);
        HashSet<Guid> brokenChannels = new HashSet<Guid>();
        foreach ((Guid id, StreamSubscriber subscriber) in _subscribers)
            try
            {
                await subscriber.Channel.Writer.WriteAsync(value, cancellationToken);
            }
            catch
            {
                _logger.LogError(new EventId((int)StreamLogs.BrokenSub, nameof(StreamLogs.BrokenSub)),
                    "Sub {Id} broken, dropping", id);
                brokenChannels.Add(id);
            }

        if (brokenChannels.Count > 0)
        {
            _logger.LogWarning(new EventId((int)StreamLogs.BrokenSubInfo, nameof(StreamLogs.BrokenSubInfo)),
                "Broken subs {Count}, dropping", brokenChannels.Count);
            foreach (Guid id in brokenChannels)
                RemoveSubscriber(id);
        }
    }

    internal void Unsubscribe(Guid id)
    {
        RemoveSubscriber(id);
        _logger.LogDebug(new EventId((int)StreamLogs.Unsubscribe, nameof(StreamLogs.Unsubscribe)),
            "Removing sub {Id}", id);
    }

    private void RemoveSubscriber(Guid id)
    {
        if (_subscribers.TryRemove(id, out StreamSubscriber? subscriber)) subscriber.Channel.Writer.TryComplete();
    }
}