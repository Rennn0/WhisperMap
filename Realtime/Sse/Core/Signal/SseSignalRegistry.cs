using System.Collections.Concurrent;

namespace Realtime.Sse.Core.Signal;

internal abstract partial class SseSignalRegistry<T> : IDisposable, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, SignalHandle> _signals =
        new ConcurrentDictionary<string, SignalHandle>();

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        foreach ((_, SignalHandle handle) in _signals) handle.Dispose();
        _signals.Clear();
        GC.SuppressFinalize(this);
    }

    internal SignalHandle GetSignal(string key, CancellationToken cancellationToken = default)
    {
        if (_signals.TryGetValue(key, out SignalHandle? existingHandle)) return existingHandle;

        SignalHandle newHandle = new SignalHandle(this, key, cancellationToken);
        if (_signals.TryAdd(key, newHandle)) return newHandle;

        newHandle.Dispose();
        throw new InvalidOperationException("cannot add signal");
    }

    internal void UnregisterSignal(string key)
    {
        if (_signals.TryRemove(key, out SignalHandle? handle)) handle.Dispose();
    }

    internal void UnregisterSignal(SignalHandle signalHandle) => UnregisterSignal(signalHandle.Key);
}