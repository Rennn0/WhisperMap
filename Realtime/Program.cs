using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Realtime.Background;
using Realtime.SseFeatures.Formatters;
using XcLib.Sse;
using XcLib.Sse.Core.Signal;
using XcLib.Sse.Core.Stream;
using XcLib.Sse.Core.Streamer;
using XcLib.Sse.DataProvider;

namespace Realtime;

public static class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
        
        const string swarmAppSettingsPath = "/run/secrets/appsettings.Production.json";
        if (File.Exists(swarmAppSettingsPath))
            builder.Configuration.AddJsonFile(swarmAppSettingsPath, false, true);

        builder.Configuration.ConfigureSseDefaults();
        builder.Services.AddSseService();

        builder.Services.AddHostedService<UserStatsBackgroundService>();

        builder.Services.AddCors(opt =>
        {
            opt.AddDefaultPolicy(pol =>
            {
                pol.AllowAnyHeader();
                pol.AllowAnyMethod();
                pol.AllowAnyOrigin();
            });
        });

        builder.Services.AddDistributedSqlServerCache(options =>
        {
            options.ConnectionString = "Server=localhost;Database=master;User Id=sa;Password=Test123test;Trusted_Connection=False;Persist Security Info=False;Encrypt=False";
            options.SchemaName = "cache";
            options.TableName = "RealtimeCache";
            options.DefaultSlidingExpiration = TimeSpan.FromMinutes(5);
        });
        
        WebApplication app = builder.Build();
        app.UseCors();

        app.MapGet("/cache", (
            [FromServices] IDistributedCache cache,
            [FromServices] SseSignalRegistry<UserStats> signalReg,
            [FromQuery(Name = "k")] string key,
            [FromQuery(Name = "v")] string value) =>
        {
            // #NOTE table daamate cli_dan
            // cache.Set(key, Encoding.UTF8.GetBytes(value));

            signalReg.GetSignal("users").PublishAsync(new UserStats { Offline = 99, Online = 33 }).GetAwaiter()
                .GetResult();
            return "x";
        });

        RouteGroupBuilder realtimeGroup = app.MapGroup("/realtime");
        RouteGroupBuilder streamGroup = realtimeGroup.MapGroup("/stream");
        streamGroup.MapGet("/u",
            async (HttpContext context,
                [FromQuery(Name = "sid")] string? streamId,
                [FromServices] SseSignalRegistry<UserStats> sseSignalRegistry,
                [FromKeyedServices(StreamerType.Signal)]
                SseStreamer<UserStats> streamer,
                [FromServices] ISseDataProvider<UserStats> sseDataProvider) =>
            {
                CancellationToken cancellationToken = context.RequestAborted;

                SseSignalRegistry<UserStats>.SignalHandle handle =
                    sseSignalRegistry.GetSignal("users", cancellationToken);

                await streamer.StreamAsync(handle);
            });

        streamGroup.MapGet("/s",
            async (HttpContext context,
                [FromQuery(Name = "sid")] string? streamId,
                [FromServices] SseStreamRegistry<UserStats> sseStreamRegistry,
                [FromKeyedServices(StreamerType.Channel)]
                SseStreamer<UserStats> streamer,
                [FromServices] ISseDataProvider<UserStats> sseDataProvider) =>
            {
                CancellationToken cancellationToken = context.RequestAborted;
                SseStreamRegistry<UserStats>.StreamHandle handle =
                    sseStreamRegistry.GetStream("users", cancellationToken);
                SseStream<UserStats>.StreamSubscription subscription =
                    handle.Subscribe(streamId, cancellationToken);

                UserStats initialVal = await sseDataProvider.GetAsync(handle, cancellationToken);

                await streamer.StreamAsync(subscription.Reader);
            });

        await app.RunAsync();
    }
}