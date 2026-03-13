using System.Threading.Channels;

namespace Realtime.Sse.Core.Stream;

internal abstract partial class SseStream<T>
{
    internal sealed class StreamSubscription : IDisposable, IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts;
        private readonly SseStream<T> _ownerStream;
        private readonly CancellationTokenRegistration _reg;
        private int _disposed;

        internal StreamSubscription(SseStream<T> ownerStream, StreamSubscriber subscriber,
            CancellationToken cancellationToken)
        {
            _ownerStream = ownerStream;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _reg = cancellationToken.Register(state =>
            {
                StreamSubscription self = (StreamSubscription)state!;
                self.Dispose();
            }, this);
            Id = subscriber.Id;
            Reader = subscriber.Reader;
        }

        internal Guid Id { get; }
        internal ChannelReader<T> Reader { get; }
        internal bool IsDisposed => Volatile.Read(ref _disposed) == 1;

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;
            _ownerStream.Unsubscribe(Id);
            _cts.Cancel();
            _reg.Dispose();
        }

        internal IAsyncEnumerable<T> ReadAllAsync(CancellationToken cancellationToken = default) =>
            Reader.ReadAllAsync(cancellationToken);
    }
}