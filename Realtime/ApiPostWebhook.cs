using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Realtime;

public static partial class Program
{
    private static void ApiPostWebhook(this RouteGroupBuilder builder)
    {
        const string scriptTemplate = """
                                      #!/usr/bin/env bash
                                      set -euo pipefail

                                      SERVICE="__SERVICE__"
                                      IMAGE="__IMAGE__"
                                      TAG="latest"

                                      echo "Updating ${SERVICE} to ${IMAGE}:${TAG}"
                                      docker service update --image "${IMAGE}:${TAG}" --force --with-registry-auth "${SERVICE}"
                                      echo "Done"
                                      """;
        builder.MapPost("{idx:int}",
            async ([FromRoute] int idx, [FromBody] JsonDocument webHookJson,
                [FromServices] ILogger<RouteGroupBuilder> logger) =>
            {
                logger.LogInformation(webHookJson.RootElement.ToString());

                string? repo = webHookJson.RootElement.GetProperty("repository").GetProperty("repo_name").GetString();
                string? tag = webHookJson.RootElement.GetProperty("push_data").GetProperty("tag").GetString();

                if (string.IsNullOrEmpty(repo) || string.IsNullOrEmpty(tag)) throw new Exception("invalid repo/tag");

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

                string expectedRepo = idx switch
                {
                    1 => "rennn0/realtime",
                    2 => "rennn0/xaticraft",
                    _ => string.Empty
                };

                if (!string.Equals(repo, expectedRepo, StringComparison.OrdinalIgnoreCase))
                    return Results.Ok(new
                    {
                        success = true,
                        skipped = true,
                        reason = $"Repo mismatch. Expected '{expectedRepo}', got '{repo}'."
                    });

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
                            RedirectStandardInput = true,
                            RedirectStandardError = true,
                            UseShellExecute = true
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
                            FileName = "/bin/bash",
                            ArgumentList = { scriptPath },
                            RedirectStandardInput = true,
                            RedirectStandardError = true,
                            UseShellExecute = true
                        }
                    };

                    proc.Start();
                    string stdOut = await proc.StandardOutput.ReadToEndAsync();
                    string stdErr = await proc.StandardError.ReadToEndAsync();
                    await proc.WaitForExitAsync();

                    logger.LogError(stdErr);
                    logger.LogInformation(stdOut);

                    return proc.ExitCode == 0
                        ? Results.Ok(new
                        {
                            success = true,
                            idx,
                            repo,
                            tag,
                            service = serviceName,
                            output = stdOut
                        })
                        : Results.Problem(
                            title: "Script execution failed",
                            detail: stdErr,
                            statusCode: StatusCodes.Status500InternalServerError);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
                finally
                {
                    try
                    {
                        if (File.Exists(scriptPath))
                            File.Delete(scriptPath);
                    }
                    catch
                    {
                        //ignored
                    }
                }
            });
    }
}