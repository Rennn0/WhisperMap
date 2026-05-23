using Microsoft.Extensions.DependencyInjection;
using XcLib.Shared.Dataflow;
using XcLib.Shared.Dataflow.Interfaces;

namespace XcLib.Shared;

public static class ServiceExtensions
{
    public static IServiceCollection AddDataflowNodeFactory<T>(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IDataflowNodeFactory<T>, DataflowNodeFactory<T>>();

        return serviceCollection;
    }
}