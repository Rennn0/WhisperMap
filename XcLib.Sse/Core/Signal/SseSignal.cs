using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace XcLib.Sse.Core.Signal;

public abstract partial class SseSignal<T> : IDisposable, IAsyncDisposable
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<T>> _waiters;

    protected SseSignal(ILoggerFactory loggerFactory)
    {
        _waiters = new ConcurrentDictionary<Guid, TaskCompletionSource<T>>();
        Logger = loggerFactory.CreateLogger($"XcLib.Signal.{nameof(SseSignal<T>)}<{typeof(T).Name}>");
    }

    protected ILogger Logger { get; init; }
    public int Waiters => _waiters.Count;

    public async ValueTask<T> WaitAsync(CancellationToken cancellationToken = default)
    {
        Guid id = Guid.NewGuid();
        TaskCompletionSource<T> waiter =
            new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_waiters.TryAdd(id, waiter))
        {
            Logger.LogCantAddWaiterGuid(id);
            throw new InvalidOperationException();
        }

        Logger.LogAddWaiterGuidTotalWaitersTotal(id, Waiters);

        await using CancellationTokenRegistration registration = cancellationToken.Register(() =>
        {
            if (!_waiters.TryRemove(id, out TaskCompletionSource<T>? removed)) return;

            removed.SetCanceled(cancellationToken);
            Logger.LogCancelledWaiterGuidRemainingWaitersRemaining(id, Waiters);
        });

        try
        {
            return await waiter.Task.ConfigureAwait(false);
        }
        finally
        {
            _waiters.TryRemove(id, out _);
        }
    }

    public ValueTask PublishAsync(T value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_waiters.IsEmpty) return ValueTask.CompletedTask;

        KeyValuePair<Guid, TaskCompletionSource<T>>[] snapshot = _waiters.ToArray();
        int delivered = 0;

        foreach ((Guid id, TaskCompletionSource<T> task) in snapshot)
        {
            if (!_waiters.TryRemove(id, out _)) continue;

            task.SetResult(value);
            delivered++;
        }

        Logger.LogDeliveredToDeliveredWaiterRemainingRemaining(delivered, Waiters);

        return ValueTask.CompletedTask;
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (TaskCompletionSource<T> taskCompletionSource in _waiters.Values) taskCompletionSource.TrySetCanceled();
        _waiters.Clear();
        Logger.LogDisposedSignalWaitersWaiters(Waiters);
    }

    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        Dispose();
        return ValueTask.CompletedTask;
    }
}