using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace XcLib.Sse.Core.Stream;

public abstract partial class SseStream<T> : IDisposable, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, StreamSubscriber> _subscribers =
        new ConcurrentDictionary<string, StreamSubscriber>();

    private int _disposed;

    protected SseStream(ILoggerFactory loggerFactory) =>
        Logger = loggerFactory.CreateLogger($"XcLib.Stream.{nameof(SseStream<T>)}<{typeof(T).Name}>");

    protected bool IsDisposed => Volatile.Read(ref _disposed) == 1;

    protected ILogger Logger { get; init; }

    public int SubscribersCount => _subscribers.Count;

    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        Dispose();
        return ValueTask.CompletedTask;
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        if (Interlocked.Exchange(ref _disposed, 1) == 1) return;
        foreach (StreamSubscriber subscriber in _subscribers.Values) subscriber.Writer.TryComplete();
        _subscribers.Clear();
        LogDisposedStreamSubscribersSubscriberscount(Logger, SubscribersCount);
    }

    public StreamSubscription Subscribe(string? streamId, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        string id = streamId ?? Guid.NewGuid().ToString("N");

        if (_subscribers.TryGetValue(id, out StreamSubscriber? subscriber))
            return new StreamSubscription(this, subscriber, cancellationToken);

        Channel<T> channel = Channel.CreateBounded<T>(new BoundedChannelOptions(1)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropOldest,
            AllowSynchronousContinuations = false
        });

        subscriber = new StreamSubscriber(id, channel);
        _subscribers[id] = subscriber;

        LogNewSubscriberIdSubsCount(Logger, id, _subscribers.Count);

        return new StreamSubscription(this, subscriber, cancellationToken);
    }

    public ValueTask PublishAsync(T value, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        if (_subscribers.IsEmpty) return ValueTask.CompletedTask;

        int delivered = 0;
        HashSet<string> brokenChannels = new HashSet<string>();
        foreach ((string id, StreamSubscriber subscriber) in _subscribers)
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
            LogBrokenSubsCountDropping(Logger, brokenChannels.Count);
            foreach (string id in brokenChannels)
                RemoveSubscriber(id);
        }

        LogDeliverDeliveredSubsSubs(Logger, delivered, SubscribersCount);

        return ValueTask.CompletedTask;
    }

    public void Unsubscribe(string id)
    {
        RemoveSubscriber(id);
        LogRemovedSubId(Logger, id);
    }

    private void RemoveSubscriber(string id)
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

    [LoggerMessage(LogLevel.Warning, "broken subs {Count}, dropping", EventId = (int)StreamLogs.BrokenSubInfo)]
    protected static partial void LogBrokenSubsCountDropping(ILogger logger, int count);

    [LoggerMessage(LogLevel.Information, "deliver {Delivered}, subs {Subs}", EventId = (int)StreamLogs.Publish)]
    protected static partial void LogDeliverDeliveredSubsSubs(ILogger logger, int delivered, int subs);

    [LoggerMessage(LogLevel.Debug, "disposed stream, subscribers {SubscribersCount}",
        EventId = (int)StreamLogs.Disposed)]
    protected static partial void LogDisposedStreamSubscribersSubscriberscount(ILogger logger, int subscribersCount);

    [LoggerMessage(LogLevel.Debug, "new subscriber {Id}, subs {Count}", EventId = (int)StreamLogs.Subscribe)]
    protected static partial void LogNewSubscriberIdSubsCount(ILogger logger, string id, int count);

    [LoggerMessage(LogLevel.Debug, "removed sub {Id}", EventId = (int)StreamLogs.Unsubscribe)]
    protected static partial void LogRemovedSubId(ILogger logger, string id);
}