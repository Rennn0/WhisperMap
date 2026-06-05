using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Routing.Constraints;
using Realtime.Api.Payment;
using Realtime.Api.Stream;
using Realtime.Background;
using Realtime.Exceptions;
using Swashbuckle.AspNetCore.SwaggerUI;
using XcLib.Data;
using XcLib.Data.SqlServer.Realtime.Entities;
using XcLib.Shared;
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

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
        
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

        builder.Services.Configure<RouteOptions>(options =>
            options.SetParameterPolicy<RegexInlineRouteConstraint>("regex"));
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddProblemDetails();

        builder.Services.AddHostedService<UserStatsBackgroundService>();

        builder.AddSqlLogging<RealtimeLog>();
        builder.AddSqlServer();
        builder.AddMongo();
        builder.AddPayments();
        
        WebApplication app = builder.Build();
        app.UseExceptionHandler();

        app.UseCors(app.Environment.IsDevelopment() ? "dev" : "prod");

        RouteGroupBuilder mainGroup = app.MapGroup("/realtime");
        mainGroup.MapPaymentApi();
        
        RouteGroupBuilder streamGroup = mainGroup.MapGroup("/stream");
        streamGroup.ApiGetStreamCache();
        streamGroup.ApiGetSignal();
        streamGroup.ApiGetStream();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DefaultModelsExpandDepth(-1);
                c.DocExpansion(DocExpansion.None);
            });
        }
        
        await app.RunBootStrapsAsync();
        await app.RunAsync();
        //#TODO sheamowme sesia rato etisheba avtorizebuls
    }
}