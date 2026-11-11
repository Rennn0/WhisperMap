using ASA.Api;
using ASA.Application.ArvisRequests.CommandHandlers;
using ASA.Application.ArvisRequests.Commands;
using ASA.Application.Interfaces;
using ASA.Application.PopularOptions.CommandHandlers;
using ASA.Application.PopularOptions.Commands;
using ASA.Infra;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

const string swarmAppSettingsPath = "/run/secrets/appsettings.Production.json";
if (File.Exists(swarmAppSettingsPath))
    builder.Configuration.AddJsonFile(swarmAppSettingsPath, false, true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GeneralExceptionHandler>();

builder.Services.AddArvisInfrastructure(builder.Configuration);
builder.Services.AddScoped<ICommandHandler<CreateArvisRequest, long>, CreateArvisRequestHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateArvisRequest, long>, UpdateArvisRequestHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteArvisRequest>, DeleteArvisRequestHandler>();

builder.Services.AddScoped<ICommandHandler<CreatePopularOption, long>, CreatePopularOptionHandler>();

WebApplication app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();