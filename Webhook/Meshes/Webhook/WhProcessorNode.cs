using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Caching.Distributed;
using Webhook.Objects;
using XcLib.Shared.Dataflow;
using XcLib.Shared.Dataflow.Interfaces;

namespace Webhook.Meshes.Webhook;

internal class WhProcessorNode : IDataflowNode<DockerWebhookRequest>
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<WhProcessorNode> _logger;
    private readonly BufferBlock<DockerWebhookRequest> _bufferBlock;
    private readonly DistributedCacheEntryOptions _cacheEntryOptions;

    private const string ScriptTemplate =
        """
        #!/bin/sh
        set -eu
        docker service update [{service}] --image [{image}]:latest --force
        """;

    public WhProcessorNode(IDistributedCache distributedCache, ILogger<WhProcessorNode> logger,
        IDataflowNodeFactory<DockerWebhookRequest> nodeFactory)
    {
        _distributedCache = distributedCache;
        _logger = logger;
        _cacheEntryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        };
        _bufferBlock = new BufferBlock<DockerWebhookRequest>();

        MeshOptions meshOptions = new MeshOptions();
        TransformBlock<DockerWebhookRequest, DockerWebhookRequest> transformer =
            new TransformBlock<DockerWebhookRequest, DockerWebhookRequest>(WebhookToTargretServiceTransformer,
                meshOptions.ExecutionDataflowBlockOptions);
        TransformBlock<DockerWebhookRequest, DockerWebhookRequest> processCreator =
            new TransformBlock<DockerWebhookRequest, DockerWebhookRequest>(WebhookTargetServiceProcessCreator,
                meshOptions.ExecutionDataflowBlockOptions);
        _bufferBlock.LinkTo(transformer, meshOptions.DataflowLinkOptions);
        transformer.LinkTo(processCreator, meshOptions.DataflowLinkOptions, ProcessorFilterBlock);
        transformer.LinkTo(DataflowBlock.NullTarget<DockerWebhookRequest>(),meshOptions.DataflowLinkOptions);
        processCreator.LinkTo(nodeFactory.Create<WhLoggerNode>().Propagator, meshOptions.DataflowLinkOptions);
    }

    public IPropagatorBlock<DockerWebhookRequest, DockerWebhookRequest> Propagator =>
        _bufferBlock;

    public bool Publish(DockerWebhookRequest value) => Propagator.Post(value);

    public Task<bool> PublishAsync(DockerWebhookRequest value) => Propagator.SendAsync(value);

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
                State = ExecutionState.Fail,
                StdErr = stderr,
                StdOut = stdout
            };
        }

        return obj with
        {
            State = exitCode == 0 ? ExecutionState.Success : ExecutionState.Fail,
            StdOut = stdout,
            StdErr = stderr
        };
    }

    private bool ProcessorFilterBlock(DockerWebhookRequest obj)
    {
        try
        {
            if (string.IsNullOrEmpty(obj.Img) || string.IsNullOrEmpty(obj.Service))
                return false;

            DateTimeOffset deployTime = DateTimeOffset.Now;
            string cacheKey = $"wh.redeploy.{obj.Service}";
            byte[]? maybeDeploying = _distributedCache.Get(cacheKey);

            _distributedCache.SetString(cacheKey,
                JsonSerializer.Serialize(
                    new WebhookLogEntry
                    {
                        Time = deployTime.ToLocalTime().ToString("g"),
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
            _logger.LogError(e, "{message}", e.Message);
            return false;
        }
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
}