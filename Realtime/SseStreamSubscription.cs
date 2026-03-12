using System.Threading.Channels;

namespace Realtime;

internal abstract partial class SseStream<T>
{
    internal sealed class StreamSubscription : IDisposable, IAsyncDisposable
    {
        private readonly SseStream<T> _sseStream;
        private int _disposed;

        internal StreamSubscription(SseStream<T> ownerStream, StreamSubscriber subscriber)
        {
            _sseStream = ownerStream;
            Id = subscriber.Id;
            Reader = subscriber.Channel.Reader;
        }

        internal Guid Id { get; }
        internal ChannelReader<T> Reader { get; }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

            _sseStream.Unsubscribe(Id);
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}