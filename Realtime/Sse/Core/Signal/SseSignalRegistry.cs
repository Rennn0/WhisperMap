using System.Collections.Concurrent;

namespace Realtime.Sse.Core.Signal;

internal abstract partial class SseSignalRegistry<T> : IDisposable, IAsyncDisposable
{
    private static readonly ConcurrentDictionary<string, SignalHandle> Signals =
        new ConcurrentDictionary<string, SignalHandle>();

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        foreach ((_, SignalHandle handle) in Signals) handle.Dispose();
        Signals.Clear();
        GC.SuppressFinalize(this);
    }

    internal static SignalHandle GetSignal(string key, CancellationToken cancellationToken = default)
    {
        if (Signals.TryGetValue(key, out SignalHandle? existingHandle)) return existingHandle;

        SignalHandle newHandle = new SignalHandle(key, cancellationToken);
        if (Signals.TryAdd(key, newHandle)) return newHandle;

        newHandle.Dispose();
        throw new InvalidOperationException("cannot add signal");
    }

    internal static void UnregisterSignal(string key)
    {
        if (Signals.TryRemove(key, out SignalHandle? handle)) handle.Dispose();
    }

    internal static void UnregisterSignal(SignalHandle signalHandle)
    {
        UnregisterSignal(signalHandle.Key);
    }
}