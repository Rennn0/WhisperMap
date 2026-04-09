using Microsoft.Extensions.Logging;

namespace XcLib.Sse.Core.Stream;

public partial class SseStreamRegistry<T>
{
    public class StreamHandle : SseStream<T>
    {
        private readonly SseStreamRegistry<T> _owner;
        private readonly CancellationTokenRegistration _reg;

        public StreamHandle(SseStreamRegistry<T> owner, string key, ILoggerFactory loggerFactory, CancellationToken ct)
            : base(loggerFactory)
        {
            Key = key;
            _owner = owner;
            _reg = ct.Register(state => ((StreamHandle)state!).Dispose(), this);
        }

        public string Key { get; }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
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
            GC.SuppressFinalize(this);
            Dispose();
            return base.DisposeAsync();
        }
    }
}