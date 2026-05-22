using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Caching.Distributed;
using Webhook.Objects;

namespace Webhook.Meshes;

/************************ TOPOLOGY ******************************
 *
 *                     _broadcastBlock
 *                |                        |
 *       _reqTransformerBlock         _loggerBlock
 *                 |
 *          _processorBlock
 *                 |
 *       _postProcessorBroadcastBlock
 *        |           |           |
 *   _loggerBlock
 ***************************************************************/
internal sealed class WebhookMesh : IAsyncDisposable
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<WebhookMesh> _logger;
    private readonly BroadcastBlock<DockerWebhookRequest> _broadcastBlock;
    private readonly DistributedCacheEntryOptions _cacheEntryOptions;
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
        _cacheEntryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        };
        ExecutionDataflowBlockOptions executionDataflowBlockOptions = new ExecutionDataflowBlockOptions()
        {
            MaxDegreeOfParallelism = 3,
            BoundedCapacity = 100
        };
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
        BroadcastBlock<DockerWebhookRequest> postProcessorBroadcastBlock =
            new BroadcastBlock<DockerWebhookRequest>(null, dataflowBlockOptions);

        TransformBlock<DockerWebhookRequest, DockerWebhookRequest> reqTransformerBlock =
            new TransformBlock<DockerWebhookRequest, DockerWebhookRequest>(WebhookToTargretServiceTransformer,
                executionDataflowBlockOptions);
        TransformBlock<DockerWebhookRequest, DockerWebhookRequest> processorBlock =
            new TransformBlock<DockerWebhookRequest, DockerWebhookRequest>(WebhookTargetServiceProcessCreator,
                executionDataflowBlockOptions);
        ActionBlock<DockerWebhookRequest> loggerBlock =
            new ActionBlock<DockerWebhookRequest>(WebhookTargetServiceExecutionLogger, executionDataflowBlockOptions);

        _broadcastBlock.LinkTo(reqTransformerBlock, dataflowLinkOptions);
        _broadcastBlock.LinkTo(loggerBlock, dataflowLinkOptions);

        reqTransformerBlock.LinkTo(processorBlock, dataflowLinkOptions, ProcessorFilterBlock);
        reqTransformerBlock.LinkTo(DataflowBlock.NullTarget<DockerWebhookRequest>());

        processorBlock.LinkTo(postProcessorBroadcastBlock, dataflowLinkOptions);
        postProcessorBroadcastBlock.LinkTo(loggerBlock, dataflowLinkOptions);
    }

    public void Publish(DockerWebhookRequest request) => _broadcastBlock.Post(request);

    private bool ProcessorFilterBlock(DockerWebhookRequest obj)
    {
        try
        {
            if (string.IsNullOrEmpty(obj.Img) || string.IsNullOrEmpty(obj.Service))
                return false;

            DateTimeOffset deployTime = DateTimeOffset.Now;
            string cacheKey = $"wh.redeploy.{obj.Service}";
            byte[]? maybeDeploying = _cache.Get(cacheKey);

            _cache.SetString(cacheKey,
                JsonSerializer.Serialize(
                    new WebhookLogEntry
                    {
                        Time = deployTime.ToString("G"),
                        Image = obj.Img
                    }, Program.AppJsonContext.Default.WebhookLogEntry),
                _cacheEntryOptions);
            return maybeDeploying is not
            {
                Length: > 0
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return false;
        }
    }

    private void WebhookTargetServiceExecutionLogger(DockerWebhookRequest request)
    {
        _logger.LogInformation("received webhook at {time}, {request}",DateTimeOffset.Now.ToString("G"),request);
    }

    private async Task<DockerWebhookRequest> WebhookTargetServiceProcessCreator(DockerWebhookRequest obj)
    {
        _logger.LogInformation(
            "processed webhook for service {service}, image {image}",
            obj.Service,
            obj.Img);

        string script = ScriptTemplate
            .Replace("[{service}]", obj.Service)
            .Replace("[{image}]", obj.Img);

        using Process proc = new Process();
        proc.StartInfo = new ProcessStartInfo
        {
            FileName = "/bin/sh",
            ArgumentList = { "-c", script },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        string stdout = "";
        string stderr = "";
        int exitCode;

        try
        {
            proc.Start();

            stdout = await proc.StandardOutput.ReadToEndAsync();
            stderr = await proc.StandardError.ReadToEndAsync();

            await proc.WaitForExitAsync();
            exitCode = proc.ExitCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "process execution failed for {service}", obj.Service);

            return obj with
            {
                ExecutionState = 11,
                StdErr = stderr,
                StdOut = stdout
            };
        }

        return obj with
        {
            ExecutionState = (sbyte)exitCode,
            StdOut = stdout,
            StdErr = stderr
        };
    }

    private static DockerWebhookRequest WebhookToTargretServiceTransformer(DockerWebhookRequest request) =>
        request.Index switch
        {
            1 => request with
            {
                Service = "xc-app_realtime",
                Img = "rennn0/realtime"
            },
            2 => request with
            {
                Service = "xc-app_api",
                Img = "rennn0/xaticraft"
            },
            3 => request with
            {
                Service = "xc-app_webhook",
                Img = "rennn0/webhook"
            },
            _ => request
        };

    public async ValueTask DisposeAsync()
    {
        _broadcastBlock.Complete();
        await _broadcastBlock.Completion;
    }
}