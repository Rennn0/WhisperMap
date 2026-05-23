namespace Webhook.Meshes.Abstraction;

internal interface IDataFlowNodeFactory<TMessage>
{
    public IDataFlowNode<TMessage> Create<TNode>() where TNode : IDataFlowNode<TMessage>;
}