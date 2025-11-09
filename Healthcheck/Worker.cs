namespace Healthcheck;

public class Worker : BackgroundService
{
    private readonly HttpClient _client;
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _client = httpClientFactory.CreateClient(nameof(Worker));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            HttpResponseMessage response = await _client.GetAsync(string.Empty, stoppingToken);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync(stoppingToken);
            _logger.LogInformation(responseBody);
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}