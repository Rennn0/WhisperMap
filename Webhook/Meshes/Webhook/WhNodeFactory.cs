using Webhook.Meshes.Abstraction;
using Webhook.Objects;

namespace Webhook.Meshes.Webhook;

internal class WhNodeFactory : IDataFlowNodeFactory<DockerWebhookRequest>
{
    private readonly IServiceProvider _serviceProvider;

    public WhNodeFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public IDataFlowNode<DockerWebhookRequest> Create<TNode>() where TNode : IDataFlowNode<DockerWebhookRequest> =>
        ActivatorUtilities.CreateInstance<TNode>(_serviceProvider);
}