namespace Realtime.Sse.Core.Signal;

internal abstract partial class SseSignalRegistry<T>
{
    internal class SignalHandle : SseSignal<T>
    {
        private readonly CancellationTokenSource _cts;
        private readonly SseSignalRegistry<T> _owner;
        private readonly CancellationTokenRegistration _reg;
        private int _disposed;

        internal SignalHandle(SseSignalRegistry<T> owner, string key, CancellationToken ct)
        {
            _owner = owner;
            Key = key;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _reg = ct.Register(state =>
            {
                SignalHandle self = (SignalHandle)state!;
                self.Dispose();
            }, this);
        }

        internal string Key { get; }
        internal CancellationToken Token => _cts.Token;
        internal bool IsDisposed => Volatile.Read(ref _disposed) == 1;

        public override void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

            try
            {
                _owner.UnregisterSignal(Key);
                _cts.Cancel();
            }
            finally
            {
                _reg.Dispose();
                _cts.Dispose();
                base.Dispose();
            }
        }

        public override ValueTask DisposeAsync()
        {
            Dispose();
            return base.DisposeAsync();
        }
    }
}