using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StreamAppApi.Contracts.Commands.RatingCommands;
using StreamAppApi.Contracts.Interfaces;

namespace StreamAppApi.App.Controllers;

[Route("api/ratings")]
[ApiController]
[Authorize]
public class RatingController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    // POST: api/ratings/set-rating
    [HttpPost("set-rating")]
    public async Task<IActionResult> Post([FromBody] SetRatingCommand setRatingCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var result = await _ratingService.SetRating(userId, setRatingCommand, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/ratings/:movieId
    [HttpGet("{videoId}")]
    public async Task<IActionResult> Get(string videoId)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var result = await _ratingService.GetVideoRatingByUser(userId, videoId, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}