using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace Itok.Orleans.Demo.Controllers;

public class Dto
{
    public string State { get; set; }
}

[ApiController]
[Route("[controller]")]
public class DemoController : ControllerBase
{
    private readonly ILogger<DemoController> _logger;
    private readonly IClusterClient _client;

    public DemoController(ILogger<DemoController> logger, IClusterClient client)
    {
        _logger = logger;
        _client = client;
    }

    [HttpPost("state")]
    public async Task<Dto> SaveStateAsync([FromBody] string state)
    {
        var stateGrain = _client.GetGrain<IStateGrain>(111);
        await stateGrain.SaveStateAsync(state);
        return new Dto()
        {
            State = state
        };
    }

    [HttpGet("state")]
    public async Task<Dto> ReadStateAsync()
    {
        var stateGrain = _client.GetGrain<IStateGrain>(111);
        return new Dto()
        {
            State = await stateGrain.ReadStateAsync()
        };
    }
}