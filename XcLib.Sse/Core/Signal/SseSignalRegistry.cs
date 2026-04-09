using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XcLib.Sse.Options;

namespace XcLib.Sse.Core.Signal;

public partial class SseSignalRegistry<T> : IDisposable, IAsyncDisposable
{
    private readonly ILoggerFactory _loggerFactory;

    private readonly ConcurrentDictionary<string, SignalHandle> _signals =
        new ConcurrentDictionary<string, SignalHandle>();

    private readonly SseSignalOptions _optionsSnapshot;

    public SseSignalRegistry(IOptionsMonitor<SseSignalOptions> optionsMonitor, ILoggerFactory loggerFactory)
    {
        _optionsSnapshot = optionsMonitor.CurrentValue;
        _loggerFactory = loggerFactory;
    }

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

    public SignalHandle GetSignal(string key, CancellationToken cancellationToken = default)
    {
        if (_signals.TryGetValue(key, out SignalHandle? existingHandle)) return existingHandle;

        SignalHandle newHandle = new SignalHandle(this, key, _loggerFactory, cancellationToken);
        if (_signals.TryAdd(key, newHandle)) return newHandle;

        newHandle.Dispose();
        throw new InvalidOperationException("cannot add signal");
    }

    public void UnregisterSignal(string key)
    {
        if (_signals.TryRemove(key, out SignalHandle? handle)) handle.Dispose();
    }

    public void UnregisterSignal(SignalHandle signalHandle) => UnregisterSignal(signalHandle.Key);
}