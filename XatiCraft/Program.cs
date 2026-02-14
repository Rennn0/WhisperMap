using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
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
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        builder.Services.Configure<ClaudflareR2Settings>(
            builder.Configuration.GetSection(nameof(ClaudflareR2Settings)));
        builder.Services.Configure<IpRestrictionSettings>(
            builder.Configuration.GetSection(nameof(IpRestrictionSettings)));
        builder.Services.Configure<ApiKeySettings>(builder.Configuration.GetSection(nameof(ApiKeySettings)));

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
            options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(ApplicationContext)));
        });

        builder.Services.AddExceptionHandler<GeneralExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddSingleton<SystemHealthMonitor>();
        builder.Services.AddScoped<UserGuard>();
        builder.Services.AddTransient<Security, AspDataProtector>();
        builder.Services.AddTransient<IProductRepo, ProductRepo>();
        builder.Services.AddTransient<IProductMetadaRepo, ProductMetadataRepo>();
        string mongoConn = builder.Configuration.GetConnectionString("Mongo") ?? throw new Exception();
        builder.Services.AddTransient<IProductRepo, Data.Repos.MongoImpl.ProductRepo>(_ =>
            new Data.Repos.MongoImpl.ProductRepo(mongoConn));
        builder.Services.AddTransient<IProductMetadaRepo, Data.Repos.MongoImpl.ProductMetadataRepo>(_ =>
            new Data.Repos.MongoImpl.ProductMetadataRepo(mongoConn));
        builder.Services.AddTransient<IAuthorizationRepo, AuthorizationRepo>(_ =>
            new AuthorizationRepo(mongoConn));
        builder.Services.AddTransient<IUploader, ClaudflareR2StorageService>();
        builder.Services.AddTransient<IReader, ClaudflareR2StorageService>();
        builder.Services.AddTransient<ICreateProductHandler, CreateProductHandler>();
        builder.Services.AddTransient<IUploadProductFileHandler, UploadProductFileHandler>();
        builder.Services.AddTransient<IGetProductHandler, GetProductHandler>();
        builder.Services.AddTransient<IGetProductsHandler, GetProductsHandler>();
        builder.Services.AddTransient<IProductCartHandler, ProductCartHandler>();
        builder.Services.AddTransient<IHandler<ApiContract, GetProductsContext>, GetProductsHandler>();
        builder.Services.AddTransient<IHandler<ApiContract, GetProductsContext>, ProductCartHandler>();
        builder.Services.AddTransient<IDeleteProductHandler, DeleteProductHandler>();
        builder.Services.AddTransient<IAuthorizationHandler, GoogleAuthHandler>();

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

        foreach (IBootstrap boot in app.Services.GetRequiredService<IEnumerable<IBootstrap>>())
            await boot.RunAsync();

        await app.RunAsync();
    }
}