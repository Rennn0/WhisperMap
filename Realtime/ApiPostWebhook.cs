using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Realtime;

public static partial class Program
{
    private static readonly SemaphoreSlim Sema = new SemaphoreSlim(1, 1);
    private static void ApiPostWebhook(this RouteGroupBuilder builder)
    {
        builder.MapPost("{idx:int}",
            async ([FromRoute] int idx, [FromBody] JsonDocument webHookJson,
                [FromServices] ILoggerFactory loggerFactory, [FromServices] IDistributedCache cache) =>
            {
                ILogger logger = loggerFactory.CreateLogger("webhook");
                string? tag = webHookJson.RootElement.GetProperty("push_data").GetProperty("tag").GetString();
                if (tag != "latest") return Results.BadRequest();
                logger.LogInformation("webhook {a}", webHookJson.RootElement);

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
                    {
                        time = deployTime.ToString("G"),
                        tag
                    }),
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