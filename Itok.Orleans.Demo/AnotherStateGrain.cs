using Orleans;
using Orleans.Runtime;

namespace Itok.Orleans.Demo;

public class AnoLastState
{
    public string RawState { get; set; }
    public string AnoRawState { get; set; }
}

public interface IAnoStateGrain : IGrain, IGrainWithIntegerKey
{
    Task<string> SaveStateAsync(string state);
    Task<string> ReadStateAsync();
}

public class AnoStateGrain : Grain, IAnoStateGrain
{
    private readonly IPersistentState<AnoLastState> _state;

    public AnoStateGrain([PersistentState(nameof(AnoStateGrain), "marten")] IPersistentState<AnoLastState> state)
    {
        _state = state;
    }

    public async Task<string> SaveStateAsync(string state)
    {
        _state.State ??= new AnoLastState();
        _state.State.RawState = state;
        _state.State.RawState = state + "__SUFFIX";
        await _state.WriteStateAsync();
        return await Task.FromResult(state);
    }

    public async Task<string> ReadStateAsync()
    {
        await _state.ReadStateAsync();
        return await Task.FromResult(_state?.State?.RawState + "__MID__" + _state?.State?.AnoRawState);
    }
}