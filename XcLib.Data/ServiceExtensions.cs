using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XcLib.Data.Abstractions;
using XcLib.Data.Mongo;
using XcLib.Data.Mongo.XatiCraft;
using XcLib.Data.Mongo.XatiCraft.Context;
using XcLib.Data.Postgres;
using XcLib.Data.Postgres.XatiCraft.Context;
using XcLib.Data.SqlServer;
using XcLib.Data.SqlServer.Realtime.Context;
using ZNetCS.AspNetCore.Logging.EntityFrameworkCore;
using ProductMetadataRepo = XcLib.Data.Postgres.XatiCraft.ProductMetadataRepo;
using ProductRepo = XcLib.Data.Postgres.XatiCraft.ProductRepo;

namespace XcLib.Data;

public static class ServiceExtensions
{
    public static IHostApplicationBuilder AddXcLibDataModule(this IHostApplicationBuilder builder)
    {
        builder
            .AddSqlServer()
            .AddMongo()
            .AddPostgre();

        return builder;
    }

    public static IHostApplicationBuilder AddSqlLogging<TLogTable>(this IHostApplicationBuilder builder,
        LogLevel level = LogLevel.Warning)
        where TLogTable : Log<int>
    {
        builder.Logging
            .AddEntityFramework<MasterDbContext, TLogTable>()
            .SuppressUntil<MasterDbContext, TLogTable>(level)
            .AddFilter("Microsoft.EntityFrameworkCore", level);

        return builder;
    }

    public static IHostApplicationBuilder AddSqlServer(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSqlServerOptions(builder.Configuration);
        builder.Services.AddSqlServer(builder.Configuration);
        builder.Services.AddSqlServerCache(builder.Configuration);

        return builder;
    }

    public static IHostApplicationBuilder AddPostgre(this IHostApplicationBuilder builder)
    {
        builder.Services.AddPostgreOptions(builder.Configuration);
        builder.Services.AddPostgre(builder.Configuration);

        return builder;
    }

    public static IHostApplicationBuilder AddMongo(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMongoOptions(builder.Configuration);
        builder.Services.AddMongo(builder.Configuration);

        return builder;
    }


    private static IServiceCollection AddSqlServerCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        string? connectionString =
            configuration["cache:conn"] ?? configuration.GetConnectionString(nameof(MasterDbContext));
        ArgumentException.ThrowIfNullOrEmpty(connectionString);

        return services.AddSqlServerCache((Action<SqlServerCacheOptions>)Action);

        void Action(SqlServerCacheOptions opt)
        {
            opt.ConnectionString = connectionString;
            opt.SchemaName = configuration["cache:schema"] ?? "cache";
            opt.TableName = configuration["cache:table"] ?? "RealtimeCache";
            opt.DefaultSlidingExpiration = TimeSpan.FromMinutes(int.Parse(configuration["cache:exp"] ?? "10"));
            opt.ExpiredItemsDeletionInterval = TimeSpan.FromMinutes(int.Parse(configuration["cache:del"] ?? "5"));
        }
    }

    private static IServiceCollection AddSqlServerCache(this IServiceCollection services,
        Action<SqlServerCacheOptions> setupAction) =>
        services.AddDistributedSqlServerCache(setupAction);

    private static IServiceCollection AddSqlServerOptions(this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString(nameof(MasterDbContext));
        ArgumentException.ThrowIfNullOrEmpty(connectionString);

        services.Configure<SqlServerConnectionOptions>(opt =>
        {
            opt.ConnectionString = connectionString;
            opt.Database = "master";
        });
        return services;
    }

    private static IServiceCollection AddMongoOptions(this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Mongo");
        ArgumentException.ThrowIfNullOrEmpty(connectionString);

        services.Configure<MongoConnectionOptions>(opt =>
        {
            opt.ConnectionString = connectionString;
            opt.Database = configuration["mongo:db"] ?? "xc-db";
        });
        return services;
    }

    private static IServiceCollection AddPostgreOptions(this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString(nameof(ApplicationContext));
        ArgumentException.ThrowIfNullOrEmpty(connectionString);

        services.Configure<PostgreConnectionOptions>(opt => { opt.ConnectionString = connectionString; });
        return services;
    }


    private static IServiceCollection AddSqlServer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MasterDbContext>(OptionsAction);

        services.AddTransient<IBootstrap, SqlServerBootstrap>();

        return services;

        void OptionsAction(IServiceProvider provider, DbContextOptionsBuilder opt)
        {
            SqlServerConnectionOptions connectionOptions =
                provider.GetRequiredService<IOptions<SqlServerConnectionOptions>>().Value;
            opt.UseSqlServer(connectionOptions.ConnectionString, sqlOpt => sqlOpt.EnableRetryOnFailure());
            opt.EnableSensitiveDataLogging(false);
            opt.EnableDetailedErrors(false);
            opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }

    private static IServiceCollection AddPostgre(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationContext>(OptionsAction);

        services.AddTransient<IBootstrap, PostgresBootstrap>();

        services.AddTransient<IProductRepo, ProductRepo>();
        services.AddTransient<IProductMetadaRepo, ProductMetadataRepo>();

        return services;

        void OptionsAction(IServiceProvider provider, DbContextOptionsBuilder opt)
        {
            PostgreConnectionOptions connectionOptions =
                provider.GetRequiredService<IOptions<PostgreConnectionOptions>>().Value;
            opt.UseNpgsql(connectionOptions.ConnectionString, pgOptions => pgOptions.EnableRetryOnFailure());
            opt.EnableSensitiveDataLogging(false);
            opt.EnableDetailedErrors(false);
            opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }

    private static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IBootstrap, MongoBootstrap>();

        services.AddTransient<IProductRepo, Mongo.XatiCraft.ProductRepo>();
        services.AddTransient<IProductMetadaRepo, Mongo.XatiCraft.ProductMetadataRepo>();
        services.AddTransient<IAuthorizationRepo, AuthorizationRepo>();
        services.AddTransient<IProductCartRepo, ProductCartRepo>();

        return services;
    }
}