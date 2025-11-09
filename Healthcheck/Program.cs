using Healthcheck;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient(nameof(Worker), (provider, client) =>
{
    IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["BackendUrl"] ?? throw new InvalidOperationException());
});
builder.Services.AddHostedService<Worker>();
IHost host = builder.Build();
host.Run();