using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using XcLib.Sse.Core.Signal;
using XcLib.Sse.Core.Stream;
using XcLib.Sse.Core.Streamer;
using XcLib.Sse.DataProvider;
using XcLib.Sse.Formatters;

namespace XcLib.Sse.Options;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSseService(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<SseSignalOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations();

        services.AddOptions<SseStreamOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations();

        return services.AddSseCore(assemblies);
    }

    public static IServiceCollection AddSseService(
        this IServiceCollection services,
        string sectionName,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        services.AddOptionsWithValidateOnStart<SseSignalOptions>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations();

        services.AddOptionsWithValidateOnStart<SseStreamOptions>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations();

        return services.AddSseCore(assemblies);
    }

    public static IServiceCollection AddSseService(
        this IServiceCollection services,
        Action<SseSignalOptions> configureSignal,
        Action<SseStreamOptions> configureStream,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureSignal);
        ArgumentNullException.ThrowIfNull(configureStream);

        services.Configure(configureSignal);
        services.Configure(configureStream);

        return services.AddSseCore(assemblies);
    }

    private static IServiceCollection AddSseCore(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddLogging();
        services.AddHttpContextAccessor();

        services.TryAddKeyedTransient<SseStreamer, SseSignalStreamer>(SseStreamer.StreamerType.Signal);
        services.TryAddKeyedTransient<SseStreamer, SseEnumerableStreamer>(SseStreamer.StreamerType.Enumerable);
        services.TryAddKeyedTransient<SseStreamer, SseChannelStreamer>(SseStreamer.StreamerType.Channel);

        services.TryAddTransient<SseStreamer.StreamerFactory>(sp =>
            key => sp.GetRequiredKeyedService<SseStreamer>(key));

        services.TryAddSingleton(typeof(SseStreamRegistry<>));
        services.TryAddSingleton(typeof(SseSignalRegistry<>));

        RegisterDiscoveredServices(services, ResolveAssemblies(assemblies));

        return services;
    }

    private static void RegisterDiscoveredServices(
        IServiceCollection services,
        IReadOnlyCollection<Assembly> assemblies)
    {
        Type formatterBaseType = typeof(SseEventFormatter<>);
        Type dataProviderInterfaceType = typeof(ISseDataProvider<>);

        List<Type> candidates = assemblies
            .SelectMany(SafeGetTypes)
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Distinct()
            .ToList();

        foreach (Type implementationType in candidates)
        {
            Type? formatterServiceType = GetClosedGenericBase(implementationType, formatterBaseType);
            if (formatterServiceType is not null)
            {
                services.TryAddSingleton(implementationType);
                services.TryAddSingleton(formatterServiceType, sp =>
                    sp.GetRequiredService(implementationType));
            }

            Type[] providerServiceTypes = implementationType
                .GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == dataProviderInterfaceType)
                .ToArray();

            if (providerServiceTypes.Length > 0)
            {
                services.TryAddSingleton(implementationType);

                foreach (Type providerServiceType in providerServiceTypes)
                    services.TryAddSingleton(providerServiceType, sp =>
                        sp.GetRequiredService(implementationType));
            }
        }
    }

    private static Assembly[] ResolveAssemblies(params Assembly[] assemblies)
    {
        if (assemblies is { Length: > 0 })
            return assemblies.Distinct().ToArray();

        Assembly? entryAssembly = Assembly.GetEntryAssembly();
        Assembly callingAssembly = Assembly.GetCallingAssembly();

        return new[]
            {
                entryAssembly,
                callingAssembly,
                typeof(XcLibSseAssemblyMarker).Assembly
            }
            .Where(a => a is not null)
            .Distinct()
            .ToArray()!;
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }

    private static Type? GetClosedGenericBase(Type type, Type openGenericBase)
    {
        for (Type? current = type.BaseType;
             current is not null && current != typeof(object);
             current = current.BaseType)
            if (current.IsGenericType && current.GetGenericTypeDefinition() == openGenericBase)
                return current;

        return null;
    }
}