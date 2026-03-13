using System.Collections.Concurrent;

namespace Realtime.Sse.Core.Signal;

internal abstract class SseSignalRegistry<T> : IDisposable, IAsyncDisposable
{
    private static readonly ConcurrentDictionary<string, SignalHandle> Signals =
        new ConcurrentDictionary<string, SignalHandle>();

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

    internal static void UnregisterSignal(SignalHandle signalHandle) => UnregisterSignal(signalHandle.Key);

    public void Dispose()
    {
        foreach ((_, SignalHandle handle) in Signals) handle.Dispose();
        Signals.Clear();
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    internal class SignalHandle : SseSignal<T>
    {
        private readonly CancellationTokenSource _cts;
        private readonly CancellationTokenRegistration _reg;
        private int _disposed;
        internal string Key { get; }
        internal CancellationToken Token => _cts.Token;

        internal SignalHandle(string key, CancellationToken ct)
        {
            Key = key;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _reg = _cts.Token.Register(static state =>
            {
                SignalHandle self = (SignalHandle)state!;
                Signals.TryRemove(self.Key, out _);
            }, this);
        }

        public override void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

            try
            {
                _cts.Cancel();
            }
            finally
            {
                _reg.Dispose();
                _cts.Dispose();
                base.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        public override ValueTask DisposeAsync()
        {
            Dispose();
            return base.DisposeAsync();
        }
    }
}