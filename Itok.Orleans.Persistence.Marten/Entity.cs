using Marten;
using Marten.Metadata;
using Marten.Schema;

namespace Itok.Orleans.Persistence.Marten;

public class MartenPersistenceOption
{
    public StoreOptions StoreOptions { get; set; }
}

[UseOptimisticConcurrency]
public class MartenStoreContainer : IVersioned
{
    [Identity]
    public string GrainRefId { get; set; }

    public dynamic Data { get; set; }
    public string GrainType { get; set; }

    public Guid Version { get; set; }
}