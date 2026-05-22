using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Caching.Distributed;
using Webhook.Objects;

namespace Webhook.Meshes;

internal class WebhookMesh : IAsyncDisposable
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<WebhookMesh> _logger;
    private readonly BufferBlock<DockerWebhookRequest> _sourceBlock;
    private readonly BroadcastBlock<DockerWebhookRequest> _broadcastBlock;
    private readonly TransformBlock<DockerWebhookRequest,(string service, string image)> _reqTransformerBlock;
    private readonly ActionBlock<(string service, string image)> _processorBlock;
    private readonly ActionBlock<DockerWebhookRequest> _loggerBlock;
    private const string ScriptTemplate =
        """
        #!/bin/sh
        set -eu
        docker service update [{service}] --image [{image}]:latest --force
        """;
    public WebhookMesh(IDistributedCache cache,ILogger<WebhookMesh> logger)
    {
        _cache = cache;
        _logger = logger;
        ExecutionDataflowBlockOptions executionDataflowBlockOptions = new ExecutionDataflowBlockOptions()
        {
            MaxDegreeOfParallelism = 3,
            EnsureOrdered =  true,
            BoundedCapacity = 9
        };
        DataflowLinkOptions dataflowLinkOptions = new DataflowLinkOptions()
        {
            PropagateCompletion = true,
            Append = true,
            MaxMessages = 9
        };
        DataflowBlockOptions dataflowBlockOptions = new DataflowBlockOptions()
        {
            EnsureOrdered = true,
            BoundedCapacity = 9,
        };
        
        _sourceBlock = new BufferBlock<DockerWebhookRequest>( dataflowBlockOptions );
        _broadcastBlock = new BroadcastBlock<DockerWebhookRequest>(r=>r,dataflowBlockOptions);
        _reqTransformerBlock = new TransformBlock<DockerWebhookRequest, (string service, string image)>(WebhookToTargretServiceTransformer,executionDataflowBlockOptions);
        _processorBlock = new ActionBlock<(string service, string image)>(WebhookTargetServiceProcessCreator,executionDataflowBlockOptions);
        _loggerBlock = new ActionBlock<DockerWebhookRequest>(WebhookTargetServiceExecutionLogger,executionDataflowBlockOptions);

        _sourceBlock.LinkTo(_broadcastBlock,dataflowLinkOptions);
        _broadcastBlock.LinkTo(_reqTransformerBlock,dataflowLinkOptions);
        _broadcastBlock.LinkTo(_loggerBlock,dataflowLinkOptions);
        _reqTransformerBlock.LinkTo(_processorBlock,dataflowLinkOptions,ProcessorBlockFilter);
    }

    public async Task PublishAsync(DockerWebhookRequest request,CancellationToken token)
        => await _sourceBlock.SendAsync(request, token);

    private bool ProcessorBlockFilter((string service, string image) obj)
    {
        DateTimeOffset deployTime = DateTimeOffset.Now;
        string cacheKey = $"wh.redeploy.{obj.service}";
        byte[]? maybeDeploying = _cache.Get(cacheKey);

        _cache.SetString(cacheKey, 
            JsonSerializer.Serialize(
                new WebhookLogEntry
                {
                    Time = deployTime.ToString("G"),
                    Image = obj.image
                }, Program.AppJsonContext.Default.WebhookLogEntry),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
            });
        return maybeDeploying is not {Length:>0};
    }
    private Task WebhookTargetServiceExecutionLogger(DockerWebhookRequest request)
    {
        _logger.LogInformation("received webhook at {time}, {request}",DateTimeOffset.Now.ToString("G"),request);
        return Task.CompletedTask;
    }

    private Task WebhookTargetServiceProcessCreator((string service, string image) obj)
    {
        _logger.LogInformation("processed webhook for service {service}, image {image}",obj.service, obj.image);
        string script = ScriptTemplate
            .Replace("[{service}]", obj.service)
            .Replace("[{image}]", obj.image);
        
        Process proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                ArgumentList = { "-c", $"{script} > /tmp/webhook-{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.log 2>&1 &" },
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false
            }
        };
        proc.Start();

        return Task.CompletedTask;
    }

    private Task<(string service, string image)> WebhookToTargretServiceTransformer(DockerWebhookRequest request) =>
        Task.FromResult( request.Index switch
        {
            1 => ("xc-app_realtime", "rennn0/realtime"),
            2 => ("xc-app_api", "rennn0/xaticraft"),
            3 => ("xc-app_webhook", "rennn0/webhook"),
            _ => default
        });

    public async ValueTask DisposeAsync()
    {
        _sourceBlock.Complete();
        _broadcastBlock.Complete();
        await Task.WhenAll(_reqTransformerBlock.Completion, _processorBlock.Completion, _loggerBlock.Completion);
        GC.SuppressFinalize(this);
    }
}