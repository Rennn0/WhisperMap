namespace Healthcheck;

public class Worker : BackgroundService
{
    private readonly HttpClient _client;
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _client = httpClientFactory.CreateClient("healthcheck");
        _client.BaseAddress = new Uri(configuration["BackendUrl"] ?? throw new Exception());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await _client.GetAsync(string.Empty, stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }
    }
}