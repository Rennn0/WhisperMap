using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XcLib.Sse.Options;

namespace XcLib.Sse.Core.Stream;

public partial class SseStreamRegistry<T> : IDisposable, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, StreamHandle> _streams =
        new ConcurrentDictionary<string, StreamHandle>();

    private readonly IOptionsMonitor<SseStreamOptions> _streamOptions;
    private readonly ILoggerFactory _loggerFactory;
    protected SseStreamOptions SseStreamOptions => _streamOptions.CurrentValue;
    protected ILogger Logger { get; init; }
    public SseStreamRegistry(IOptionsMonitor<SseStreamOptions> optionsMonitor, ILoggerFactory loggerFactory)
    {
        _streamOptions = optionsMonitor;
        _loggerFactory = loggerFactory;
        Logger = _loggerFactory.CreateLogger($"XcLib.Stream.{nameof(SseStreamRegistry<T>)}<{typeof(T).Name}>");
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        Dispose();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach ((_, StreamHandle handle) in _streams) handle.Dispose();
        _streams.Clear();
    }

    public StreamHandle GetStream(string eventKey, CancellationToken cancellationToken)
    {
        if (_streams.TryGetValue(eventKey, out StreamHandle? existingHandle)) return existingHandle;

        StreamHandle newHandle = new StreamHandle(this, eventKey, _loggerFactory, cancellationToken);
        if (_streams.TryAdd(eventKey, newHandle)) return newHandle;

        newHandle.Dispose();
        throw new InvalidOperationException("cannot add stream");
    }

    public void UnregisterStream(string key)
    {
        if (_streams.TryRemove(key, out StreamHandle? _))
        {
        }
    }

    public void UnregisterStream(StreamHandle streamHandle) => UnregisterStream(streamHandle.Key);
}