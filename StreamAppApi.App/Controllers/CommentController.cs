using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StreamAppApi.Contracts.Commands.CommentCommands;
using StreamAppApi.Contracts.Interfaces;

namespace StreamAppApi.App.Controllers;

[Route("api/comments")]
[ApiController]
[Authorize]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    // POST: api/comments/set-comment
    [HttpPost("set-comment")]
    public async Task<IActionResult> Post([FromBody] SetCommentCommand setCommentCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var result = await _commentService.SetComment(userId, setCommentCommand, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/ratings/:videoId
    [HttpGet("{videoId}")]
    public async Task<IActionResult> Get(string videoId)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var result = await _commentService.GetVideoComments(videoId, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}