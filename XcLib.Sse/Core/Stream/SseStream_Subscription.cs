using System.Threading.Channels;

namespace XcLib.Sse.Core.Stream;

public abstract partial class SseStream<T>
{
    public sealed class StreamSubscription : IDisposable, IAsyncDisposable
    {
        private readonly SseStream<T> _ownerStream;
        private readonly CancellationTokenRegistration _reg;
        private int _disposed;

        public StreamSubscription(SseStream<T> ownerStream, StreamSubscriber subscriber,
            CancellationToken cancellationToken)
        {
            Id = subscriber.Id;
            Reader = subscriber.Reader;
            _ownerStream = ownerStream;
            _reg = cancellationToken.Register(state => ((StreamSubscription)state!).Dispose(), this);
        }

        public string Id { get; }
        public ChannelReader<T> Reader { get; }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;
            _ownerStream.Unsubscribe(Id);
            _reg.Dispose();
        }

        public IAsyncEnumerable<T> ReadAllAsync(CancellationToken cancellationToken = default) =>
            Reader.ReadAllAsync(cancellationToken);
    }
}