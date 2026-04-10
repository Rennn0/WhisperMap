using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Realtime.Background;
using Realtime.Context;
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
        
        builder.Services.AddCors(opt =>
        {
            opt.AddPolicy("prod", pol =>
            {
                pol.WithMethods("GET");
                pol.WithOrigins("https://xati.org");
                
            });
            opt.AddPolicy("dev", pol =>
            {
                pol.WithMethods("GET");
                pol.WithOrigins("http://localhost:18000");
            });
        });

        builder.Services.AddDbContext<RealtimeDbContext>(opt =>
        {
            opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlDefault"),
                sqlOpt => { sqlOpt.EnableRetryOnFailure(); });

            opt.EnableSensitiveDataLogging(false);
            opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });
        
        builder.Services.AddDistributedSqlServerCache(options =>
        {
            options.ConnectionString = builder.Configuration.GetConnectionString("SqlDefault");
            options.SchemaName = "cache";
            options.TableName = "RealtimeCache";
            options.DefaultSlidingExpiration = TimeSpan.FromMinutes(10);
            options.ExpiredItemsDeletionInterval = TimeSpan.FromMinutes(5);
        });

        builder.Logging.AddEntityFramework<RealtimeDbContext>();

        builder.Services.AddHostedService<UserStatsBackgroundService>();
        
        WebApplication app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseCors("dev");
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseCors("prod");
            app.UseAntiforgery();
        }

        app.MapGet("/cache", (
            [FromServices] IDistributedCache cache,
            [FromServices] SseSignalRegistry<UserStats> signalReg,
            [FromServices] ILogger<WebApplication> logger,
            [FromQuery(Name = "k")] string key,
            [FromQuery(Name = "v")] string value) =>
        {
            // byte[]? c1 = cache.Get(key);
            // byte[]? c2 = cache.Get(key + "2");

            cache.Set(key, Encoding.UTF8.GetBytes(value));
            cache.Set(key + "2", Encoding.UTF8.GetBytes(value), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10),
                SlidingExpiration = TimeSpan.FromMinutes(1)
            });

            logger.LogWarning("hello there");
            
            signalReg.GetSignal("users").PublishAsync(new UserStats { Offline = 99, Online = 33 }).GetAwaiter()
                .GetResult();
            return "x";
        });

        RouteGroupBuilder realtimeGroup = app.MapGroup("/realtime");
        RouteGroupBuilder streamGroup = realtimeGroup.MapGroup("/stream");
        streamGroup.MapGet("/signal",
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

        streamGroup.MapGet("/u",
            async (HttpContext context,
                [FromQuery(Name = "sid")] string? streamId,
                [FromServices] SseStreamRegistry<UserStats> sseStreamRegistry,
                [FromKeyedServices(StreamerType.Enumerable)]
                SseStreamer<UserStats> streamer,
                [FromServices] ISseDataProvider<UserStats> sseDataProvider) =>
            {
                CancellationToken cancellationToken = context.RequestAborted;
                SseStreamRegistry<UserStats>.StreamHandle handle =
                    sseStreamRegistry.GetStream("users", cancellationToken);
                SseStream<UserStats>.StreamSubscription subscription =
                    handle.Subscribe(streamId, cancellationToken);

                UserStats initialVal = await sseDataProvider.GetAsync(handle, cancellationToken);

                await streamer.StreamAsync(subscription.ReadAllAsync(cancellationToken), initialValue: initialVal);
            });

        await app.RunAsync();
    }
}