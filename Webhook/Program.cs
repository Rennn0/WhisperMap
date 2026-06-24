using System.Text;
using System.Text.Json.Serialization;
using MemoryPack;
using Webhook.Objects;
using XcLib.Shared.Utils;

namespace Webhook;

public static partial class Program
{
    public static void Main(string[] args)
    {
        // WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
        //
        // builder.Services.ConfigureHttpJsonOptions(options =>
        // {
        //     options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
        // });
        //
        // const string swarmAppSettingsPath = "/run/secrets/appsettings.Production.json";
        // if (File.Exists(swarmAppSettingsPath))
        //     builder.Configuration.AddJsonFile(swarmAppSettingsPath, false, true);
        //
        // builder.Services
        //     .AddReactiveBus<DockerWebhookRequest>()
        //     .AddDataflowMesh<WebhookMesh>()
        //     .AddDataflowNodeFactory<DockerWebhookRequest>();
        //
        // builder.AddSqlLogging<MasterLog>(LogLevel.Information);
        // builder.AddSqlServer();
        // builder.AddMongo();
        //
        // WebApplication app = builder.Build();
        //
        // RouteGroupBuilder webHookGroup = app.MapGroup("/webhook");
        // webHookGroup.ApiPostWebhook();
        //
        // app.Run();


        byte[] bytes = File.ReadAllBytes("../_test_/MOCK_DATA.json");
        Foo foo = new Foo(1, 2, Encoding.UTF8.GetString(bytes));
        Foo bar = new Foo(1, 2, "hello world");
        MemoryPackImplSerializer serial = new MemoryPackImplSerializer();
        SystemJsonImplSerializer sys = new SystemJsonImplSerializer();

        ReadOnlyMemory<byte> ser = serial.Serialize(foo);
        ReadOnlyMemory<byte> sy = sys.Serialize(foo);
        
        Console.WriteLine($"{ser.Length} vs {sy.Length} ({bytes.Length})");
        
        ser = serial.Serialize(bar);
        sy = sys.Serialize(bar);
        
        Console.WriteLine($"{ser.Length} vs {sy.Length} ({bytes.Length})");

        // ReadOnlyMemory<byte> comp = serial.Compress(bytes);
        // ReadOnlyMemory<byte> compSys = sys.Compress(bytes);
        // Console.WriteLine($"{bytes.Length} vs {comp.Length} vs {compSys.Length}");
        // ReadOnlyMemory<byte> compDec = serial.Decompress(comp);
        // ReadOnlyMemory<byte> compSysDec = sys.Decompress(compSys);
        // Console.WriteLine($"{bytes.Length} vs {compDec.Length} vs {compSysDec.Length}");
        // Console.WriteLine(Encoding.UTF8.GetString(compDec.Span));
    }


    [JsonSerializable(typeof(WebhookLogEntry))]
    [JsonSerializable(typeof(DockerWebhookRequest))]
    [JsonSerializable(typeof(PushData))]
    internal partial class AppJsonContext : JsonSerializerContext
    {
    }

    [MemoryPackable]
    public partial record Foo(int A, int B, string C);
}