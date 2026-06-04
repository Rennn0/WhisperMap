using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XcLib.Shared.Dataflow;
using XcLib.Shared.Dataflow.Interfaces;
using XcLib.Shared.Payment;
using XcLib.Shared.Payment.FlittImpl;
using XcLib.Shared.Payment.Interfaces;
using XcLib.Shared.Reactive;
using XcLib.Shared.Reactive.Interfaces;

namespace XcLib.Shared;

public static class ServiceExtensions
{
    public static IServiceCollection AddDataflowNodeFactory<T>(this IServiceCollection serviceCollection)
        where T : class
    {
        serviceCollection.AddTransient<IDataflowNodeFactory<T>, DefaultDataflowNodeFactory<T>>();
        return serviceCollection;
    }

    public static IServiceCollection AddDataflowMesh<T>(this IServiceCollection serviceCollection)
        where T : class
    {
        serviceCollection.AddActivatedSingleton<T>();
        return serviceCollection;
    }

    public static IServiceCollection AddReactiveBus<TMessage>(this IServiceCollection serviceCollection)
        where TMessage : class
    {
        serviceCollection.TryAddActivatedSingleton<IReactiveBus<TMessage>, DefaultReactiveBus<TMessage>>();
        return serviceCollection;
    }

    public static IServiceCollection AddReactiveBus<TMessage, TImplementation>(
        this IServiceCollection serviceCollection)
        where TMessage : class
        where TImplementation : class, IReactiveBus<TMessage>
    {
        serviceCollection.TryAddActivatedSingleton<IReactiveBus<TMessage>, TImplementation>();
        return serviceCollection;
    }

    public static IHostApplicationBuilder AddPayments(this IHostApplicationBuilder hostApplicationBuilder)
    {
        hostApplicationBuilder.Services.Configure<PaymentConfiguration>(
            hostApplicationBuilder.Configuration.GetSection(nameof(PaymentConfiguration)));
        hostApplicationBuilder.Services.AddHttpClient();

        hostApplicationBuilder.Services.AddTransient<ISignatureProvider, FlittSignatureProvider>();

        hostApplicationBuilder.Services.AddTransient<IPaymentProvider, FlittPaymentProvider>();

        return hostApplicationBuilder;
    }
}