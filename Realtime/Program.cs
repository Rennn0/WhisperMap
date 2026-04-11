using Microsoft.EntityFrameworkCore;
using Realtime.Background;
using XcLib.Data.SqlServer.Realtime.Context;
using XcLib.Sse;
using XcLib.Sse.Options;
using ZNetCS.AspNetCore.Logging.EntityFrameworkCore;

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
                pol.WithOrigins("https://xati.org", "https://api.xati.org", "https://www.xati.org",
                    "https://www.api.xati.org");
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

            opt.EnableSensitiveDataLogging();
            opt.EnableDetailedErrors();
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
        builder.Logging
            .AddEntityFramework<RealtimeDbContext, Log>()
            .SuppressUntil<RealtimeDbContext, Log>(LogLevel.Warning)
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
        
        await app.RunAsync();
    }
}