using System.Drawing;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Commands;
using StreamAppApi.Contracts.Commands.MovieCommands;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Interfaces;
using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll;

public class MovieService : IMovieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TelegramBotService _telegramBotService;
    private readonly StreamPlatformDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly VideoService _videoService;
    
    public MovieService(
        IHttpContextAccessor httpContextAccessor,
        TelegramBotOptions telegramBotOptions,
        StreamPlatformDbContext dbContext,
        IConfiguration configuration,
        VideoService videoService)
    {
        _telegramBotService = new(telegramBotOptions);
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _videoService = videoService;
        _dbContext = dbContext;
    }

    public async Task<List<MovieDto>> GetAllMoviesAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        string? searchTerm = _httpContextAccessor.HttpContext.Request.Query["searchTerm"];

        searchTerm ??= string.Empty;

        var movies = await GetAllMoviesNoTracking()
            .Where(movie => movie.Movie.Title.ToLower().Contains(searchTerm.ToLower()) || movie.Movie.Slug.Contains(searchTerm.ToLower()))
            .OrderByDescending(movie => movie.Movie.Rating)
            .ToListAsync(cancellationToken);
        
        var key = Convert.FromBase64String(_configuration["AppSettings:CryptKey"]);
        return movies.Select(movie => MovieToDto(userId, movie, key)).ToList();
    }

    public async Task<MovieDto> GetMovieBySlug(string userId, string slug, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingMovie = await GetAllMoviesNoTracking()
                                    .FirstOrDefaultAsync(existingMovie => existingMovie.Movie.Slug == slug, 
                                        cancellationToken)
                                ?? throw new ArgumentException("Movie not found.");
        
        return MovieToDto(userId, existingMovie, Convert.FromBase64String(_configuration["AppSettings:CryptKey"]));
    }

    public async Task<List<MovieDto>> GetMovieByPersons(
        string userId,
        List<string> personIds,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingMovies = await GetAllMoviesNoTracking()
                .Where(
                    existingMovies => existingMovies.Crew.Any(crew => personIds.Contains(crew.PersonId)))
                .OrderByDescending(movie => movie.Movie.Rating)
                .ToListAsync(cancellationToken)
            ?? throw new ArgumentException("Movie not found.");
        
        var key = Convert.FromBase64String(_configuration["AppSettings:CryptKey"]);
        return existingMovies.Select(movie => MovieToDto(userId, movie, key)).ToList();;
    }

    public async Task<List<MovieDto>> GetMovieByGenres(
        string userId,
        List<string> genreIds,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingMovies = await GetAllMoviesNoTracking()
                    .Where(movie => movie.Genres.Any(genre => genreIds.Contains(genre.GenreId)))
                    .OrderByDescending(movie => movie.Movie.Rating)
                    .ToListAsync(cancellationToken)
            ?? throw new ArgumentException("Movie not found.");
        
        var key = Convert.FromBase64String(_configuration["AppSettings:CryptKey"]);
        return existingMovies.Select(movie => MovieToDto(userId, movie, key)).ToList();;
    }

    public async Task UpdateCountOpenedAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingMovie = await _dbContext.Movies
            .FirstOrDefaultAsync(m => m.Slug == slug, cancellationToken);

        if (existingMovie != null)
        {
            existingMovie.CountOpened++;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            throw new ArgumentException($"Movie with slug \"{slug}\" not found.");
        }
    }

    public async Task<List<MovieDto>> GetMostPopularMovieAsync(string userId, CancellationToken cancellationToken = default)
    {
        var popularMovies = await GetAllMoviesNoTracking()
            .Where(m => m.Movie.CountOpened > 0)
            .OrderByDescending(m => m.Movie.CountOpened)
            .ToListAsync(cancellationToken);
        
        var key = Convert.FromBase64String(_configuration["AppSettings:CryptKey"]);
        return popularMovies.Select(movie => MovieToDto(userId, movie, key)).ToList();;
    }
    
    public async Task<OrderDto> BuyMovieById(string userId, string movieId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var existingMovie = await GetAllMoviesNoTracking()
                .FirstOrDefaultAsync(
                    existingMovie =>
                        existingMovie.VideoId == movieId,
                    cancellationToken)
            ?? throw new ArgumentException("Movie not found.");
        
        Order newOrder = new(
            existingMovie.Movie.Price ?? 0,
            userId,
            null,
            null,
            movieId);
        
        await _dbContext.Orders.AddAsync(newOrder, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return new(newOrder.OrderId, newOrder.OrderDate, newOrder.Sum, newOrder.UserId, null, null, newOrder.MovieId);
    }

    /* Admin Rights */

    public async Task<MovieDto> CreateMovie(MovieCreateCommand movieCreateCommand, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
        {
            throw new OperationCanceledException();
        }
        
        byte[] key = Convert.FromBase64String(_configuration["AppSettings:CryptKey"]);

        var existingMovie = await GetAllMoviesNoTracking()
            .FirstOrDefaultAsync(movie => movie.Movie.Slug == movieCreateCommand.slug.ToLower(), cancellationToken);

        if (existingMovie != null)
        {
            throw new($"Movie with slug \"{movieCreateCommand.slug.ToLower()}\" already exists in the database");
        }

        var newMovie = await CreateMovieHelper(movieCreateCommand, cancellationToken);
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!newMovie.Video.IsSendTelegram)
        {
            try
            {
                await _telegramBotService.SendPostLink($"Скорее смотрите новый фильм {newMovie.Title} в нашем онлайн-кинотеатре!",
                    VideoService.DecryptStringFromBytes_Aes(newMovie.Poster, key, newMovie.PosterIv));
            }
            catch (Exception exception)
            {
                Console.WriteLine("warn: " + exception.Message);
                newMovie.Video.IsSendTelegram = false;
            }
        }
        
        return await GetMovieById(newMovie.Video.VideoId, cancellationToken);
    }
    
    public async Task<List<MovieDto>> BatchCreateMovies(IFormFileCollection files, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        List<MovieDto> result = new();
        
        // Шаг 1: Чтение данных из файлов
        var movieCreateCommands = await ReadMovieCreateCommandsFromFiles(files, cancellationToken);
        
        // Шаг 2: Создание фильмов в пакетном режиме
        foreach (var movieCreateCommand in movieCreateCommands)
        {
            result.Add(await CreateMovie(movieCreateCommand, cancellationToken));
        }
        
        // Сохранение изменений в базе данных
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return result;
    }
    
    public async Task<MovieDto> GetMovieById(string movieId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingMovie = await GetAllMoviesNoTracking()
                .FirstOrDefaultAsync(
                    existingMovie =>
                        existingMovie.VideoId == movieId,
                    cancellationToken)
            ?? throw new ArgumentException("Movie not found.");

        return MovieToDto(existingMovie, Convert.FromBase64String(_configuration["AppSettings:CryptKey"]));
    }

    public async Task<MovieDto> UpdateMovie(string movieId, MovieUpdateCommand movieUpdateCommand, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var movieToUpdate = await GetAllMovies()
            .FirstOrDefaultAsync(movieToUpdate => movieToUpdate.VideoId == movieId, cancellationToken);

        if (movieToUpdate == null)
        {
            throw new ArgumentException($"Movie with Id \"{movieId}\" not found.");
        }

        UpdateMovieHelper(ref movieToUpdate, movieUpdateCommand, cancellationToken);

        return MovieToDto(movieToUpdate, Convert.FromBase64String(_configuration["AppSettings:CryptKey"]));
    }

    public async Task DeleteMovie(string movieId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingMovie = await GetAllMovies()
            .FirstOrDefaultAsync(existingMovie => existingMovie.VideoId == movieId, cancellationToken);

        if (existingMovie == null)
        {
            throw new NullReferenceException("Movie not fount.");
        }

        _dbContext.Movies.Remove(existingMovie.Movie);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // Helpful methods

    private  IQueryable<Video> GetAllMovies()
    {
        return _dbContext.Videos
            // Include comments
            .Include(video => video.Comments)
            .ThenInclude(comment => comment.User)
            // Include rating
            .Include(video => video.Ratings)
            // Include crew and role
            .Include(video => video.Crew)
            .ThenInclude(crew => crew.Role)
            // Include crew and person
            .Include(video => video.Crew)
            .ThenInclude(crew => crew.Person)
            // Include genre
            .Include(video => video.Genres)
            .ThenInclude(videoGenre => videoGenre.Genre)
            // Include counties
            .Include(video => video.Countries)
            .ThenInclude(videoGenre => videoGenre.Country)
            // Include movie
            .Include(video => video.Movie)
            .Where(video => video.Type == TypeVideo.Movie);
    }
    
    private  IQueryable<Video> GetAllMoviesNoTracking()
    {
        return _dbContext.Videos.AsNoTracking()
            // Include comments
            .Include(video => video.Comments)
            .ThenInclude(comment => comment.User)
            // Include rating
            .Include(video => video.Ratings)
            // Include crew and role
            .Include(video => video.Crew)
            .ThenInclude(crew => crew.Role)
            // Include crew and person
            .Include(video => video.Crew)
            .ThenInclude(crew => crew.Person)
            // Include genre
            .Include(video => video.Genres)
            .ThenInclude(videoGenre => videoGenre.Genre)
            // Include counties
            .Include(video => video.Countries)
            .ThenInclude(videoGenre => videoGenre.Country)
            // Include movie
            .Include(video => video.Movie)
            .Where(video => video.Type == TypeVideo.Movie);
    }

    private async Task<Movie> CreateMovieHelper(MovieCreateCommand movieCreateCommand, 
        CancellationToken cancellationToken)
    {
        Movie newMovie = new(
            movieCreateCommand.title,
            movieCreateCommand.slug,
            VideoService.EncryptStringToBytes_Aes(movieCreateCommand.poster,Convert.FromBase64String(_configuration["AppSettings:CryptKey"]), out var posterUrlIv),
            posterUrlIv,
            VideoService.EncryptStringToBytes_Aes(movieCreateCommand.bigPoster,Convert.FromBase64String(_configuration["AppSettings:CryptKey"]), out var bigPosterUrlIv),
            bigPosterUrlIv,
            movieCreateCommand.needSubscribe,
            movieCreateCommand.price);
        await _dbContext.Movies.AddAsync(newMovie, cancellationToken);
        
        Video newVideo = new Video(
            VideoService.EncryptStringToBytes_Aes(movieCreateCommand.videoUrl, Convert.FromBase64String(_configuration["AppSettings:CryptKey"]), out var videoUrlIv),
            videoUrlIv,
            movieCreateCommand.year,
            movieCreateCommand.duration,
            TypeVideo.Movie,
            movieCreateCommand.isSendTelegram ?? false)
        {
            MovieId = newMovie.MovieId
        };

        newMovie.Video = newVideo;
        
        _dbContext.Videos.Add(newVideo);
        
        var genres = GenreService.MapGenresArrToList(newVideo, movieCreateCommand.genres);
        var crew = PersonService.MapPersonsArrToList(newVideo, movieCreateCommand.crew);
        var counties =  CountryService.MapCountiesIdsToVideoCountryList(newVideo, movieCreateCommand.countries);
        
        _dbContext.VideoGenres.AddRange(genres);
        _dbContext.Crews.AddRange(crew);
        _dbContext.VideoCountries.AddRange(counties);
        
        newVideo.Genres = genres;
        newVideo.Crew = crew;
        newVideo.Countries = counties;
        
        return newMovie;
    }
    
    private async Task<List<MovieCreateCommand>> ReadMovieCreateCommandsFromFiles(IFormFileCollection files, CancellationToken cancellationToken)
    {
        List<MovieCreateCommand> movieCreateCommands = new();
        
        foreach (var file in files)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is not provided or empty.");
            }
            
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;
            
            var commands = await JsonSerializer.DeserializeAsync<List<MovieCreateCommand>>(stream, cancellationToken: cancellationToken);
            if (commands != null)
            {
                movieCreateCommands.AddRange(commands);
            }
        }
        
        return movieCreateCommands;
    }
    
    private void UpdateMovieHelper(ref Video movieToUpdate, MovieUpdateCommand movieUpdateCommand, 
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(movieUpdateCommand.poster))
        {
            movieToUpdate.Movie.Poster = VideoService.EncryptStringToBytes_Aes(movieUpdateCommand.poster,
                Convert.FromBase64String(_configuration["AppSettings:CryptKey"]), out var posterIv);
            movieToUpdate.Movie.PosterIv = posterIv;
        }
        
        if (!string.IsNullOrEmpty(movieUpdateCommand.bigPoster))
        {
            movieToUpdate.Movie.BigPoster = VideoService.EncryptStringToBytes_Aes(movieUpdateCommand.bigPoster,
                Convert.FromBase64String(_configuration["AppSettings:CryptKey"]), out var bigPosterIv);
            movieToUpdate.Movie.BigPosterIv = bigPosterIv;
        }
        
        movieToUpdate.Movie.Title = string.IsNullOrEmpty(movieUpdateCommand.title) ? movieToUpdate.Movie.Title 
            : movieUpdateCommand.title;

        if (!string.IsNullOrEmpty(movieUpdateCommand.videoUrl))
        {
            movieToUpdate.VideoUrl = VideoService.EncryptStringToBytes_Aes(movieUpdateCommand.videoUrl,
                Convert.FromBase64String(_configuration["AppSettings:CryptKey"]), out var videoUrlIv);
            movieToUpdate.VideoUrlIv = videoUrlIv;
        }

        
        movieToUpdate.Year = movieUpdateCommand.year ?? movieToUpdate.Year;

        movieToUpdate.Duration = movieUpdateCommand.duration ?? movieToUpdate.Duration;
        
        movieToUpdate.Movie.Slug = string.IsNullOrEmpty(movieUpdateCommand.slug) ? movieToUpdate.Movie.Slug 
            : movieUpdateCommand.slug.ToLower();
        
        if (!movieUpdateCommand.genres.IsNullOrEmpty())
        {
            movieToUpdate.Genres = _videoService.UpdateVideoGenres(movieUpdateCommand.genres, movieToUpdate.MovieId);
        }
        
        if (!movieUpdateCommand.crew.IsNullOrEmpty())
        {
            movieToUpdate.Crew = _videoService.UpdateVideoCrew(movieUpdateCommand.crew, movieToUpdate);
        }
        
        if (!movieUpdateCommand.countries.IsNullOrEmpty())
        {
            movieToUpdate.Countries = _videoService.UpdateVideoCountries(movieUpdateCommand.countries, movieToUpdate.MovieId);
        }
        
        movieToUpdate.Movie.NeedSubscribe = movieUpdateCommand.needSubscribe?? movieToUpdate.Movie.NeedSubscribe;
        
        movieToUpdate.Movie.Price = movieUpdateCommand.price ?? movieToUpdate.Movie.Price;

        movieToUpdate.IsSendTelegram = movieUpdateCommand.isSendTelegram ?? movieToUpdate.IsSendTelegram;

        _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public MovieDto MovieToDto(string userId, Video movie, byte[] key)
    {
        if (movie.Movie.NeedSubscribe)
        {
            return movie.Movie.NeedSubscribe == _videoService.ContainsInOrdersVideo(userId, movie.VideoId)
                ? new(
                    movie.VideoId,
                    VideoService.DecryptStringFromBytes_Aes(movie.Movie.Poster, key, movie.Movie.PosterIv),
                    VideoService.DecryptStringFromBytes_Aes(movie.Movie.BigPoster, key, movie.Movie.BigPosterIv),
                    movie.Movie.Title,
                    VideoService.DecryptStringFromBytes_Aes(movie.VideoUrl, key, movie.VideoUrlIv),
                    movie.Movie.Slug,
                    movie.Year,
                    movie.Duration,
                    movie.Movie.CountOpened,
                    GenreService.MapGenresToDto(movie.Genres),
                    PersonService.MapCrewToPersonCrewDto(movie.Crew, key),
                    CountryService.MapCountiesToCountryDto(movie.Countries),
                    CommentService.MapCommentsToVideoCommentDtos(movie.Comments),
                    movie.Movie.NeedSubscribe,
                    movie.Movie.Price,
                    movie.Movie.Rating,
                    movie.IsSendTelegram)
                : new(
                    movie.VideoId,
                    VideoService.DecryptStringFromBytes_Aes(movie.Movie.Poster, key, movie.Movie.PosterIv),
                    VideoService.DecryptStringFromBytes_Aes(movie.Movie.BigPoster, key, movie.Movie.BigPosterIv),
                    movie.Movie.Title,
                    null,
                    movie.Movie.Slug,
                    movie.Year,
                    movie.Duration,
                    movie.Movie.CountOpened,
                    GenreService.MapGenresToDto(movie.Genres),
                    PersonService.MapCrewToPersonCrewDto(movie.Crew, key),
                    CountryService.MapCountiesToCountryDto(movie.Countries),
                    CommentService.MapCommentsToVideoCommentDtos(movie.Comments),
                    movie.Movie.NeedSubscribe,
                    movie.Movie.Price,
                    movie.Movie.Rating,
                    movie.IsSendTelegram);
        }
        
        return new(
            movie.VideoId,
            VideoService.DecryptStringFromBytes_Aes(movie.Movie.Poster, key, movie.Movie.PosterIv),
            VideoService.DecryptStringFromBytes_Aes(movie.Movie.BigPoster, key, movie.Movie.BigPosterIv),
            movie.Movie.Title,
            VideoService.DecryptStringFromBytes_Aes(movie.VideoUrl, key, movie.VideoUrlIv),
            movie.Movie.Slug,
            movie.Year,
            movie.Duration,
            movie.Movie.CountOpened,
            GenreService.MapGenresToDto(movie.Genres),
            PersonService.MapCrewToPersonCrewDto(movie.Crew, key),
            CountryService.MapCountiesToCountryDto(movie.Countries),
            CommentService.MapCommentsToVideoCommentDtos(movie.Comments),
            movie.Movie.NeedSubscribe,
            movie.Movie.Price,
            movie.Movie.Rating,
            movie.IsSendTelegram);
        
    }
    
    public MovieDto MovieToDto(Video movie, byte[] key)
    {
        return new(
            movie.VideoId,
            VideoService.DecryptStringFromBytes_Aes(movie.Movie.Poster, key, movie.Movie.PosterIv),
            VideoService.DecryptStringFromBytes_Aes(movie.Movie.BigPoster, key, movie.Movie.BigPosterIv),
            movie.Movie.Title,
            VideoService.DecryptStringFromBytes_Aes(movie.VideoUrl, key, movie.VideoUrlIv),
            movie.Movie.Slug,
            movie.Year,
            movie.Duration,
            movie.Movie.CountOpened,
            GenreService.MapGenresToDto(movie.Genres),
            PersonService.MapCrewToPersonCrewDto(movie.Crew, key),
            CountryService.MapCountiesToCountryDto(movie.Countries),
            CommentService.MapCommentsToVideoCommentDtos(movie.Comments),
            movie.Movie.NeedSubscribe,
            movie.Movie.Price,
            movie.Movie.Rating,
            movie.IsSendTelegram);
    }
}