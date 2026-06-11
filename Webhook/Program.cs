using System.Text.Json.Serialization;
using Webhook.Meshes.Webhook;
using Webhook.Objects;
using XcLib.Data;
using XcLib.Data.SqlServer.Realtime.Entities;
using XcLib.Shared;

namespace Webhook;

public static partial class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
        });

        const string swarmAppSettingsPath = "/run/secrets/appsettings.Production.json";
        if (File.Exists(swarmAppSettingsPath))
            builder.Configuration.AddJsonFile(swarmAppSettingsPath, false, true);

        builder.Services
            .AddReactiveBus<DockerWebhookRequest>()
            .AddDataflowMesh<WebhookMesh>()
            .AddDataflowNodeFactory<DockerWebhookRequest>();
        
        builder.AddSqlLogging<MasterLog>(LogLevel.Information);
        builder.AddSqlServer();
        builder.AddMongo();

        WebApplication app = builder.Build();

        RouteGroupBuilder webHookGroup = app.MapGroup("/webhook");
        webHookGroup.ApiPostWebhook();

        app.Run();
    }


    [JsonSerializable(typeof(WebhookLogEntry))]
    [JsonSerializable(typeof(DockerWebhookRequest))]
    [JsonSerializable(typeof(PushData))]
    internal partial class AppJsonContext : JsonSerializerContext
    {
    }
}