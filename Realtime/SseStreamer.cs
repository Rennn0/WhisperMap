using System.Threading.Channels;

namespace Realtime;

internal sealed class SseStreamer
{
    private enum SseLogs
    {
        StartStream,
        EndStream,
        ExcStreamCancelled,
        ExcStreamDestroyed
    }

    private readonly HttpContext _context;
    private readonly CancellationTokenSource _cancellationSource;
    private readonly ILogger<SseStreamer> _logger;

    internal SseStreamer(HttpContext context, CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, context.RequestAborted);
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug).AddSimpleConsole(opt =>
            {
                opt.IncludeScopes = true;
                opt.SingleLine = true;
                opt.TimestampFormat = "[HH:mm:ss] ";
            });
        });
        _logger = loggerFactory.CreateLogger<SseStreamer>();
    }

    internal CancellationToken CancellationToken => _cancellationSource.Token;

    internal void InitResponse()
    {
        _context.Response.Headers.ContentType = "text/event-stream";
        _context.Response.Headers.CacheControl = "no-cache";
        _context.Response.Headers["X-Accel-Buffering"] = "no";
    }


    internal async Task StreamAsync<T>(IAsyncEnumerable<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter)
    {
        _logger.LogDebug(new EventId((int)SseLogs.StartStream, nameof(SseLogs.StartStream)),
            "Starting SSE streaming for {RequestPath}, event {EventName}", _context.Request.Path, eventName);

        await using IAsyncEnumerator<T> enumerator = source.GetAsyncEnumerator(CancellationToken);
        Task<bool> moveNextTask = enumerator.MoveNextAsync().AsTask();

        while (!CancellationToken.IsCancellationRequested)
        {
            Task pingTask = Task.Delay(heartbeatInterval, CancellationToken);
            Task completed = await Task.WhenAny(moveNextTask, pingTask);

            if (completed == moveNextTask)
            {
                if (!await moveNextTask) break;

                await WriteEventAsync(eventName, enumerator.Current, formatter);
                moveNextTask = enumerator.MoveNextAsync().AsTask();
            }
            else
            {
                await WriteCommentAsync("ping");
            }
        }
    }

    internal async Task StreamAsync<T>(ChannelReader<T> source, string eventName, TimeSpan heartbeatInterval,
        SseEventFormatter<T> formatter)
    {
        _logger.LogDebug(new EventId((int)SseLogs.StartStream, nameof(SseLogs.StartStream)),
            "Starting SSE streaming for {RequestPath}, event {EventName}", _context.Request.Path, eventName);

        try
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                Task<bool> waitForDataTask = source.WaitToReadAsync(CancellationToken).AsTask();
                Task pingTask = Task.Delay(heartbeatInterval, CancellationToken);
                Task completed = await Task.WhenAny(waitForDataTask, pingTask);

                if (completed == waitForDataTask)
                {
                    if (!await waitForDataTask)
                    {
                        _logger.LogDebug(new EventId((int)SseLogs.EndStream, nameof(SseLogs.EndStream)),
                            "End channel streaming for {RequestPath}, event {EventName}", _context.Request.Path,
                            eventName);
                        break;
                    }

                    while (source.TryRead(out T? update)) await WriteEventAsync(eventName, update, formatter);
                }
                else
                {
                    await WriteCommentAsync("ping");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug(new EventId((int)SseLogs.EndStream, nameof(SseLogs.EndStream)),
                "Streaming for {RequestPath}, event {EventName} cancelled", _context.Request.Path, eventName);
        }
        catch (Exception e)
        {
            _logger.LogError(new EventId((int)SseLogs.ExcStreamDestroyed, nameof(SseLogs.ExcStreamDestroyed)),
                "Streaming for {RequestPath}, event {EventName} destroyed, exception {Exception}",
                _context.Request.Path, eventName, e.Message);
        }
    }

    private async Task WriteEventAsync<T>(string eventName, T data, SseEventFormatter<T> formatter)
    {
        CancellationToken.ThrowIfCancellationRequested();

        string payload = formatter.Format(eventName, data);
        await _context.Response.WriteAsync(payload, CancellationToken);
        await _context.Response.Body.FlushAsync(CancellationToken);
    }

    private async Task WriteCommentAsync(string comment)
    {
        CancellationToken.ThrowIfCancellationRequested();

        string payload = $":{comment}\n\n";
        await _context.Response.WriteAsync(payload, CancellationToken);
        await _context.Response.Body.FlushAsync(CancellationToken);
    }
}