using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Caching.Distributed;
using Webhook.Meshes.Abstraction;
using Webhook.Objects;

namespace Webhook.Meshes.Webhook;

internal class WhLoggerNode : IDataFlowNode<DockerWebhookRequest>
{
    private readonly ILogger<WhLoggerNode> _logger;
    private readonly BufferBlock<DockerWebhookRequest> _bufferBlock;

    public WhLoggerNode(IDistributedCache distributedCache, ILogger<WhLoggerNode> logger)
    {
        _logger = logger;
        _bufferBlock = new BufferBlock<DockerWebhookRequest>();
        _bufferBlock.LinkTo(new ActionBlock<DockerWebhookRequest>(Action));
    }

    private void Action(DockerWebhookRequest obj)
    {
        _logger.LogInformation("received webhook at {time}, {request}", DateTimeOffset.Now.ToString("G"), obj);
    }

    public IPropagatorBlock<DockerWebhookRequest, DockerWebhookRequest> Propagator => _bufferBlock;

    public bool Publish(DockerWebhookRequest value) => Propagator.Post(value);

    public Task<bool> PublishAsync(DockerWebhookRequest value) => Propagator.SendAsync(value);
}