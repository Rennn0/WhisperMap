using System.Threading.Channels;

namespace Realtime;

internal abstract partial class SseStream<T>
{
    internal sealed class StreamSubscriber
    {
        public StreamSubscriber(Guid id, Channel<T> channel)
        {
            Id = id;
            Channel = channel;
        }

        internal Guid Id { get; }
        internal Channel<T> Channel { get; }
    }
}