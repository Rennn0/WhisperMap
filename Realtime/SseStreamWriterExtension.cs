namespace Realtime;

internal static class SseStreamWriterExtension
{
    public static SseStreamer CreateSseStreamer(this HttpContext context, CancellationToken cancellationToken)
    {
        SseStreamer streamer = new SseStreamer(context, cancellationToken);
        streamer.InitResponse();
        return streamer;
    }
}