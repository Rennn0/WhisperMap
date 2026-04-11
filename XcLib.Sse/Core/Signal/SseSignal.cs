using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace XcLib.Sse.Core.Signal;

public abstract partial class SseSignal<T> : IDisposable, IAsyncDisposable
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<T>> _waiters;

    protected SseSignal(ILoggerFactory loggerFactory)
    {
        _waiters = new ConcurrentDictionary<Guid, TaskCompletionSource<T>>();
        _logger = loggerFactory.CreateLogger($"XcLib.Signal.{nameof(SseSignal<T>)}<{typeof(T).Name}>");
    }

    public int Waiters => _waiters.Count;

    public async ValueTask<T> WaitAsync(CancellationToken cancellationToken = default)
    {
        Guid id = Guid.NewGuid();
        TaskCompletionSource<T> waiter =
            new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_waiters.TryAdd(id, waiter))
        {
            LogCantAddWaiterGuid(id);
            throw new InvalidOperationException();
        }

        LogAddWaiterGuidTotalWaitersTotal(id, Waiters);

        await using CancellationTokenRegistration registration = cancellationToken.Register(() =>
        {
            if (!_waiters.TryRemove(id, out TaskCompletionSource<T>? removed)) return;

            removed.SetCanceled(cancellationToken);
            LogCancelledWaiterGuidRemainingWaitersRemaining(id, Waiters);
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

        LogDeliveredToDeliveredWaiterRemainingRemaining(delivered, Waiters);

        return ValueTask.CompletedTask;
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (TaskCompletionSource<T> taskCompletionSource in _waiters.Values) taskCompletionSource.TrySetCanceled();
        _waiters.Clear();
        LogDisposedSignalWaitersWaiters(Waiters);
    }

    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        Dispose();
        return ValueTask.CompletedTask;
    }

    private enum SignalLogs
    {
        Wait = 200,
        CancelledWaiter,
        CompletedWaiters,
        ExcCannotAdd,
        Dispose
    }

    [LoggerMessage(LogLevel.Error, "cant add waiter {Guid}", EventId = (int)SignalLogs.ExcCannotAdd)]
    protected partial void LogCantAddWaiterGuid(Guid guid);

    [LoggerMessage(LogLevel.Debug, "add waiter {Guid}, total waiters {Total}", EventId = (int)SignalLogs.Wait)]
    protected partial void LogAddWaiterGuidTotalWaitersTotal(Guid guid, int total);

    [LoggerMessage(LogLevel.Debug, "cancelled waiter {Guid}, remaining waiters {Remaining}",
        EventId = (int)SignalLogs.CancelledWaiter)]
    protected partial void LogCancelledWaiterGuidRemainingWaitersRemaining(Guid guid, int remaining);

    [LoggerMessage(LogLevel.Debug, "delivered to {Delivered} waiter, remaining {Remaining}",
        EventId = (int)SignalLogs.CompletedWaiters)]
    protected partial void LogDeliveredToDeliveredWaiterRemainingRemaining(int delivered, int remaining);

    [LoggerMessage(LogLevel.Debug, "disposed signal, waiters {Waiters}", EventId = (int)SignalLogs.Dispose)]
    protected partial void LogDisposedSignalWaitersWaiters(int waiters);
}