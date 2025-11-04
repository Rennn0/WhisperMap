using XatiCraft.Handlers.Impl;
using XatiCraft.Handlers.Read;
using XatiCraft.Handlers.Upload;
using XatiCraft.Settings;

namespace XatiCraft;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.Configure<ClaudflareR2Settings>(
            builder.Configuration.GetSection(nameof(ClaudflareR2Settings)));

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddScoped<IUploader, ClaudflareR2StorageService>();
        builder.Services.AddScoped<IReader, ClaudflareR2StorageService>();

        WebApplication app = builder.Build();
        app.UseAuthorization();
        app.MapControllers();
        app.UseSwagger();
        app.UseSwaggerUI();

        app.Run();
    }
}