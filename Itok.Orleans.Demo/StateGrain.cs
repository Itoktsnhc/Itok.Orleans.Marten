using Orleans;
using Orleans.Runtime;

namespace Itok.Orleans.Demo;

public class LastState
{
    public string RawState { get; set; }
}

public interface IStateGrain : IGrain, IGrainWithIntegerKey
{
    Task<string> SaveStateAsync(string state);
    Task<string> ReadStateAsync();
}

public class StateGrain : Grain, IStateGrain
{
    private readonly IPersistentState<LastState> _state;

    public StateGrain([PersistentState("last_state", "marten")] IPersistentState<LastState> state)
    {
        _state = state;
    }

    public async Task<string> SaveStateAsync(string state)
    {
        _state.State ??= new LastState();
        _state.State.RawState = state;
        await _state.WriteStateAsync();
        return await Task.FromResult(state);
    }

    public async Task<string> ReadStateAsync()
    {
        await _state.ReadStateAsync();
        return await Task.FromResult(_state?.State?.RawState);
    }
}