namespace XcLib.Shared.Dataflow.Interfaces;

public interface IDataflowNodeFactory<TMessage> where TMessage : class
{
    public IDataflowNode<TMessage> Create<TNode>() where TNode : IDataflowNode<TMessage>;
}