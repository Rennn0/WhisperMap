using Healthcheck;
using Microsoft.Extensions.DependencyInjection.Extensions;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient(nameof(Worker), (provider, client) =>
{
    // IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
    // client.BaseAddress = new Uri(configuration["BackendUrl"] ?? throw new InvalidOperationException());
});
builder.Services.AddHostedService<Worker>();
builder.Logging.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, SomeLoggerProvider>());
IHost host = builder.Build();
host.Run();

internal class SomeLogger(string categoryName) : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine($"HOLAA {state}_ {categoryName}_  {formatter(state, exception)}");
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;
}

[ProviderAlias("SomeSome")]
internal class SomeLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new SomeLogger(categoryName);

    public void Dispose()
    {
        //
    }
}