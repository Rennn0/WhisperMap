namespace Realtime.Sse.Core.Stream;

internal abstract partial class SseStreamRegistry<T>
{
    internal class StreamHandle : SseStream<T>
    {
        private readonly CancellationTokenSource _cts;
        private readonly SseStreamRegistry<T> _owner;
        private readonly CancellationTokenRegistration _reg;
        private int _disposed;

        internal StreamHandle(SseStreamRegistry<T> owner, string key, CancellationToken ct)
        {
            _owner = owner;
            Key = key;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _reg = ct.Register(state =>
            {
                StreamHandle self = (StreamHandle)state!;
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
                _owner.UnregisterStream(Key);
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