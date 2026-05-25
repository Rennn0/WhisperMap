using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using XcLib.Shared.Reactive.Interfaces;

namespace XcLib.Shared.Reactive;

public class DefaultReactiveBus<T> : IReactiveBus<T>, IDisposable where T : class
{
    protected SubjectBase<T> Subject { get; init; }
    private readonly ILogger<T> _logger;
    public IObservable<T> OnMessage => Subject;

    // ReSharper disable once MemberCanBeProtected.Global
    public DefaultReactiveBus(ILogger<T> logger)
    {
        _logger = logger;
        Subject = new Subject<T>();
    }

    public void Publish(T message) => Subject.OnNext(message);

    public Task PublishAsync(T message)
    {
        Subject.OnNext(message);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Subject.Dispose();
        GC.SuppressFinalize(this);
    }
}