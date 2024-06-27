using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamAppApi.Contracts.Commands;
using StreamAppApi.Contracts.Commands.MovieCommands;
using StreamAppApi.Contracts.Interfaces;

namespace StreamAppApi.App.Controllers;

[Route("api/movies")]
[ApiController]
public class MovieController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MovieController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    // GET: api/movies/
    [HttpGet]
    public async Task<IActionResult> GetAllMovies()
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var movies = await _movieService.GetAllMoviesAsync(userId, cancellationToken);

            return Ok(movies);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/movies/by-slug/:slug
    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetMovieBySlug(string slug)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var movie = await _movieService.GetMovieBySlug(userId, slug, cancellationToken);

            if (movie == null)
            {
                return NotFound();
            }

            return Ok(movie);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/movies/by-person/:personId
    [HttpGet("by-persons/")]
    public async Task<IActionResult> GetMovieByActor([FromBody] MovieByPersonsCommand movieByPersonsCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var movie = await _movieService.GetMovieByPersons(userId, movieByPersonsCommand.personIds, cancellationToken);

            if (movie == null)
                return NotFound();

            return Ok(movie);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/movies/by-genres
    [HttpGet("by-genres")]
    public async Task<IActionResult> GetMovieByGenres([FromBody] MovieByGenresCommand movieByGenresCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var movies = await _movieService.GetMovieByGenres(userId, movieByGenresCommand.genreIds, cancellationToken);

            return Ok(movies);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/movies/most-popular
    [HttpGet("most-popular")]
    public async Task<IActionResult> GetMostPopularMovies()
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var movies = await _movieService.GetMostPopularMovieAsync(userId, cancellationToken);

            return Ok(movies);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/movies/update-count-opened
    [HttpPut("update-count-opened")]
    public async Task<IActionResult> UpdateCountOpened([FromBody] MovieUpdateCountCommand movieUpdateCountCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            await _movieService.UpdateCountOpenedAsync(movieUpdateCountCommand.slug, cancellationToken);

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: api/movies/buy/:movieId
    [Authorize]
    [HttpPost("buy/{movieId}")]
    public async Task<IActionResult> BuyMovieById(string movieId)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var order = await _movieService.BuyMovieById(userId, movieId, cancellationToken);

            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /* Admin Rights */

    // POST: api/movies
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post([FromBody] MovieCreateCommand movieCreateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var createdMovie = await _movieService.CreateMovie(movieCreateCommand, cancellationToken);

            return Ok(createdMovie);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    // POST: api/movies/package-movies
    [HttpPost("package-movies/")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Post(IFormFileCollection files)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;
         
        try
        {
            var result = await _movieService.BatchCreateMovies(files, cancellationToken);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/movies/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetMovieById(string id)
    {
        try
        {
            var cancellationToken = HttpContext?.RequestAborted ?? default;
            var movie = await _movieService.GetMovieById(id, cancellationToken);

            if (movie == null)
            {
                return NotFound();
            }

            return Ok(movie);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/movies/:id
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutMovieById(string id, [FromBody] MovieUpdateCommand movieUpdateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var updatedMovie = await _movieService.UpdateMovie(id, movieUpdateCommand, cancellationToken);

            return Ok(updatedMovie);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/movies/{id} 
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            await _movieService.DeleteMovie(id, cancellationToken);

            return Ok($"Movie with id \"{id}\" has been deleted");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}