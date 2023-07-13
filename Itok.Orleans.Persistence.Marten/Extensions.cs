using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;

namespace Itok.Orleans.Persistence.Marten;

public static class Extensions
{
    public static ISiloBuilder AddMartenGrainStorage(
        this ISiloBuilder builder,
        string name,
        Action<MartenPersistenceOption> options = null)
    {
        return builder.ConfigureServices(
            services => services.AddMartenGrainStorage(name, options));
    }

    public static IServiceCollection AddMartenGrainStorage(
        this IServiceCollection services,
        string name,
        Action<MartenPersistenceOption> options)
    {
        services.AddOptions<MartenPersistenceOption>(name).Configure(options);

        return services.AddSingletonNamedService(name, MartenGrainStorage.Create)
            .AddSingletonNamedService(
                name,
                (s, n) => (ILifecycleParticipant<ISiloLifecycle>) s.GetRequiredServiceByName<IGrainStorage>(n));
    }
}