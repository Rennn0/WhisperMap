using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Realtime;

public static partial class Program
{
    private static void ApiPostWebhook(this RouteGroupBuilder builder)
    {
        builder.MapPost("{idx:int}",
            async ([FromRoute] int idx, [FromBody] JsonDocument webHookJson,
                [FromServices] ILoggerFactory loggerFactory, [FromServices] IDistributedCache cache) =>
            {
                ILogger logger = loggerFactory.CreateLogger("webhook");
                logger.LogInformation("webhook {a}", webHookJson);

                (string service, string image) = idx switch
                {
                    1 => ("xc_realtime", "rennn0/realtime"),
                    2 => ("xc_api", "rennn0/xaticraft"),
                    _ => throw new ArgumentOutOfRangeException(nameof(idx))
                };

                string cacheKey = $"wh_redeploy_{service}";
                byte[]? maybeDeploying = await cache.GetAsync(cacheKey);
                if (maybeDeploying is { Length: > 0 })
                    return Results.Ok();

                DateTimeOffset deployTime = DateTimeOffset.UtcNow;
                await cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(deployTime.ToString()),
                    new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
                });

                string script =
                    $"""
                     #!/bin/sh
                     set -eu
                     docker service update --image {image}:latest --force --with-registry-auth {service}
                     """;
                logger.LogInformation("exec script {a}", script);
                
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
                    logger.LogInformation("started webhook update for idx {Idx}, service {Service}", idx, service);

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
            });
    }
}