using System.Threading.Channels;

namespace Realtime.Sse.Core.Stream;

internal abstract partial class SseStream<T>
{
    internal sealed class StreamSubscription : IDisposable, IAsyncDisposable
    {
        private readonly CancellationTokenRegistration _cancellationTokenRegistration;
        private readonly SseStream<T> _ownerStream;
        private int _disposed;

        internal StreamSubscription(SseStream<T> ownerStream, StreamSubscriber subscriber,
            CancellationTokenRegistration cancellationTokenRegistration)
        {
            _ownerStream = ownerStream;
            _cancellationTokenRegistration = cancellationTokenRegistration;
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
            _cancellationTokenRegistration.Dispose();
            _ownerStream.Unsubscribe(Id);
        }

        internal IAsyncEnumerable<T> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            return Reader.ReadAllAsync(cancellationToken);
        }
    }
}