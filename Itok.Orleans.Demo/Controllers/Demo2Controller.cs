using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace Itok.Orleans.Demo.Controllers;


[ApiController]
[Route("[controller]")]
public class Demo2Controller : ControllerBase
{
    private readonly ILogger<Demo2Controller> _logger;
    private readonly IClusterClient _client;

    public Demo2Controller(ILogger<Demo2Controller> logger, IClusterClient client)
    {
        _logger = logger;
        _client = client;
    }

    [HttpPost("state")]
    public async Task<Dto> SaveStateAsync([FromBody] string state)
    {
        var stateGrain = _client.GetGrain<IAnoStateGrain>(111);
        await stateGrain.SaveStateAsync(state);
        return new Dto()
        {
            State = state
        };
    }

    [HttpGet("state")]
    public async Task<Dto> ReadStateAsync()
    {
        var stateGrain = _client.GetGrain<IAnoStateGrain>(111);
        return new Dto()
        {
            State = await stateGrain.ReadStateAsync()
        };
    }
}