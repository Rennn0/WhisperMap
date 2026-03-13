using System.Threading.Channels;

namespace Realtime.Sse.Core.Stream;

internal abstract partial class SseStream<T>
{
    internal sealed class StreamSubscriber
    {
        public StreamSubscriber(Guid id, Channel<T> channel)
        {
            Id = id;
            Reader = channel.Reader;
            Writer = channel.Writer;
        }

        internal Guid Id { get; }
        internal ChannelReader<T> Reader { get; }
        internal ChannelWriter<T> Writer { get; }
    }
}