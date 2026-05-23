using Microsoft.Extensions.DependencyInjection;
using XcLib.Shared.Dataflow.Interfaces;

namespace XcLib.Shared.Dataflow;

public class DataflowNodeFactory<TMessage> : IDataflowNodeFactory<TMessage>
{
    private readonly IServiceProvider _serviceProvider;

    public DataflowNodeFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public IDataflowNode<TMessage> Create<TNode>() where TNode : IDataflowNode<TMessage> =>
        ActivatorUtilities.CreateInstance<TNode>(_serviceProvider);
}