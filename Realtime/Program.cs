using Realtime.Background;
using XcLib.Data;
using XcLib.Data.SqlServer.Realtime.Entities;
using XcLib.Sse;
using XcLib.Sse.Options;

namespace Realtime;

public static partial class Program
{
    private static Task<SseOptions> OptionsLoaderTask() =>
        // await Task.Delay(4000);
        Task.FromResult(new SseOptions
        {
            PingInterval = (uint)Random.Shared.Next(3, 10)
        });

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
                    "https://www.xati.org"
                );
            });
            opt.AddPolicy("dev", pol =>
            {
                pol.WithMethods("GET");
                pol.WithOrigins(
                    "http://localhost:18000"
                );
            });
        });

        builder.Services.AddHostedService<UserStatsBackgroundService>();

        builder.AddSqlLogging<RealtimeLog>();
        builder.AddSqlServer();
        
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
        RouteGroupBuilder streamGroup = mainGroup.MapGroup("/stream");
        
        streamGroup.ApiGetStreamCache();
        streamGroup.ApiGetSignal();
        streamGroup.ApiGetStream();
        await app.RunAsync();
    }
}