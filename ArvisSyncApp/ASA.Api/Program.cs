using ASA.Application.ArvisRequests.CommandHandlers;
using ASA.Application.ArvisRequests.Commands;
using ASA.Application.Interfaces;
using ASA.Infra;
using JasperFx;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

const string swarmAppSettingsPath = "/run/secrets/appsettings.Production.json";
if (File.Exists(swarmAppSettingsPath))
    builder.Configuration.AddJsonFile(swarmAppSettingsPath, false, true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddArvisInfrastructure(builder.Configuration);
builder.Services.AddScoped<ICommandHandler<CreateArvisRequest, long>, CreateArvisRequestHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateArvisRequest, long>, UpdateArvisRequestHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteArvisRequest>, DeleteArvisRequestHandler>();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
await app.RunJasperFxCommands(args);