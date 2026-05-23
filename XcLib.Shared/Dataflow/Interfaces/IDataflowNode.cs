using System.Threading.Tasks.Dataflow;

namespace XcLib.Shared.Dataflow.Interfaces;

public interface IDataflowNode<T>
{
    public IPropagatorBlock<T, T> Propagator { get; }
    public bool Publish(T value);
    public Task<bool> PublishAsync(T value);
}