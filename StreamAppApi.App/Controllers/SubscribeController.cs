using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StreamAppApi.Contracts.Commands.GenreCommands;
using StreamAppApi.Contracts.Commands.SubscribeCommands;
using StreamAppApi.Contracts.Interfaces;

namespace StreamAppApi.App.Controllers;

[Route("api/subscribes")]
[ApiController]
public class SubscribeController : ControllerBase
{
    private readonly ISubscribeService _subscribeService;

    public SubscribeController(ISubscribeService subscribeService)
    {
        _subscribeService = subscribeService;
    }

    // GET: api/subscribes/:id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubscribeById(string id)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var subscribe = await _subscribeService.GetSubscribeById(id, cancellationToken);

            if (subscribe == null)
            {
                return NotFound();
            }

            return Ok(subscribe);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/subscribes
    [HttpGet]
    public async Task<IActionResult> GetSubscribes()
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var collections = await _subscribeService.GetAllSubscribes(cancellationToken);

            return Ok(collections);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    // POST: api/movies/buy/:subscribeId
    [Authorize]
    [HttpPost("buy/{subscribeId}")]
    public async Task<IActionResult> BuySubscribeById(string subscribeId)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;
        
        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var order = await _subscribeService.BuySubscribeById(userId, subscribeId, cancellationToken);
            
            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /* Admin Rights */

    // POST: api/subscribes
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post([FromBody] SubscribeCreateCommand subscribeCreateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var createdGenre = await _subscribeService.CreateSubscribe(subscribeCreateCommand, cancellationToken);

            return Ok(createdGenre);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/subscribes/:id
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutGenreById(string id, [FromBody] SubscribeUpdateCommand subscribeUpdateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var updatedGenre = await _subscribeService.UpdateSubscribe(id, subscribeUpdateCommand, cancellationToken);

            return Ok(updatedGenre);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/subscribes/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            await _subscribeService.DeleteSubscribe(id, cancellationToken);

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}