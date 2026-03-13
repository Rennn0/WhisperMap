using System.Threading.Channels;

namespace Realtime.Sse.Core.Stream;

internal abstract partial class SseStream<T>
{
    internal sealed class StreamSubscription : IDisposable, IAsyncDisposable
    {
        private readonly SseStream<T> _ownerStream;
        private readonly CancellationTokenRegistration _reg;
        private int _disposed;

        internal StreamSubscription(SseStream<T> ownerStream, StreamSubscriber subscriber,
            CancellationTokenRegistration reg)
        {
            _ownerStream = ownerStream;
            _reg = reg;
            Id = subscriber.Id;
            Reader = subscriber.Reader;
        }

        internal Guid Id { get; }
        internal ChannelReader<T> Reader { get; }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;
            _reg.Dispose();
            _ownerStream.Unsubscribe(Id);
        }

        internal IAsyncEnumerable<T> ReadAllAsync(CancellationToken cancellationToken = default) =>
            Reader.ReadAllAsync(cancellationToken);
    }
}