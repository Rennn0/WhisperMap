using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Realtime;

public static partial class Program
{
    private static void ApiPostWebhook(this RouteGroupBuilder builder)
    {
        const string scriptTemplate = """
                                      #!/bin/sh
                                      set -eu

                                      SERVICE="__SERVICE__"
                                      IMAGE="__IMAGE__"
                                      TAG="latest"

                                      echo "Updating ${SERVICE} to ${IMAGE}:${TAG}"
                                      docker service update --image "${IMAGE}:${TAG}" --force --with-registry-auth "${SERVICE}"
                                      echo "Done"
                                      """;
        builder.MapPost("{idx:int}",
            async ([FromRoute] int idx, [FromBody] JsonDocument webHookJson,
                [FromServices] ILoggerFactory loggerFactory) =>
            {
                ILogger logger = loggerFactory.CreateLogger("webhook");
                logger.LogInformation(webHookJson.RootElement.ToString());

                string? serviceName = idx switch
                {
                    1 => "xc_realtime",
                    2 => "xc_api",
                    _ => null
                };

                string? imageName = idx switch
                {
                    1 => "rennn0/realtime",
                    2 => "rennn0/xaticraft",
                    _ => null
                };

                string script = scriptTemplate
                    .Replace("__SERVICE__", serviceName, StringComparison.Ordinal)
                    .Replace("__IMAGE__", imageName, StringComparison.Ordinal);

                logger.LogInformation(script);
                string scriptPath = Path.Combine(Path.GetTempPath(), $"swarm_update_{Guid.NewGuid():N}.sh");
                try
                {
                    await File.WriteAllTextAsync(scriptPath, script);
                    Process proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "/bin/chmod",
                            ArgumentList = { "+x", scriptPath },
                            RedirectStandardError = true,
                            UseShellExecute = false
                        }
                    };

                    proc.Start();
                    await proc.WaitForExitAsync();
                    if (proc.ExitCode != 0)
                    {
                        string chmodErr = await proc.StandardError.ReadToEndAsync();
                        logger.LogError(chmodErr);
                        return Results.Problem($"chmod failed: {chmodErr}");
                    }

                    proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "/bin/sh",
                            ArgumentList = { "-c", $"{scriptPath} > /tmp/webhook-{idx}.log 2>&1 &" },
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false
                        }
                    };

                    proc.Start();
                    logger.LogWarning("Started webhook update for idx {Idx}, service {Service}", idx, serviceName);

                    return Results.Ok(new
                    {
                        success = true,
                        idx,
                        service = serviceName,
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