using System.Threading.Channels;

namespace XcLib.Sse.Core.Stream;

public abstract partial class SseStream<T>
{
    public sealed class StreamSubscriber
    {
        public StreamSubscriber(string id, Channel<T> channel)
        {
            Id = id;
            Reader = channel.Reader;
            Writer = channel.Writer;
        }

        public string Id { get; }
        public ChannelReader<T> Reader { get; }
        public ChannelWriter<T> Writer { get; }
    }
}