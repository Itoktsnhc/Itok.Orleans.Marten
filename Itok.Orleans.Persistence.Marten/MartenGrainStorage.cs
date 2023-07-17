using System.Dynamic;
using System.Reflection;
using Mapster;
using MapsterMapper;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;

namespace Itok.Orleans.Persistence.Marten;

public class MartenGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private readonly string _storageName;
    private readonly MartenPersistenceOption _persistenceOption;
    private readonly IOptions<ClusterOptions> _clusterOptions;
    private readonly IGrainFactory _grainFactory;
    private readonly ITypeResolver _typeResolver;
    private Lazy<IDocumentStore> _store = null;
    private readonly IMapper _mapper = new Mapper();

    public MartenGrainStorage(string storageName, MartenPersistenceOption persistenceOption,
        IOptions<ClusterOptions> clusterOptions,
        IGrainFactory grainFactory, ITypeResolver typeResolver)
    {
        _storageName = storageName;
        _persistenceOption = persistenceOption;
        _clusterOptions = clusterOptions;
        _grainFactory = grainFactory;
        _typeResolver = typeResolver;
    }

    public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
        await using var session = _store.Value.QuerySession();
        var id = grainReference.GrainIdentity.IdentityString;
        var container = await session.LoadAsync<MartenStoreContainer>(id);
        grainState.State = container != null
            ? _mapper.Map(container.Data, grainState.State, container.Data.GetType(), grainState.State.GetType())
            : Activator.CreateInstance(grainState.State.GetType());
        grainState.ETag = container?.Version.ToString("N") ?? Guid.NewGuid().ToString("N");
        grainState.RecordExists = container != null;
    }

    public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
        await using var session = _store.Value.LightweightSession();
        var id = grainReference.GrainIdentity.IdentityString;
        var entity = new MartenStoreContainer()
        {
            Data = grainState,
            GrainType = grainType,
            GrainRefId = id,
            Version = Guid.ParseExact(grainState.ETag, "N")
        };
        session.Store(entity);
        await session.SaveChangesAsync();
        grainState.ETag = entity.Version.ToString("N");
    }

    public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
        await using var session = _store.Value.LightweightSession();
        var id = grainReference.GrainIdentity.IdentityString;
        session.Delete<MartenStoreContainer>(id);
        await session.SaveChangesAsync();
    }

    public void Participate(ISiloLifecycle lifecycle)
    {
        var name = OptionFormattingUtilities.Name<MartenGrainStorage>(_storageName);
        lifecycle.Subscribe(name, ServiceLifecycleStage.ApplicationServices, Init);
    }

    private Task Init(CancellationToken ct)
    {
        _store = new Lazy<IDocumentStore>(() => new DocumentStore(_persistenceOption.StoreOptions));
        return Task.CompletedTask;
    }

    public static IGrainStorage Create(IServiceProvider sp, string name)
    {
        using var scope = sp.CreateScope();
        sp = scope.ServiceProvider;
        var options = sp.GetRequiredService<IOptionsSnapshot<MartenPersistenceOption>>();
        var clusterOptions = sp.GetRequiredService<IOptions<ClusterOptions>>();
        var grainFactory = sp.GetRequiredService<IGrainFactory>();
        var typeResolver = sp.GetRequiredService<ITypeResolver>();
        return ActivatorUtilities.CreateInstance<MartenGrainStorage>(sp, name, options.Get(name), clusterOptions,
            grainFactory, typeResolver);
    }
}