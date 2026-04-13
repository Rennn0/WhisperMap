using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Webhook;

public static partial class Program
{
    private static readonly SemaphoreSlim Sema = new SemaphoreSlim(1, 1);

    internal readonly record struct WebhookLogEntry
    {
        public string? Tag { get; init; }
        public string? Time { get; init; }
    }

    internal readonly record struct DockerWebhookRequest
    {
        [JsonPropertyName("push_data")] public PushData? Push { get; init; }
    }

    internal readonly record struct PushData
    {
        [JsonPropertyName("tag")] public string? Tag { get; init; }
    }


    private static void ApiPostWebhook(this RouteGroupBuilder builder)
    {
        builder.MapPost("{idx:int}",
            async ([FromRoute] int idx, [FromBody] DockerWebhookRequest request,
                [FromServices] ILoggerFactory loggerFactory, [FromServices] IDistributedCache cache) =>
            {
                ILogger logger = loggerFactory.CreateLogger("webhook");
                logger.LogInformation("webhook {a}", request);
                if (request is not { Push.Tag: "latest" }) return Results.BadRequest();
                
                (string service, string image) = idx switch
                {
                    1 => ("xc_realtime", "rennn0/realtime"),
                    2 => ("xc_api", "rennn0/xaticraft"),
                    _ => throw new ArgumentOutOfRangeException(nameof(idx))
                };

                await Sema.WaitAsync();

                DateTimeOffset deployTime = DateTimeOffset.Now;
                string cacheKey = $"wh.redeploy.{service}";
                byte[]? maybeDeploying = await cache.GetAsync(cacheKey);

                if (maybeDeploying is { Length: > 0 })
                {
                    Sema.Release();
                    return Results.Ok();
                }

                await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(new
                        WebhookLogEntry
                        {
                            Time = deployTime.ToString("G"),
                            Tag = request.Push.Value.Tag
                        }, AppJsonContext.Default.WebhookLogEntry),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
                    });

                string script =
                    $"""
                     #!/bin/sh
                     set -eu
                     docker service update {service} --image {image}:latest --force
                     """;
                logger.LogWarning("exec script {a}", script);

                try
                {
                    Process proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "/bin/sh",
                            ArgumentList = { "-c", $"{script} > /tmp/webhook-{idx}.log 2>&1 &" },
                            RedirectStandardOutput = false,
                            RedirectStandardError = false,
                            UseShellExecute = false
                        }
                    };
                    proc.Start();
                    logger.LogWarning("started webhook update for idx {Idx}, service {Service}", idx, service);

                    return Results.Ok(new
                    {
                        success = true,
                        idx,
                        service,
                        started = true
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
                finally
                {
                    Sema.Release();
                }
            });
    }
}