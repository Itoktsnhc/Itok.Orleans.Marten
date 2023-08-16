using Orleans;
using Orleans.Runtime;

namespace Itok.Orleans.Demo;


public class Person
{
    public long Id { get; set; }
    public string IdentityStr { get; set; }
    public string Name { get; set; }

}

public interface IWithStateGrain : IGrain, IGrainWithIntegerKey
{
    Task<Person> InitialPersonAsync();
    Task<Person> ChangePersonNameAsync(string personName);
    Task<Person> GetCurrentPersonAsync();
}

public class WithStateGrain : Grain, IWithStateGrain
{
    private readonly IPersistentState<Person> _state;

    public WithStateGrain([PersistentState(nameof(WithStateGrain), "marten")] IPersistentState<Person> state)
    {
        _state = state;
    }

    public async Task<Person> ChangePersonNameAsync(string personName)
    {
        if (!_state.RecordExists) throw new InvalidOperationException("Current grain not initialized");
        _state.State.Name = personName;
        await _state.WriteStateAsync();
        return _state.State;
    }

    public async Task<Person> GetCurrentPersonAsync()
    {
        await _state.ReadStateAsync();
        return _state.State;
    }

    public async Task<Person> InitialPersonAsync()
    {
        if (_state.RecordExists) return _state.State;
        _state.State = new Person
        {
            Id = this.GetGrainIdentity().PrimaryKeyLong,
            IdentityStr = IdentityString,
            Name = "INITIAL_NAME",
        };
        await _state.WriteStateAsync();
        return _state.State;
    }
}
