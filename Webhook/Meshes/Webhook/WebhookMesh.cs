using System.Threading.Tasks.Dataflow;
using Webhook.Meshes.Abstraction;
using Webhook.Objects;

namespace Webhook.Meshes.Webhook;

internal sealed class WebhookMesh : IAsyncDisposable
{
    private readonly BroadcastBlock<DockerWebhookRequest> _broadcastBlock;

    public WebhookMesh(IDataFlowNodeFactory<DockerWebhookRequest> nodeFactory)
    {
        DataflowLinkOptions dataflowLinkOptions = new DataflowLinkOptions()
        {
            Append = true,
            PropagateCompletion = true
        };
        DataflowBlockOptions dataflowBlockOptions = new DataflowBlockOptions()
        {
            EnsureOrdered = true,
            BoundedCapacity = 100
        };

        _broadcastBlock = new BroadcastBlock<DockerWebhookRequest>(null, dataflowBlockOptions);
        _broadcastBlock.LinkTo(nodeFactory.Create<WhLoggerNode>().Propagator, dataflowLinkOptions);
        _broadcastBlock.LinkTo(nodeFactory.Create<WhProcessorNode>().Propagator, dataflowLinkOptions);
    }

    public bool Publish(DockerWebhookRequest request) => _broadcastBlock.Post(request);

    public async ValueTask DisposeAsync()
    {
        _broadcastBlock.Complete();
        await _broadcastBlock.Completion;
    }
}