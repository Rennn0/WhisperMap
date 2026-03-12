namespace Realtime;

public static class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
        builder.Services.AddSingleton<SseStream<string>, ProductsSseStream>();
        builder.Services.AddSingleton<SseStream<float>, ProductsPriceSseStream>();

        WebApplication app = builder.Build();
        RouteGroupBuilder rtGroup = app.MapGroup("/realtime");
        rtGroup.MapGet("/", () => "xx");
        rtGroup.MapGet("/products",
            async (HttpContext ctx, SseStream<float> stream, CancellationToken cancellationToken) =>
            {
                SseStreamer streamer = ctx.CreateSseStreamer(cancellationToken);
                await using SseStream<float>.StreamSubscription subscription = stream.Subscribe();

                try
                {
                    await streamer.StreamAsync(subscription.Reader,
                        "products",
                        TimeSpan.FromSeconds(3),
                        new SseDefaultFormatter());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

        AsyncLocal<SomeWrapper> counter = new AsyncLocal<SomeWrapper>
            { Value = new SomeWrapper { Val = 1 } };

        Timer timer = new Timer(async void (_) =>
        {
            try
            {
                if (Random.Shared.Next(0, 2) == 0) return;

                SseStream<string> sStream = app.Services.GetRequiredService<SseStream<string>>();
                SseStream<float> fStream = app.Services.GetRequiredService<SseStream<float>>();

                await sStream.PublishAsync(DateTimeOffset.Now.ToString());
                await fStream.PublishAsync(counter.Value.Val++);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10));

        await app.RunAsync();
    }

    private class SomeWrapper
    {
        public float Val { get; set; }
    }
}