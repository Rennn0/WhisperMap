namespace XcLib.Shared.Dataflow.Interfaces;

public interface IDataflowNodeFactory<TMessage>
{
    public IDataflowNode<TMessage> Create<TNode>() where TNode : IDataflowNode<TMessage>;
}