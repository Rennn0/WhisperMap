using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using XatiCraft.Data;
using XatiCraft.Data.Repos;
using XatiCraft.Data.Repos.EfCoreImpl;
using XatiCraft.Guards;
using XatiCraft.Handlers.Api;
using XatiCraft.Handlers.Impl;
using XatiCraft.Handlers.Read;
using XatiCraft.Handlers.Upload;
using XatiCraft.Objects;
using XatiCraft.Settings;

namespace XatiCraft;

/// <summary>
/// </summary>
public static class Program
{
    /// <summary>
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
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
            opt.AddPolicy("policy_session", context =>
            {
                string session = context.Request.Cookies["session"] ?? "";

                return RateLimitPartition.GetFixedWindowLimiter(session, f => new FixedWindowRateLimiterOptions
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
                // context.HttpContext.Response.WriteAsync("rate_limit_exc", token);
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

        builder.Services.AddSingleton<SystemHealthMonitor>();
        builder.Services.AddScoped<UserManager>();
        builder.Services.AddTransient<IProductRepo, ProductRepo>();
        builder.Services.AddTransient<IProductMetadaRepo, ProductMetadataRepo>();
        builder.Services.AddTransient<IUploader, ClaudflareR2StorageService>();
        builder.Services.AddTransient<IReader, ClaudflareR2StorageService>();
        builder.Services.AddTransient<ICreateProductHandler, CreateProductHandler>();
        builder.Services.AddTransient<IUploadProductFileHandler, UploadProductFileHandler>();
        builder.Services.AddTransient<IGetProductHandler, GetProductHandler>();
        builder.Services.AddTransient<IGetProductsHandler, GetProductsHandler>();

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
        app.MapControllers();
        app.UseResponseCaching();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.Run();
    }
}