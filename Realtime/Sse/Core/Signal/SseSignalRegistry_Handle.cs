namespace Realtime.Sse.Core.Signal;

internal abstract partial class SseSignalRegistry<T>
{
    internal class SignalHandle : SseSignal<T>
    {
        private readonly CancellationTokenSource _cts;
        private readonly CancellationTokenRegistration _reg;
        private int _disposed;

        internal SignalHandle(string key, CancellationToken ct, CancellationTokenRegistration reg)
        {
            Key = key;
            _reg = reg;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        }

        internal string Key { get; }
        internal CancellationToken Token => _cts.Token;

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