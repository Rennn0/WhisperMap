using System.Text.Json.Serialization;
using XcLib.Data;
using XcLib.Data.SqlServer.Realtime.Entities;

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

        builder.AddSqlLogging<MasterLog>();
        builder.AddSqlServer();

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