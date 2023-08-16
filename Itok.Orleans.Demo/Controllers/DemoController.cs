using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace Itok.Orleans.Demo.Controllers;


[ApiController]
[Route("[controller]")]
public class PersonController : ControllerBase
{
    private readonly ILogger<PersonController> _logger;
    private readonly IClusterClient _client;

    public PersonController(ILogger<PersonController> logger, IClusterClient client)
    {
        _logger = logger;
        _client = client;
    }

    [HttpPost("{id:long}/name")]
    public async Task<Person> SetNewPersonNameAsync([FromRoute] long id, [FromBody] string newPersonName)
    {
        var grain = _client.GetGrain<IWithStateGrain>(id);
        await grain.InitialPersonAsync();
        return await grain.ChangePersonNameAsync(newPersonName);
    }

    [HttpGet("{id:long}")]
    public async Task<Person> GetPersonAsync([FromRoute] long id)
    {
        var grain = _client.GetGrain<IWithStateGrain>(id);
        return await grain.GetCurrentPersonAsync();
    }
}