using System.Diagnostics.Metrics;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Data.Repos;
using XatiCraft.Data.Repos.EfCoreImpl;
using XatiCraft.Data.Repos.MongoImpl;
using XatiCraft.Guards;
using XatiCraft.Handlers;
using XatiCraft.Handlers.Api;
using XatiCraft.Handlers.Impl;
using XatiCraft.Handlers.Read;
using XatiCraft.Handlers.Upload;
using XatiCraft.Settings;
using ProductMetadataRepo = XatiCraft.Data.Repos.EfCoreImpl.ProductMetadataRepo;
using ProductRepo = XatiCraft.Data.Repos.EfCoreImpl.ProductRepo;
using OpenTelemetry.Metrics;

namespace XatiCraft;

/// <summary>
/// </summary>
public static class Program
{
    /// <summary>
    /// </summary>
    /// <param name="args"></param>
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        const string swarmAppSettingsPath = "/run/secrets/appsettings.Production.json";
        if (File.Exists(swarmAppSettingsPath))
            builder.Configuration.AddJsonFile(swarmAppSettingsPath, false, true);

        var meter = new Meter("xc_api_meter");
        var reqCounter = meter.CreateCounter<long>("xc_req_counter");

        new Timer(_ => reqCounter.Add(1)).Change(1000,1000);
        
        builder.Logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = false;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddPrometheusExporter()
                    .AddMeter(meter.Name);
            });
        
        builder.Services.Configure<ClaudflareR2Settings>(
            builder.Configuration.GetSection(nameof(ClaudflareR2Settings)));
        builder.Services.Configure<IpRestrictionSettings>(
            builder.Configuration.GetSection(nameof(IpRestrictionSettings)));
        builder.Services.Configure<ApiKeySettings>(builder.Configuration.GetSection(nameof(ApiKeySettings)));
        builder.Services.Configure<GithubAuthSettings>(builder.Configuration.GetSection(nameof(GithubAuthSettings)));
        builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection(nameof(GoogleAuthSettings)));

        builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressMapClientErrors = true;
        }).AddJsonOptions(opt =>
        {
            opt.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseSerializer();
            opt.JsonSerializerOptions.WriteIndented = true;
            opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
        builder.Services.AddRateLimiter(opt =>
        {
            opt.AddPolicy(AuthGuard.SessionPolicy, context =>
            {
                string session = context.Request.Cookies[AuthGuard.SessionCookie] ?? "";

                return RateLimitPartition.GetFixedWindowLimiter(session, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 60,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });

            opt.OnRejected = (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.WriteAsync("rate_limit_exc", token);
                return ValueTask.CompletedTask;
            };
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(opt =>
        {
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            opt.IncludeXmlComments(xmlPath);
        });
        builder.Services.AddMemoryCache();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.IdleTimeout = TimeSpan.FromDays(1);
            options.Cookie.Name = "XatiCraft.Session";
            options.Cookie.MaxAge = TimeSpan.FromDays(1);
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });
        builder.Services.AddResponseCompression();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddDbContext<ApplicationContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(ApplicationContext)), pgOptions => pgOptions.EnableRetryOnFailure(int.MaxValue));
        });

        builder.Services.AddExceptionHandler<GeneralExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddHttpClient();

        builder.Services.AddSingleton<SystemHealthMonitor>();
        builder.Services.AddScoped<UserGuard>();
        builder.Services.AddTransient<Security, AspDataProtector>();
        builder.Services.AddTransient<Security, SimpleBase64Protector>();
        builder.Services.AddTransient<IProductRepo, ProductRepo>();
        builder.Services.AddTransient<IProductMetadaRepo, ProductMetadataRepo>();
        string mongoConn = builder.Configuration.GetConnectionString("Mongo") ?? throw new Exception("MongoConnection");
        builder.Services.AddTransient<IProductRepo, Data.Repos.MongoImpl.ProductRepo>(_ =>
            new Data.Repos.MongoImpl.ProductRepo(mongoConn));
        builder.Services.AddTransient<IProductMetadaRepo, Data.Repos.MongoImpl.ProductMetadataRepo>(_ =>
            new Data.Repos.MongoImpl.ProductMetadataRepo(mongoConn));
        builder.Services.AddTransient<IAuthorizationRepo, AuthorizationRepo>(_ =>
            new AuthorizationRepo(mongoConn));
        builder.Services.AddTransient<IProductCartRepo, ProductCartRepo>(_ =>
            new ProductCartRepo(mongoConn));
        builder.Services.AddTransient<IUploader, ClaudflareR2StorageService>();
        builder.Services.AddTransient<IReader, ClaudflareR2StorageService>();
        builder.Services.AddTransient<IProductManager, ProductManager>();
        builder.Services.AddTransient<IUploadProductFileHandler, UploadProductFileHandler>();
        builder.Services.AddTransient<IGetProductHandler, GetProductHandler>();
        builder.Services.AddTransient<IGetProductsHandler, GetProductsGeneralHandler>();
        builder.Services.AddTransient<IProductCartHandler, GetProductCartCookieHandler>();
        builder.Services.AddTransient<IHandler<ApiContract, GetProductsContext>, GetProductsCartHandler>();
        builder.Services.AddTransient<IHandler<ApiContract, GetProductsContext>, GetProductsGeneralHandler>();
        builder.Services.AddTransient<IHandler<ApiContract, GetProductsContext>, GetProductCartCookieHandler>();
        builder.Services.AddTransient<IHandler<ApiContract, AddProductInCartContext>, GetProductCartCookieHandler>();
        builder.Services.AddTransient<IHandler<ApiContract, RemoveProductFromCartContext>, GetProductCartCookieHandler>();
        builder.Services.AddTransient<IHandler<ApiContract, RemoveProductFromCartContext>, ProductCartHandlerMongo>();
        builder.Services.AddTransient<IHandler<ApiContract, AddProductInCartContext>, ProductCartHandlerMongo>();
        builder.Services.AddTransient<IDeleteProductHandler, DeleteProductHandler>();
        builder.Services.AddTransient<IAuthorizationHandler, GoogleAuthHandler>();
        builder.Services.AddTransient<IAuthorizationHandler, GithubAuthHandler>();

        builder.Services.AddTransient<IBootstrap, MongoBootstrap>(_ => new MongoBootstrap(mongoConn));

#if USE_CERT
        builder.Services.AddCert();
#endif
        WebApplication app = builder.Build();
        app.Services.GetRequiredService<SystemHealthMonitor>();
        app.UseForwardedHeaders();
        app.UseSession();
        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseExceptionHandler();
        app.MapControllers();
        app.UseResponseCaching();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapPrometheusScrapingEndpoint("/metrics");

        await Parallel.ForEachAsync( app.Services.GetRequiredService<IEnumerable<IBootstrap>>(), CancellationToken.None, (bootstrap, _) => bootstrap.RunAsync());

        await app.RunAsync();
    }
}