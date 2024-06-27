using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamAppApi.Contracts.Commands;
using StreamAppApi.Contracts.Commands.SerialCommands;
using StreamAppApi.Contracts.Interfaces;

namespace StreamAppApi.App.Controllers;

[Route("api/serials")]
[ApiController]
public class SerialController : ControllerBase
{
    private readonly ISerialService _serialService;

    public SerialController(ISerialService serialService)
    {
        _serialService = serialService;
    }

    // GET: api/serials/
    [HttpGet]
    public async Task<IActionResult> GetAllSerials()
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var serials = await _serialService.GetAllSerials(userId, cancellationToken);

            return Ok(serials);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/serials/by-slug/:slug
    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetSerialBySlug(string slug)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var serial = await _serialService.GetSerialBySlug(userId, slug, cancellationToken);

            if (serial == null)
            {
                return NotFound();
            }

            return Ok(serial);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/serials/by-person/:personId
    [HttpGet("by-persons/")]
    public async Task<IActionResult> GetSerialByPerson([FromBody] List<string> personIds)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var serial = await _serialService.GetSerialByPersons(userId, personIds, cancellationToken);

            if (serial == null)
                return NotFound();

            return Ok(serial);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/serials/by-genres
    [HttpGet("by-genres")]
    public async Task<IActionResult> GetSerialByGenres([FromBody] List<string> genreIds)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var serials = await _serialService.GetSerialByGenres(userId, genreIds, cancellationToken);

            return Ok(serials);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/serials/most-popular
    [HttpGet("most-popular")]
    public async Task<IActionResult> GetMostPopularSerials()
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var serials = await _serialService.GetMostPopularSerialAsync(userId, cancellationToken);

            return Ok(serials);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/serials/update-count-opened
    [HttpPut("update-count-opened")]
    public async Task<IActionResult> UpdateCountOpened([FromBody] string serialId)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            await _serialService.UpdateCountOpenedAsync(serialId, cancellationToken);

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: api/serials/buy/:serialId
    [Authorize]
    [HttpPost("buy/{serialId}")]
    public async Task<IActionResult> BuySerialById(string serialId)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var order = await _serialService.BuySerialById(userId, serialId, cancellationToken);

            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    // GET: api/serials/by-slug/:slug/:numberSeason
    [HttpGet("by-slug/{slug}/{numberSeason}")]
    public async Task<IActionResult> GetSeasonBySlug(string slug, int numberSeason)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;
        
        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var serial = await _serialService.GetSeasonBySlug(userId, slug, numberSeason, cancellationToken);
            
            if (serial == null)
            {
                return NotFound();
            }
            
            return Ok(serial);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    // GET: api/serials/:slug/:numberSeason/:episodeId
    [HttpGet("by-slug/{slug}/{numberSeason}/{episodeId}")]
    public async Task<IActionResult> GetEpisodeById(string slug, int numberSeason, string episodeId)
    {
        try
        {
            var cancellationToken = HttpContext?.RequestAborted ?? default;
            var userId = User.FindFirst("_id")?.Value;
            var serial = await _serialService.GetEpisodeById(userId, slug,
            numberSeason, episodeId, cancellationToken);
            
            if (serial == null)
            {
                return NotFound();
            }
            
            return Ok(serial);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /* Admin Rights */

    // POST: api/serials
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PostSerial([FromBody] SerialCreateCommand serialCreateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var createdSerial = await _serialService.CreateSerial(serialCreateCommand, cancellationToken);

            return Ok(createdSerial);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    // POST: api/serials/package-serials
    [HttpPost("package-serials/")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> PackagePostSerials(IFormFileCollection files)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;
         
        try
        {
            var result = await _serialService.BatchCreateSerials(files, cancellationToken);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/serials/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetSerialById(string id)
    {
        try
        {
            var cancellationToken = HttpContext?.RequestAborted ?? default;
            var serial = await _serialService.GetSerialById(id, cancellationToken);

            if (serial == null)
            {
                return NotFound();
            }

            return Ok(serial);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/serials/:id
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutSerialById(string id, [FromBody] SerialUpdateCommand serialUpdateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var updatedSerial = await _serialService.UpdateSerial(id, serialUpdateCommand, cancellationToken);

            return Ok(updatedSerial);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/serials/{id} 
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSerial(string id)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            await _serialService.DeleteSerial(id, cancellationToken);

            return Ok($"Serial with id \"{id}\" has been deleted");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: api/serials/seasons
    [HttpPost("seasons/")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PostSeason([FromBody] OnlySeasonCreateCommand seasonCreateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var createdSerial = await _serialService.CreateSeason(seasonCreateCommand, cancellationToken);

            return Ok(createdSerial);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    // GET: api/serials/seasons/{id}
    [HttpGet("seasons/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetSeasonById(string id)
    {
        try
        {
            var cancellationToken = HttpContext?.RequestAborted ?? default;
            var serial = await _serialService.GetSeasonById(id, cancellationToken);

            if (serial == null)
            {
                return NotFound();
            }

            return Ok(serial);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/serials/seasons/:id
    [HttpPut("seasons/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutSeasonById(string id, [FromBody] SeasonUpdateCommand serialUpdateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var updatedSerial = await _serialService.UpdateSeason(id, serialUpdateCommand, cancellationToken);

            return Ok(updatedSerial);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/serials/seasons/{id} 
    [HttpDelete("seasons/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSeason(string id)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            await _serialService.DeleteSeason(id, cancellationToken);

            return Ok($"Season with id \"{id}\" has been deleted");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: api/serials/episodes
    [HttpPost("episodes/")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PostEpisode([FromBody] OnlyEpisodeCreateCommand episodeCreateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var createdSerial = await _serialService.CreateEpisode(episodeCreateCommand, cancellationToken);

            return Ok(createdSerial);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/serials/episodes/:id
    [HttpPut("episodes/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutEpisodeById(string id, [FromBody] EpisodeUpdateCommand episodeUpdateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var updatedSerial = await _serialService.UpdateEpisode(id, episodeUpdateCommand, cancellationToken);

            return Ok(updatedSerial);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/serials/episodes/{id} 
    [HttpDelete("episodes/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEpisode(string id)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            await _serialService.DeleteEpisode(id, cancellationToken);

            return Ok($"Episode with id \"{id}\" has been deleted");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}