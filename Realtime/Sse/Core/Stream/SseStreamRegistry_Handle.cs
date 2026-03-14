namespace Realtime.Sse.Core.Stream;

internal abstract partial class SseStreamRegistry<T>
{
    internal class StreamHandle : SseStream<T>
    {
        private readonly SseStreamRegistry<T> _owner;
        private readonly CancellationTokenRegistration _reg;

        internal StreamHandle(SseStreamRegistry<T> owner, string key, CancellationToken ct)
        {
            Key = key;
            _owner = owner;
            _reg = ct.Register(state => ((StreamHandle)state!).Dispose(), this);
        }

        internal string Key { get; }

        public override void Dispose()
        {
            if (IsDisposed) return;

            try
            {
                _owner.UnregisterStream(Key);
            }
            finally
            {
                _reg.Dispose();
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