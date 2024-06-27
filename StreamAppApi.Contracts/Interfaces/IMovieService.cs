using Microsoft.AspNetCore.Http;

using StreamAppApi.Contracts.Commands.MovieCommands;
using StreamAppApi.Contracts.Commands;
using StreamAppApi.Contracts.Commands.FileCommands;
using StreamAppApi.Contracts.Dto;

namespace StreamAppApi.Contracts.Interfaces;

public interface IMovieService
{
    Task<MovieDto> GetMovieBySlug(string userId, string slug, CancellationToken cancellationToken);

    Task<List<MovieDto>> GetMovieByPersons(
        string userId,
        List<string> personId,
        CancellationToken cancellationToken);

    Task<List<MovieDto>> GetMovieByGenres(
        string userId,
        List<string> genreIds,
        CancellationToken cancellationToken);

    Task<List<MovieDto>> GetAllMoviesAsync(string userId, CancellationToken cancellationToken);

    Task<List<MovieDto>> GetMostPopularMovieAsync(string userId, CancellationToken cancellationToken);

    Task UpdateCountOpenedAsync(
        string slug,
        CancellationToken cancellationToken);
    
    Task<OrderDto> BuyMovieById(string userId, string movieId, CancellationToken cancellationToken);

    /* Admin Rights */

    Task<MovieDto> CreateMovie(
        MovieCreateCommand movieCreateCommand,
        CancellationToken cancellationToken);
    
    Task<List<MovieDto>> BatchCreateMovies(IFormFileCollection files, CancellationToken cancellationToken);

    Task<MovieDto> GetMovieById(
        string id,
        CancellationToken cancellationToken);

    Task<MovieDto> UpdateMovie(
        string id,
        MovieUpdateCommand movieUpdateCommand,
        CancellationToken cancellationToken);

    Task DeleteMovie(
        string id,
        CancellationToken cancellationToken);
}