using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using XcLib.Sse.Configuration;
using XcLib.Sse.Core.Signal;
using XcLib.Sse.Core.Stream;
using XcLib.Sse.Core.Streamer;
using XcLib.Sse.DataProvider;
using XcLib.Sse.Formatters;
using XcLib.Sse.Options;

namespace XcLib.Sse;

public static class ServiceExtensions
{
    public static IServiceCollection AddSseService(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptionsWithValidateOnStart<SseOptions>()
            .BindConfiguration(nameof(SseOptions))
            .ValidateDataAnnotations();
        services.AddOptionsWithValidateOnStart<SseSignalOptions>()
            .BindConfiguration(nameof(SseSignalOptions))
            .ValidateDataAnnotations();
        services.AddOptionsWithValidateOnStart<SseStreamOptions>()
            .BindConfiguration(nameof(SseStreamOptions))
            .ValidateDataAnnotations();

        return services.AddSseCore(assemblies);
    }
    public static IServiceCollection AddSseService(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<SseOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SseSignalOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SseStreamOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services.AddSseCore(assemblies);
    }

    public static IServiceCollection AddSseService(
        this IServiceCollection services,
        string sectionName,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        services.AddOptionsWithValidateOnStart<SseOptions>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations();
        
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

        services.TryAddKeyedTransient(typeof(SseStreamer<>), StreamerType.Signal, typeof(SseSignalStreamer<>));
        services.TryAddKeyedTransient(typeof(SseStreamer<>), StreamerType.Enumerable, typeof(SseEnumerableStreamer<>));
        services.TryAddKeyedTransient(typeof(SseStreamer<>), StreamerType.Channel, typeof(SseChannelStreamer<>));

        services.TryAddSingleton(typeof(SseStreamRegistry<>));
        services.TryAddSingleton(typeof(SseSignalRegistry<>));

        RegisterDiscoveredServices(services, ResolveAssemblies(assemblies));

        //#NOTE default implementations in case client assembly dont include features
        services.TryAddScoped<IConfigurationTrigger>(_ => ConfigurationExtensions.Trigger);
        services.TryAddSingleton(typeof(SseEventFormatter<>), typeof(DefaultEventFormatter<>));
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        
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