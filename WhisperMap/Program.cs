using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhisperMap.Contracts;
using WhisperMap.Data;
using WhisperMap.Grains;
using WhisperMap.Handlers;
using WhisperMap.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MapContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MapContext"), o => o.UseNetTopologySuite()));

builder.Services.AddCors(opt => { opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()); });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryGrainStorageAsDefault();
});

builder.Services.AddScoped<IUserHandler, MapContextUserHandler>();
builder.Services.AddScoped<IPinHandler, MapContextPinHandler>();

WebApplication app = builder.Build();


app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api/user", async ([FromBody] PostUserContract contract, [FromServices] IUserHandler handler) =>
{
    await handler.AddUserAsync(contract);
    return Results.NoContent();
});

app.MapPost("/api/pin",
    async ([FromBody] PostPinContract contract, [FromServices] IPinHandler handler,
        [FromServices] IGrainFactory grainFactory) =>
    {
        Pin pin = await handler.AddPinAsync(contract);

        if (contract.TextBox is not null)
        {
            ITextBoxGrain grain = grainFactory.GetGrain<ITextBoxGrain>(pin.Id.ToString());
            await grain.PackAsync(contract.PedestrianId, pin.Id, contract.TextBox.Text);
        }

        if (contract.AudioBox is not null)
        {
            IAudioBoxGrain grain = grainFactory.GetGrain<IAudioBoxGrain>(pin.Id.ToString());
            await grain.PackAsync(contract.PedestrianId, pin.Id, contract.AudioBox.Audio);
        }

        return Results.NoContent();
    });

app.MapGet("/api/pin/unpack",
    async (int pinId, int founder, [FromServices] IPinHandler handler, [FromServices] IGrainFactory grainFactory) =>
    {
        Pin pin = await handler.GetPinAsync(pinId) ?? throw new Exception();
        string[] objDesc = pin?.Object?.Description?.Split('_', StringSplitOptions.RemoveEmptyEntries) ?? [];

        Dictionary<string, object> gifts = [];

        if (objDesc.Any(d => d.Equals(nameof(TextBox))))
        {
            ITextBoxGrain grain = grainFactory.GetGrain<ITextBoxGrain>(pin!.Id.ToString());
            string gift = await grain.UnPackAsync(founder);
            gifts[nameof(TextBox)] = gift;
        }

        if (objDesc.Any(d => d.Equals(nameof(AudioBox))))
        {
            IAudioBoxGrain grain = grainFactory.GetGrain<IAudioBoxGrain>(pin!.Id.ToString());
            byte[] gift = await grain.UnPackAsync(founder);
            gifts[nameof(AudioBox)] = gift;
        }

        return gifts;
    });

app.MapGet("/api/pin/nearby",
    async (double latitude, double longitude, double radius, [FromServices] IPinHandler handler,
        [FromServices] IGrainFactory grainFactory) =>
    {
        IEnumerable<PinWithDistance> results =
            await handler.GetNearbyPinsAsync(new GetNearbyPinsContract(latitude, longitude, radius));
        var projectionTasks = results.Select(async r =>
        {
            bool? textG = r.Pin.Object?.Description?.Contains("Text");

            IBlackBoxGrain grain;
            if (textG.HasValue && textG.Value)
                grain = grainFactory.GetGrain<ITextBoxGrain>(r.Pin.Id.ToString());
            else
                grain = grainFactory.GetGrain<IAudioBoxGrain>(r.Pin.Id.ToString());
            CommonStatistics statistics = await grain.GetStatisticsAsync();
            return new
            {
                r.Pin.Id,
                Ped = new
                {
                    r.Pin.Pedestrian.Id,
                    r.Pin.Pedestrian.Username
                },
                r.Pin.Location.X,
                r.Pin.Location.Y,
                r.Distance,
                r.Pin.Createdat,
                statistics
            };
        });

        return await Task.WhenAll(projectionTasks);
    }
);

app.Run();