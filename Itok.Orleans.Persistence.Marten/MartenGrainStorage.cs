using System.Dynamic;
using System.Reflection;
using Mapster;
using MapsterMapper;
using Marten;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;

namespace Itok.Orleans.Persistence.Marten;

public class MartenGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private readonly string _storageName;
    private readonly StoreOptions _storeOptions;
    private readonly IOptions<ClusterOptions> _clusterOptions;
    private readonly IGrainFactory _grainFactory;
    private readonly ITypeResolver _typeResolver;
    private Lazy<IDocumentStore> _store = null;
    private readonly IMapper _mapper = new Mapper();

    public MartenGrainStorage(string storageName, StoreOptions storeOptions, IOptions<ClusterOptions> clusterOptions,
        IGrainFactory grainFactory, ITypeResolver typeResolver)
    {
        _storageName = storageName;
        _storeOptions = storeOptions;
        _clusterOptions = clusterOptions;
        _grainFactory = grainFactory;
        _typeResolver = typeResolver;
    }

    public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
        await using var session = _store.Value.QuerySession();
        var id = grainReference.GrainIdentity.IdentityString;
        var container = await session.LoadAsync<MartenContainer>(id);
        grainState.State = container != null
            ? _mapper.Map(container.Data, _typeResolver.ResolveType(grainType))
            : Activator.CreateInstance(grainState.State.GetType());
        grainState.ETag = container?.ETag;
        grainState.RecordExists = container != null;
    }

    public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
        throw new NotImplementedException();
    }

    public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
    {
        throw new NotImplementedException();
    }

    public void Participate(ISiloLifecycle lifecycle)
    {
        lifecycle.Subscribe(
            OptionFormattingUtilities.Name<MartenGrainStorage>(_storageName),
            ServiceLifecycleStage.ApplicationServices,
            Init);
    }

    private Task Init(CancellationToken ct)
    {
        _store = new Lazy<IDocumentStore>(() => new DocumentStore(_storeOptions));
        return Task.CompletedTask;
    }
}