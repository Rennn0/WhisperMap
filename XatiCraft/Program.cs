using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using XatiCraft.Data;
using XatiCraft.Handlers.Impl;
using XatiCraft.Handlers.Read;
using XatiCraft.Handlers.Upload;
using XatiCraft.Settings;

namespace XatiCraft;

public static class Program
{
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
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.Name = "XatiCraft.Session";
            options.Cookie.MaxAge = TimeSpan.FromMinutes(30);
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        builder.Services.AddDbContext<ApplicationContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(ApplicationContext)));
        });

        builder.Services.AddScoped<IUploader, ClaudflareR2StorageService>();
        builder.Services.AddScoped<IReader, ClaudflareR2StorageService>();

        WebApplication app = builder.Build();
        app.UseForwardedHeaders();
        app.UseSession();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.Run();
    }
}