using System.Threading.Tasks.Dataflow;
using Webhook.Objects;
using XcLib.Shared.Dataflow;
using XcLib.Shared.Dataflow.Interfaces;

namespace Webhook.Meshes.Webhook;

internal sealed class WebhookMesh : IAsyncDisposable
{
    private readonly BroadcastBlock<DockerWebhookRequest> _broadcastBlock;

    public WebhookMesh(IDataflowNodeFactory<DockerWebhookRequest> nodeFactory)
    {
        MeshOptions meshOptions = new MeshOptions();
        _broadcastBlock = new BroadcastBlock<DockerWebhookRequest>(null, meshOptions.DataflowBlockOptions);
        _broadcastBlock.LinkTo(nodeFactory.Create<WhLoggerNode>().Propagator, meshOptions.DataflowLinkOptions);
        _broadcastBlock.LinkTo(nodeFactory.Create<WhProcessorNode>().Propagator, meshOptions.DataflowLinkOptions);
    }

    public bool Publish(DockerWebhookRequest request) => _broadcastBlock.Post(request);

    public async ValueTask DisposeAsync()
    {
        _broadcastBlock.Complete();
        await _broadcastBlock.Completion;
    }
}