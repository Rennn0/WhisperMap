namespace XcLib.Shared.Reactive.Interfaces;

public interface IReactiveBus<TMessage> where TMessage : class
{
    IObservable<TMessage> OnMessage { get; }
    void Publish(TMessage message);
    Task PublishAsync(TMessage message);
}