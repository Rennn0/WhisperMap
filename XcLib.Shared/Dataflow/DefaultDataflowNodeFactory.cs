using Microsoft.Extensions.DependencyInjection;
using XcLib.Shared.Dataflow.Interfaces;

namespace XcLib.Shared.Dataflow;

public class DefaultDataflowNodeFactory<TMessage> : IDataflowNodeFactory<TMessage> where TMessage : class
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultDataflowNodeFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public IDataflowNode<TMessage> Create<TNode>() where TNode : IDataflowNode<TMessage> =>
        ActivatorUtilities.CreateInstance<TNode>(_serviceProvider);
}