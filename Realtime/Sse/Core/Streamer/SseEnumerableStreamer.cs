using Realtime.Sse.Formatters;

namespace Realtime.Sse.Core.Streamer;

internal class SseEnumerableStreamer : SseStreamer
{
    public SseEnumerableStreamer(HttpContext context) :
        base(context, CancellationToken.None) => Logger = LogFactory.CreateLogger<SseEnumerableStreamer>();

    public SseEnumerableStreamer(HttpContext context, CancellationToken cancellationToken) :
        base(context, cancellationToken) => Logger = LogFactory.CreateLogger<SseEnumerableStreamer>();

    internal async Task StreamAsync<T>(IAsyncEnumerable<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter)
    {
        Logger.LogDebug(new EventId((int)SseLogs.StartStream, nameof(SseLogs.StartStream)),
            "Starting SSE streaming for {RequestPath}, event {EventName}", Context.Request.Path, eventName);

        IAsyncEnumerator<T>? enumerator = null;
        Task<bool>? moveNextTask = null;

        try
        {
            enumerator = source.GetAsyncEnumerator(CancellationToken);
            moveNextTask = enumerator.MoveNextAsync().AsTask();

            while (!CancellationToken.IsCancellationRequested)
            {
                Task pingTask = Task.Delay(heartbeatInterval, CancellationToken);
                Task completed = await Task.WhenAny(moveNextTask, pingTask);
                CancellationToken.ThrowIfCancellationRequested();

                if (completed == moveNextTask)
                {
                    if (!await moveNextTask) break;

                    await SafeWriteAsync(() => WriteEventAsync(eventName, enumerator.Current, formatter));
                    moveNextTask = enumerator.MoveNextAsync().AsTask();
                }
                else
                {
                    await SafeWriteAsync(() => WriteCommentAsync("ping"));
                }
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug(new EventId((int)SseLogs.ExcStreamCancelled, nameof(SseLogs.ExcStreamCancelled)),
                "Streaming for {RequestPath}, event {EventName} cancelled", Context.Request.Path, eventName);
        }
        catch (Exception e)
        {
            Logger.LogError(new EventId((int)SseLogs.ExcStreamDestroyed, nameof(SseLogs.ExcStreamDestroyed)), e,
                "Streaming for {RequestPath}, event {EventName} destroyed, exception {Exception}",
                Context.Request.Path, eventName, e.Message);
        }
        finally
        {
            if (moveNextTask is { IsCompleted: false })
                try
                {
                    await moveNextTask;
                }
                catch
                {
                    // ignored
                }

            if (enumerator is not null)
                try
                {
                    await enumerator.DisposeAsync();
                }
                catch
                {
                    // ignored
                }
        }
    }
}