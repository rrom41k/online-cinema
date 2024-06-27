using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Commands.GenreCommands;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Interfaces;
using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll;

public class GenreService : IGenreService
{
    private readonly StreamPlatformDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public GenreService(StreamPlatformDbContext dbContext, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public async Task<GenreDto> GetGenreBySlug(string slug, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingGenre = await _dbContext.Genres.AsNoTracking()
                .FirstOrDefaultAsync(existingGenre => existingGenre.Slug == slug, cancellationToken)
            ?? throw new ArgumentException("Genre not found.");

        return GenreToDto(existingGenre);
    }

    public async Task<List<GenreDto>> GetAllGenres(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        string? searchTerm = _httpContextAccessor.HttpContext.Request.Query["searchTerm"];

        if (searchTerm == null)
        {
            searchTerm = "";
        }

        var genres = await _dbContext.Genres.AsNoTracking()
            .Where(
                genre =>
                    genre.Name.ToLower().Contains(searchTerm.ToLower())
                    || genre.Slug.Contains(searchTerm.ToLower())
                    || genre.Description.ToLower().Contains(searchTerm.ToLower()))
            .ToListAsync(cancellationToken);

        return MapGenresToDto(genres);
    }

    public async Task<List<CollectionDto>> GetCollections(CancellationToken cancellationToken)
    {
        var genres = await _dbContext.Genres.AsNoTracking().ToListAsync(cancellationToken);
        var collection = new List<CollectionDto>();

        foreach (var genre in genres)
        {
            var videoGenres = await _dbContext.VideoGenres.AsNoTracking()
                .Include(videoGenres => videoGenres.Video)
                    .ThenInclude(video => video.Movie)
                .Include(videoGenre => videoGenre.Video)
                    .ThenInclude(video => video.Season)
                        .ThenInclude(season => season.Serial)
                .Where(movie => movie.GenreId == genre.GenreId).ToListAsync(cancellationToken);

            if (videoGenres == null)
            {
                continue;
            }
            
            List<CollectionDto> result = new();
            foreach (var videoGenre in videoGenres)
            {
                if (videoGenre.Video != null)
                {
                    switch (videoGenre.Video.Type)
                    {
                        case TypeVideo.Movie:
                            collection.Add(new CollectionDto(
                                videoGenre.VideoId,
                                genre.GenreId,
                                VideoService.DecryptStringFromBytes_Aes(videoGenre.Video.Movie.Poster, 
                                    Convert.FromBase64String(_configuration["AppSettings:CryptKey"]), videoGenre.Video.Movie.PosterIv),
                                genre.Name,
                                genre.Slug));
                            break;
                        case TypeVideo.Serial:
                            collection.Add(new CollectionDto(
                                genre.GenreId,
                                videoGenre.VideoId,
                                VideoService.DecryptStringFromBytes_Aes(videoGenre.Video.Season.Serial.Poster,
                                    Convert.FromBase64String(_configuration["AppSettings:CryptKey"]),
                                    videoGenre.Video.Season.Serial.PosterIv),
                                genre.Name,
                                genre.Slug));
                            break;
                        default:
                            throw new Exception($"Undefined type of video with id: \"{videoGenre.VideoId}\"");
                    }
                }
            }
        }
        return collection;
    }

    /* Admin Rights */

    public async Task<GenreDto> CreateGenre(GenreCreateCommand genreCreateCommand, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
        {
            throw new OperationCanceledException();
        }

        var findGenre =
            await _dbContext.Genres.FirstOrDefaultAsync(genre => genre.Slug == genreCreateCommand.slug.ToLower());

        if (findGenre != null)
        {
            throw new("Genre with this slug contains in DB");
        }

        Genre newGenre = new(
            genreCreateCommand.name,
            genreCreateCommand.slug,
            genreCreateCommand.description,
            genreCreateCommand.icon);

        _dbContext.Genres.Add(newGenre);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return GenreToDto(newGenre);
    }

    public async Task<GenreDto> GetGenreById(string id, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingGenre = await _dbContext.Genres.AsNoTracking()
                .FirstOrDefaultAsync(existingGenre => existingGenre.GenreId == id, cancellationToken)
            ?? throw new ArgumentException("Genre not found.");

        return GenreToDto(existingGenre);
    }

    public async Task<GenreDto> UpdateGenre(
        string id,
        GenreUpdateCommand genreUpdateCommand,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var genreToUpdate = await _dbContext.Genres
            .FirstOrDefaultAsync(genreToUpdate => genreToUpdate.GenreId == id, cancellationToken);

        if (genreToUpdate == null)
        {
            throw new ArgumentException("Genre not found.");
        }

        UpdateGenreHelper(ref genreToUpdate, genreUpdateCommand);
        await _dbContext.SaveChangesAsync(cancellationToken);


        return GenreToDto(genreToUpdate);
    }

    public async Task<GenreDto> DeleteGenre(string id, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingGenre = await _dbContext.Genres.AsNoTracking()
            .FirstOrDefaultAsync(existingGenre => existingGenre.GenreId == id, cancellationToken);

        if (existingGenre == null)
        {
            throw new ArgumentException("Genre not fount.");
        }

        _dbContext.Genres.Remove(existingGenre);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return GenreToDto(existingGenre);
    }

    // Helpful methods
    
    public static List<GenreDto> MapGenresToGenresDto(ICollection<SubscribeGenre> genres)
    {
        var listGenres = new List<GenreDto>();

        foreach (var sg in genres)
        {
            GenreDto newGenreDto = new(
                sg.Genre.GenreId,
                sg.Genre.Name,
                sg.Genre.Slug,
                sg.Genre.Description,
                sg.Genre.Icon);
            listGenres.Add(newGenreDto);
        }

        return listGenres;
    }

    private void UpdateGenreHelper(ref Genre genreToUpdate, GenreUpdateCommand genreUpdateCommand)
    {
        genreToUpdate.Name = string.IsNullOrEmpty(genreUpdateCommand.name) ? genreToUpdate.Name : 
            genreUpdateCommand.name;
        
        genreToUpdate.Slug = string.IsNullOrEmpty(genreUpdateCommand.slug.ToLower()) ? genreToUpdate.Slug : 
            genreUpdateCommand.slug.ToLower();
        
        genreToUpdate.Description = string.IsNullOrEmpty(genreUpdateCommand.description)? genreToUpdate.Description : 
            genreUpdateCommand.description;
        
        genreToUpdate.Icon = string.IsNullOrEmpty(genreUpdateCommand.icon)? genreToUpdate.Icon : genreUpdateCommand.icon;
    }

    public static GenreDto GenreToDto(Genre genre)
    {
        return new(
            genre.GenreId,
            genre.Name,
            genre.Slug,
            genre.Description,
            genre.Icon);
    }

    private List<GenreDto> MapGenresToDto(List<Genre> genres)
    {
        List<GenreDto> genresListDto = new();

        foreach (var genre in genres)
        {
            genresListDto.Add(GenreToDto(genre));
        }

        return genresListDto;
    }

    public static List<GenreDto> MapGenresToDto(ICollection<VideoGenre> genres)
    {
        List<GenreDto> genresListDto = new();

        foreach (var genre in genres)
        {
            var genreDto = GenreToDto(genre.Genre);
            genresListDto.Add(genreDto);
        }

        return genresListDto;
    }

    public static List<VideoGenre> MapGenresArrToList(Video video, string[] genres)
    {
        var listGenres = new List<VideoGenre>();

        foreach (var genreId in genres)
        {
            var newGenreMovie = new VideoGenre
            {
                Video = video,
                VideoId = video.VideoId,
                GenreId = genreId
            };
            listGenres.Add(newGenreMovie);
        }

        return listGenres;
    }

    public static List<SubscribeGenre> MapGenresArrToList(Subscribe subscribe, string[] genres)
    {
        var listGenres = new List<SubscribeGenre>();

        foreach (var genreId in genres)
        {
            var newSubscribeGenre = new SubscribeGenre
            {
                Subscribe = subscribe,
                SubscribeId = subscribe.SubscribeId,
                GenreId = genreId
            };
            listGenres.Add(newSubscribeGenre);
        }

        return listGenres;
    }
}