using System.Collections.Concurrent;

namespace Realtime.Sse.Core.Stream;

internal abstract partial class SseStreamRegistry<T> : IDisposable, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, StreamHandle> _streams =
        new ConcurrentDictionary<string, StreamHandle>();

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        foreach ((_, StreamHandle handle) in _streams) handle.Dispose();
        _streams.Clear();
        GC.SuppressFinalize(this);
    }

    internal StreamHandle GetStream(string key, CancellationToken cancellationToken = default)
    {
        if (_streams.TryGetValue(key, out StreamHandle? existingHandle)) return existingHandle;

        CancellationTokenRegistration reg =
            cancellationToken.Register(state => _streams.TryRemove(key, out _), this);
        StreamHandle newHandle = new StreamHandle(key, cancellationToken, reg);
        if (_streams.TryAdd(key, newHandle)) return newHandle;

        newHandle.Dispose();
        throw new InvalidOperationException("cannot add stream");
    }

    internal void UnregisterStream(string key)
    {
        if (_streams.TryRemove(key, out StreamHandle? handle)) handle.Dispose();
    }

    internal void UnregisterStream(StreamHandle streamHandle) => UnregisterStream(streamHandle.Key);
}