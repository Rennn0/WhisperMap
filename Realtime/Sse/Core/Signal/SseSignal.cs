using System.Collections.Concurrent;

namespace Realtime.Sse.Core.Signal;

internal abstract class SseSignal<T> : IDisposable, IAsyncDisposable
{
    private enum SignalLogs
    {
        Wait = 200,
        CancelledWaiter,
        CompletedWaiters,
        ExcCannotAdd,
        Dispose
    }

    private readonly ILogger<SseSignal<T>> _logger;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<T>> _waiters;

    internal SseSignal()
    {
        _waiters = new ConcurrentDictionary<Guid, TaskCompletionSource<T>>();
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug).AddSimpleConsole(opt =>
            {
                opt.IncludeScopes = true;
                opt.SingleLine = true;
                opt.TimestampFormat = "[HH:mm:ss] ";
            });
        });
        _logger = loggerFactory.CreateLogger<SseSignal<T>>();
    }

    internal int Waiters => _waiters.Count;

    internal async ValueTask<T> WaitAsync(CancellationToken cancellationToken = default)
    {
        Guid id = Guid.NewGuid();
        TaskCompletionSource<T> waiter =
            new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_waiters.TryAdd(id, waiter))
        {
            _logger.LogError(new EventId((int)SignalLogs.ExcCannotAdd, nameof(SignalLogs.ExcCannotAdd)),
                "Cant add waiter '{Guid}'.", id);
            throw new InvalidOperationException();
        }

        _logger.LogDebug(new EventId((int)SignalLogs.Wait, nameof(SignalLogs.Wait)),
            "Add waiter '{Guid}', total waiters {Total}", id, Waiters);

        await using CancellationTokenRegistration registration = cancellationToken.Register(() =>
        {
            if (!_waiters.TryRemove(id, out TaskCompletionSource<T>? removed)) return;

            removed.SetCanceled(cancellationToken);
            _logger.LogDebug(new EventId((int)SignalLogs.CancelledWaiter, nameof(SignalLogs.CancelledWaiter)),
                "Cancelled waiter {Guid}, remaining waiters {Remaining}", id, Waiters);
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

    internal ValueTask PublishAsync(T value, CancellationToken cancellationToken = default)
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

        _logger.LogDebug(new EventId((int)SignalLogs.CompletedWaiters, nameof(SignalLogs.CompletedWaiters)),
            "Delivered to {Delivered} waiter, remaining {Remaining}", delivered, Waiters);

        return ValueTask.CompletedTask;
    }

    public virtual void Dispose()
    {
        foreach (TaskCompletionSource<T> taskCompletionSource in _waiters.Values) taskCompletionSource.TrySetCanceled();
        _waiters.Clear();
        _logger.LogDebug(new EventId((int)SignalLogs.Dispose, nameof(SignalLogs.Dispose)),
            "Disposed signal, waiters {Waiters}", Waiters);
    }

    public virtual ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}