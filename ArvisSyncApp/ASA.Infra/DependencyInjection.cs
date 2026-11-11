using ASA.Application.ArvisRequests;
using ASA.Application.ArvisRequests.Queries;
using ASA.Application.Interfaces;
using ASA.Application.PopularOptions.Queries;
using ASA.Domain;
using ASA.Domain.Enums;
using ASA.Domain.Events;
using ASA.Domain.Interfaces;
using ASA.Infra.ArvisRequests;
using ASA.Infra.ArvisRequests.QueryHandlers;
using ASA.Infra.PopularOptions;
using ASA.Infra.PopularOptions.QueryHandlers;
using ASA.Infra.Projections;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Weasel.Core;
using Weasel.Postgresql.Tables.Partitioning;

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
            options.Events.AppendMode = EventAppendMode.Quick;
            options.UseSystemTextJsonForSerialization(EnumStorage.AsString);
            
            options.Events.MapEventType<ArvisRequestCreated>(nameof(ArvisRequestCreated));
            options.Events.MapEventType<ArvisRequestRenamed>(nameof(ArvisRequestRenamed));
            options.Events.MapEventType<ArvisRequestHeaderReplaced>(nameof(ArvisRequestHeaderReplaced));
            options.Events.MapEventType<ArvisRequestCookieReplaced>(nameof(ArvisRequestCookieReplaced));
            options.Events.MapEventType<ArvisRequestDeleted>(nameof(ArvisRequestDeleted));

            options.Projections.Add(new ArvisRequestAggregation(), ProjectionLifecycle.Live);
            options.Projections.Add(new ArvisRequestViewProjection(), ProjectionLifecycle.Inline);

            options.Schema.For<ArvisRequestView>().Metadata(m => m.Revision.MapTo(x => x.Version))
                .Index(x => x.UpdatedAt);

            options.Schema.For<PopularRequestOption>().DatabaseSchemaName("configs").PartitionOn(x => x.Place, x =>
            {
                ListPartitioning l = x.ByList();
                foreach (string name in Enum.GetNames<ConfigPlace>()) l.AddPartition(name.ToLowerInvariant(), name);
            });
        })
        .UseLightweightSessions()
        .ApplyAllDatabaseChangesOnStartup();

        services.AddScoped<IEventRepository<ArvisRequest>, ArvisEventRepository>();
        services.AddScoped<IRepository<PopularRequestOption>, PopularOptionsRepository>();
        
        services.AddScoped<IQueryHandler<GetArvisRequest, ArvisRequestView?>, GetArvisRequestHandler>();
        services.AddScoped<IQueryHandler<ListArvisRequest, IReadOnlyList<ArvisRequestView>>, ListArvisRequestHandler>();
        services
            .AddScoped<IQueryHandler<GetArvisRequestHistory, ArvisRequestWithHistory>,
                GetArvisRequestHistoryHandler>();

        services.AddScoped<IQueryHandler<GetPopularOption, PopularRequestOption?>, GetPopularOptionHandler>();
        services
            .AddScoped<IQueryHandler<ListPopularOptions, IReadOnlyList<PopularRequestOption>>,
                ListPopularOptionHandler>();
        return services;
    }
}