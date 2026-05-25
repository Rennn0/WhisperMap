using System.Threading.Tasks.Dataflow;
using Webhook.Objects;
using XcLib.Shared.Dataflow;
using XcLib.Shared.Dataflow.Interfaces;
using XcLib.Shared.Reactive.Interfaces;

namespace Webhook.Meshes.Webhook;

internal sealed class WebhookMesh : IAsyncDisposable
{
    private readonly ILogger<WebhookMesh> _logger;
    private readonly BroadcastBlock<DockerWebhookRequest> _broadcastBlock;
    private readonly IDisposable _subscription;

    public WebhookMesh(
        IDataflowNodeFactory<DockerWebhookRequest> nodeFactory,
        IReactiveBus<DockerWebhookRequest> reactiveBus,
        ILogger<WebhookMesh> logger
    )
    {
        _logger = logger;
        MeshOptions meshOptions = new MeshOptions();
        _broadcastBlock = new BroadcastBlock<DockerWebhookRequest>(null, meshOptions.DataflowBlockOptions);
        _broadcastBlock.LinkTo(nodeFactory.Create<WhLoggerNode>().Propagator, meshOptions.DataflowLinkOptions);
        _broadcastBlock.LinkTo(nodeFactory.Create<WhProcessorNode>().Propagator, meshOptions.DataflowLinkOptions);

        _subscription = reactiveBus.OnMessage.Subscribe(OnNext, OnError, OnCompleted);
    }

    private static void OnCompleted()
    {
    }

    private void OnError(Exception obj) => _logger.LogError(obj, "{msg}", obj.Message);

    private void OnNext(DockerWebhookRequest obj) => _broadcastBlock.Post(obj);

    public async ValueTask DisposeAsync()
    {
        _broadcastBlock.Complete();
        _subscription.Dispose();
        await _broadcastBlock.Completion;
    }
}