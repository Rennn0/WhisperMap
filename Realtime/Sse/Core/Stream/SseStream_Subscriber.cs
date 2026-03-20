using System.Threading.Channels;

namespace Realtime.Sse.Core.Stream;

internal abstract partial class SseStream<T>
{
    internal sealed class StreamSubscriber
    {
        public StreamSubscriber(string id, Channel<T> channel)
        {
            Id = id;
            Reader = channel.Reader;
            Writer = channel.Writer;
        }

        internal string Id { get; }
        internal ChannelReader<T> Reader { get; }
        internal ChannelWriter<T> Writer { get; }
    }
}