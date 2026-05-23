using System.Threading.Tasks.Dataflow;

namespace Webhook.Meshes.Abstraction;

internal interface IDataFlowNode<T>
{
    public IPropagatorBlock<T, T> Propagator { get; }
    public bool Publish(T value);
    public Task<bool> PublishAsync(T value);
}