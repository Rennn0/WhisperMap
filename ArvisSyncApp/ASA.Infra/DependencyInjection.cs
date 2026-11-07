using ASA.Application.ArvisRequests;
using ASA.Application.ArvisRequests.Queries;
using ASA.Application.Interfaces;
using ASA.Domain.Events;
using ASA.Domain.Interfaces;
using ASA.Infra.Projections;
using ASA.Infra.QueryHandlers;
using JasperFx.Events.Projections;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ASA.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddArvisInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMarten(options =>
        {
            options.Connection(configuration.GetConnectionString("ApplicationContext") ??
                               throw new InvalidOperationException());
            options.DatabaseSchemaName = "marten_arvis";

            options.Events.MapEventType<ArvisRequestCreated>(nameof(ArvisRequestCreated));
            options.Events.MapEventType<ArvisRequestRenamed>(nameof(ArvisRequestRenamed));
            options.Events.MapEventType<ArvisRequestHeaderReplaced>(nameof(ArvisRequestHeaderReplaced));
            options.Events.MapEventType<ArvisRequestCookieReplaced>(nameof(ArvisRequestCookieReplaced));
            options.Events.MapEventType<ArvisRequestDeleted>(nameof(ArvisRequestDeleted));

            options.Projections.Add(new ArvisRequestAggregation(), ProjectionLifecycle.Live);
            options.Projections.Add(new ArvisRequestViewProjection(), ProjectionLifecycle.Inline);

            options.Schema.For<ArvisRequestView>().Metadata(m => m.Revision.MapTo(x => x.Version))
                .Index(x => x.UpdatedAt);
        }).ApplyAllDatabaseChangesOnStartup();

        services.AddScoped<IArvisRequestRepository, MartenArvisRequestRepository>();
        services.AddScoped<IQueryHandler<GetArvisRequest, ArvisRequestView?>, GetArvisRequestHandler>();
        services.AddScoped<IQueryHandler<ListArvisRequest, IReadOnlyList<ArvisRequestView>>, ListArvisRequestHandler>();


        return services;
    }
}