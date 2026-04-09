using Microsoft.Extensions.Logging;

namespace XcLib.Sse.Core.Signal;

public partial class SseSignalRegistry<T>
{
    public class SignalHandle : SseSignal<T>
    {
        private readonly CancellationTokenSource _cts;
        private readonly SseSignalRegistry<T> _owner;
        private readonly CancellationTokenRegistration _reg;
        private int _disposed;

        public SignalHandle(SseSignalRegistry<T> owner, string key, ILoggerFactory loggerFactory, CancellationToken ct)
            : base(loggerFactory)
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

        public string Key { get; }
        public CancellationToken Token => _cts.Token;
        public bool IsDisposed => Volatile.Read(ref _disposed) == 1;

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
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
            GC.SuppressFinalize(this);
            Dispose();
            return base.DisposeAsync();
        }
    }
}