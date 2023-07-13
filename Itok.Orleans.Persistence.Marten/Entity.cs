namespace Itok.Orleans.Persistence.Marten;

public class MartenContainer
{
    public string GrainRefId { get; set; }
    public dynamic Data { get; set; }
    public string ETag { get; set; }
    public string GrainType { get; set; }
}