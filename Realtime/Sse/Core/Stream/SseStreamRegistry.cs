using System.Collections.Concurrent;

namespace Realtime.Sse.Core.Stream;

internal abstract partial class SseStreamRegistry<T> : IDisposable, IAsyncDisposable
{
    private static readonly ConcurrentDictionary<string, StreamHandle> Streams =
        new ConcurrentDictionary<string, StreamHandle>();

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        foreach ((_, StreamHandle handle) in Streams) handle.Dispose();
        Streams.Clear();
        GC.SuppressFinalize(this);
    }

    internal static StreamHandle GetStream(string key, CancellationToken cancellationToken = default)
    {
        if (Streams.TryGetValue(key, out StreamHandle? existingHandle)) return existingHandle;

        StreamHandle newHandle = new StreamHandle(key, cancellationToken);
        if (Streams.TryAdd(key, newHandle)) return newHandle;

        newHandle.Dispose();
        throw new InvalidOperationException("cannot add stream");
    }

    internal static void UnregisterStream(string key)
    {
        if (Streams.TryRemove(key, out StreamHandle? handle)) handle.Dispose();
    }

    internal static void UnregisterStream(StreamHandle streamHandle)
    {
        UnregisterStream(streamHandle.Key);
    }
}