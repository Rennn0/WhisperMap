using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Realtime.Sse.Core.Stream;

internal abstract partial class SseStream<T> : IDisposable, IAsyncDisposable
{
    private readonly ConcurrentDictionary<Guid, StreamSubscriber> _subscribers =
        new ConcurrentDictionary<Guid, StreamSubscriber>();

    private int _disposed;

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
        Logger = loggerFactory.CreateLogger<SseStream<T>>();
    }

    internal bool IsDisposed => Volatile.Read(ref _disposed) == 1;

    protected ILogger<SseStream<T>> Logger { get; init; }

    internal int SubscribersCount => _subscribers.Count;

    public virtual ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public virtual void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1) return;
        foreach (StreamSubscriber subscriber in _subscribers.Values) subscriber.Writer.TryComplete();
        _subscribers.Clear();
        Logger.LogDebug(
            new EventId((int)StreamLogs.Disposed, nameof(StreamLogs.Disposed)),
            "Disposed stream, subscribers {SubscribersCount}",
            SubscribersCount);
    }

    internal StreamSubscription Subscribe(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        Guid id = Guid.NewGuid();
        Channel<T> channel = Channel.CreateBounded<T>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropOldest,
            AllowSynchronousContinuations = false
        });
        StreamSubscriber subscriber = new StreamSubscriber(id, channel);
        if (!_subscribers.TryAdd(id, subscriber))
        {
            channel.Writer.TryComplete();
            throw new InvalidOperationException("Cannot add stream subscriber");
        }

        Logger.LogDebug(new EventId((int)StreamLogs.Subscribe, nameof(StreamLogs.Subscribe)),
            "New subscriber {Id}, subs {Count}", id,
            _subscribers.Count);

        return new StreamSubscription(this, subscriber, cancellationToken);
    }

    internal ValueTask PublishAsync(T value, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        if (_subscribers.IsEmpty) return ValueTask.CompletedTask;
        Logger.LogDebug(new EventId((int)StreamLogs.Publish, nameof(StreamLogs.Publish)),
            "Pub, subs {Count}", _subscribers.Count);

        int delivered = 0;
        HashSet<Guid> brokenChannels = new HashSet<Guid>();
        foreach ((Guid id, StreamSubscriber subscriber) in _subscribers)
        {
            ThrowIfDisposed();
            if (subscriber.Writer.TryWrite(value))
            {
                delivered++;
                continue;
            }

            brokenChannels.Add(id);
        }

        if (brokenChannels.Count > 0)
        {
            Logger.LogWarning(new EventId((int)StreamLogs.BrokenSubInfo, nameof(StreamLogs.BrokenSubInfo)),
                "Broken subs {Count}, dropping", brokenChannels.Count);
            foreach (Guid id in brokenChannels)
                RemoveSubscriber(id);
        }

        Logger.LogInformation(new EventId((int)StreamLogs.Publish, nameof(StreamLogs.Publish)),
            "Deliver {Delivered}, subs {Subs}", delivered, SubscribersCount);

        return ValueTask.CompletedTask;
    }

    internal void Unsubscribe(Guid id)
    {
        RemoveSubscriber(id);
        Logger.LogDebug(new EventId((int)StreamLogs.Unsubscribe, nameof(StreamLogs.Unsubscribe)),
            "Removed sub {Id}", id);
    }

    private void RemoveSubscriber(Guid id)
    {
        if (_subscribers.TryRemove(id, out StreamSubscriber? subscriber)) subscriber.Writer.TryComplete();
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(IsDisposed, this);

    private enum StreamLogs
    {
        Subscribe = 100,
        Unsubscribe,
        Publish,
        BrokenSub,
        BrokenSubInfo,
        Disposed
    }
}