using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Realtime.Background;
using XcLib.Data.SqlServer.Realtime.Context;
using XcLib.Data.SqlServer.Realtime.Entities;
using XcLib.Sse;
using XcLib.Sse.Options;

namespace Realtime;

public static partial class Program
{
    private static async Task<SseOptions> OptionsLoaderTask()
    {
        // await Task.Delay(4000);
        return new SseOptions
        {
            PingInterval = (uint)Random.Shared.Next(3, 10)
        };
    }
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
        
        const string swarmAppSettingsPath = "/run/secrets/appsettings.Production.json";
        if (File.Exists(swarmAppSettingsPath))
            builder.Configuration.AddJsonFile(swarmAppSettingsPath, false, true);

        builder.Configuration.ConfigureSseDefaults(OptionsLoaderTask);
        builder.Services.AddSseService();

        builder.Services.AddCors(opt =>
        {
            opt.AddPolicy("prod", pol =>
            {
                pol.WithMethods("GET");
                pol.WithOrigins(
                    "https://xati.org",
                    "https://api.xati.org",
                    "https://www.xati.org",
                    "https://www.api.xati.org"
                );
            });
            opt.AddPolicy("dev", pol =>
            {
                pol.WithMethods("GET");
                pol.WithOrigins("http://localhost:18000");
            });
        });

        builder.Services.AddDbContext<MasterDbContext>(opt =>
        {
            opt.UseSqlServer(builder.Configuration.GetConnectionString(nameof(MasterDbContext)),
                sqlOpt => { sqlOpt.EnableRetryOnFailure(); });
            opt.EnableSensitiveDataLogging(false);
            opt.EnableDetailedErrors();
            opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });
        builder.Services.AddDistributedSqlServerCache(options =>
        {
            options.ConnectionString = builder.Configuration.GetConnectionString(nameof(MasterDbContext));
            options.SchemaName = "cache";
            options.TableName = "RealtimeCache";
            options.DefaultSlidingExpiration = TimeSpan.FromMinutes(10);
            options.ExpiredItemsDeletionInterval = TimeSpan.FromMinutes(5);
        });
        builder.Logging
            .AddEntityFramework<MasterDbContext, RealtimeLog>()
            .SuppressUntil<MasterDbContext, RealtimeLog>(LogLevel.Warning)
            .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
        
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
        }

        RouteGroupBuilder mainGroup = app.MapGroup("/realtime");
        RouteGroupBuilder webHookGroup = mainGroup.MapGroup("/webhook");
        RouteGroupBuilder streamGroup = mainGroup.MapGroup("/stream");

        webHookGroup.ApiPostWebhook();
        
        streamGroup.ApiGetStreamCache();
        streamGroup.ApiGetSignal();
        streamGroup.ApiGetStream();

        mainGroup.MapGet("/test",
            ([FromServices] MasterDbContext context, [FromServices] ILoggerFactory loggerFactory,
                [FromServices] IDistributedCache cache) =>
            {
                List<MasterLog> logs = context.MasterLogs.ToList();
                List<RealtimeCache> caches = context.RealtimeCaches.ToList();

                foreach (RealtimeCache cach in caches) cach.Value = ",M"u8.ToArray();

                context.SaveChanges();

                cache.SetString(DateTimeOffset.Now.ToString(), "1");
                ILogger logger = loggerFactory.CreateLogger("test");

                logger.LogWarning("oops");

                return Results.Ok();
            });
        
        await app.RunAsync();
    }
}