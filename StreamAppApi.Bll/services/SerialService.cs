using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Commands.SerialCommands;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Interfaces;
using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll;

public class SerialService : ISerialService
{
    private readonly StreamPlatformDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly VideoService _videoService;
    private readonly TelegramBotService _telegramBotService;
    private IHttpContextAccessor _httpContextAccessor;
    
    public SerialService(IConfiguration configuration, VideoService videoService, StreamPlatformDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, TelegramBotOptions telegramBotOptions)
    {
        _configuration = configuration;
        _videoService = videoService;
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _telegramBotService = new(telegramBotOptions);;
    }
    
    public async Task<List<SerialDto>> GetAllSerials(string userId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        string? searchTerm = _httpContextAccessor.HttpContext.Request.Query["searchTerm"];
        
        searchTerm ??= string.Empty;
        
        var serials = await GetAllSerialsNoTracking()
            .Where(serial => serial.Slug.Contains(searchTerm.ToLower()) || serial.Title.ToLower().Contains(searchTerm.ToLower()))
            .OrderByDescending(serial => serial.Rating)
            .ToListAsync(cancellationToken);
        
        return serials.Select(serial => SerialToDto(userId, serial)).ToList();
    }
    
    public async Task<SerialDto> GetSerialBySlug(string userId, string slug, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var serial = await GetAllSerialsNoTracking()
            .FirstOrDefaultAsync(serial => serial.Title.Contains(slug), cancellationToken)
            ?? throw new Exception($"Serial with slug: \"{slug}\" don't exist");
        
        return SerialToDto(userId, serial);
    }
    
    public async Task<List<SerialDto>> GetSerialByGenres(string userId, List<string> genreIds, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var serials = GetAllSerialsNoTracking()
                .Where(serial => serial.Seasons.FirstOrDefault()
                    .Videos.FirstOrDefault()
                        .Genres.Any(vg => genreIds.Contains(vg.GenreId)))
            ?? throw new Exception($"Serial with any of genre Ids: \"{genreIds}\" don't exist");
        
        return await serials.Select(serial => SerialToDto(userId, serial)).ToListAsync(cancellationToken);
    }
    
    public async Task<List<SerialDto>> GetSerialByPersons(string userId, List<string> personIds, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var serials = GetAllSerialsNoTracking()
                .Where(serial => serial.Seasons.FirstOrDefault()
                    .Videos.FirstOrDefault()
                    .Crew.Any(crew => personIds.Contains(crew.PersonId)))
            ?? throw new Exception($"Serial with any of person Ids: \"{personIds}\" don't exist");
        
        return await serials.Select(serial => SerialToDto(userId, serial)).ToListAsync(cancellationToken);
    }
    
    public async Task UpdateCountOpenedAsync(
        string serialId,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var serial = GetAllSerialsNoTracking()
                .FirstOrDefault(serial => serial.SerialId == serialId)
            ?? throw new Exception($"Serial with Id: \"{serialId}\" don't exist");
        
        serial.CountOpened++;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<List<SerialDto>> GetMostPopularSerialAsync(string userId, CancellationToken cancellationToken)
    {
        var popularSerials = await GetAllSerialsNoTracking()
            .Where(serial => serial.CountOpened > 0)
            .OrderByDescending(serial => serial.CountOpened)
            .ToListAsync(cancellationToken);
        
        return popularSerials.Select(serial => SerialToDto(userId, serial)).ToList();
    }
    
    public async Task<OrderDto> BuySerialById(string userId, string serialId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var existingSerial = await GetAllSerials()
                .FirstOrDefaultAsync(
                    serial =>
                        serial.SerialId == serialId,
                    cancellationToken)
            ?? throw new ArgumentException("Serial not found.");
        
        Order newOrder = new(
            existingSerial.Price ?? 0,
            userId,
            null,
            null,
            serialId);
        
        await _dbContext.Orders.AddAsync(newOrder, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return new(newOrder.OrderId, newOrder.OrderDate, newOrder.Sum, newOrder.UserId, null, newOrder.SerialId, null);
    }
    
    public async Task<OnlySeasonDto> GetSeasonBySlug(string userId, string slug, int seasonNumber, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var serial = await GetAllSerialsNoTracking()
                .FirstOrDefaultAsync(serial => serial.Title.Contains(slug), cancellationToken)
            ?? throw new Exception($"Serial with slug: \"{slug}\" don't exist");
        
        var season = serial.Seasons.FirstOrDefault(season => season.NumberSeason == seasonNumber)
            ?? throw new Exception($"Season of serial \"{serial.Title}\" with number: \"{slug}\" don't exist");
        
        return SeasonNeedSubToDto<OnlySeasonDto>(userId, season);
    }
    
    public async Task<OnlyEpisodeDto> GetEpisodeById(
        string userId, 
        string slug,
        int seasonNumber,
        string episodeId,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var serial = await GetAllSerialsNoTracking()
                .FirstOrDefaultAsync(serial => serial.Title.Contains(slug), cancellationToken)
            ?? throw new Exception($"Serial with slug: \"{slug}\" don't exist");
        
        var season = serial.Seasons.FirstOrDefault(season => season.NumberSeason == seasonNumber)
            ?? throw new Exception($"Season of serial \"{serial.Title}\" with number: \"{slug}\" don't exist");
        
        var episode = season.Videos.FirstOrDefault(episode => episode.VideoId == episodeId)
            ?? throw new Exception($"Episode of serial \"{serial.Title}\" with id: \"{episodeId}\" don't exist");
        
        return EpisodeNeedSubToDto<OnlyEpisodeDto>(userId, episode);
    }
    
    /* Admin rights */
    
    public async Task<SerialDto> CreateSerial(SerialCreateCommand serialCreateCommand, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var newSerial = CreateSerialHelper(serialCreateCommand);
        _dbContext.Serials.Add(newSerial);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return SerialToDto(null, newSerial);
    }
    
    public async Task<List<SerialDto>> BatchCreateSerials(IFormFileCollection files, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        List<SerialDto> result = new();
        
        // Шаг 1: Чтение данных из файлов
        var movieCreateCommands = await ReadSerialCreateCommandsFromFiles(files, cancellationToken);
        
        // Шаг 2: Создание фильмов в пакетном режиме
        foreach (var movieCreateCommand in movieCreateCommands)
        {
            result.Add(await CreateSerial(movieCreateCommand, cancellationToken));
        }
        
        // Сохранение изменений в базе данных
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return result;
    }
    
    public async Task<SerialDto> GetSerialById(string serialId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var currentSerial = GetAllSerialsNoTracking().FirstOrDefault(serial => serial.SerialId == serialId);
        
        return SerialToDto(null, currentSerial);
    }
    
    public async Task<SerialDto> UpdateSerial(string serialId, SerialUpdateCommand serialUpdateCommand, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var serialToUpdate = GetAllSerials().FirstOrDefault(serial => serial.SerialId == serialId);
        
        UpdateSerialHelper(ref serialToUpdate, serialUpdateCommand, cancellationToken);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return SerialToDto(null, serialToUpdate);
    }
    
    public async Task DeleteSerial(string serialId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var serialToDelete = _dbContext.Serials.FirstOrDefault(seral => seral.SerialId == serialId) 
            ?? throw new ArgumentException("Serial not found.");
        
        _dbContext.Serials.Remove(serialToDelete);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<OnlySeasonDto> CreateSeason(OnlySeasonCreateCommand seasonCreateCommand, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var newSeason = CreateSeasonHelper(seasonCreateCommand);
        await _dbContext.Seasons.AddAsync(newSeason, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return SeasonToDto<OnlySeasonDto>(newSeason);
    }
    
    public async Task<OnlySeasonDto> GetSeasonById(string serialId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var existingSeason = await _dbContext.Seasons.FirstOrDefaultAsync(season => 
                season.SerialId == serialId, cancellationToken)
            ?? throw new ArgumentException("Season not found.");
        
        return SeasonToDto<OnlySeasonDto>(existingSeason);
    }
    
    public async Task<OnlySeasonDto> UpdateSeason(string seasonId, SeasonUpdateCommand seasonUpdateCommand,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var existingSeason = await _dbContext.Seasons.FirstOrDefaultAsync(season =>
                season.SeasonId == seasonId, cancellationToken)
            ?? throw new ArgumentException("Season not found.");
        
        UpdateSeasonHelper(ref existingSeason, seasonUpdateCommand);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return SeasonToDto<OnlySeasonDto>(existingSeason);
    }
    
    public async Task DeleteSeason(string seasonId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var existingSeason = await _dbContext.Seasons.FirstOrDefaultAsync(season =>
                season.SeasonId == seasonId, cancellationToken)
            ?? throw new ArgumentException("Season not found.");
        
        _dbContext.Seasons.Remove(existingSeason);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<OnlyEpisodeDto> CreateEpisode(OnlyEpisodeCreateCommand episodeCreateCommand, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var newEpisode = CreateEpisodeHelper(episodeCreateCommand);
        
        await _dbContext.Videos.AddAsync(newEpisode, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        
        if (!newEpisode.IsSendTelegram)
        {
            try
            {
                byte[] key = Convert.FromBase64String(_configuration["AppSettings:CryptKey"]);
                await _telegramBotService.SendPostLink($"Скорее смотрите новый {newEpisode.EpisodeNumber} эпизод сериала {newEpisode.Season?.Serial.Title} в нашем онлайн-кинотеатре!",
                    VideoService.DecryptStringFromBytes_Aes(newEpisode.Season?.Serial.Poster, key, newEpisode.Season?.Serial.PosterIv));
            }
            catch (Exception exception)
            {
                Console.WriteLine("warn: " + exception.Message);
                newEpisode.IsSendTelegram = false;
            }
        }
        
        return EpisodeToDto<OnlyEpisodeDto>(newEpisode);
    }
    
    public async Task<OnlyEpisodeDto> UpdateEpisode(string videoId, EpisodeUpdateCommand episodeUpdateCommand, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var episodeToUpdate = _dbContext.Videos
            .Include(episode => episode.Season)
            .ThenInclude(season => season!.Serial).FirstOrDefault(episode => episode.VideoId == videoId)
            ?? throw new ArgumentException("Episode not found.");
        
        UpdateEpisodeHelper(ref episodeToUpdate, episodeUpdateCommand);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return EpisodeToDto<OnlyEpisodeDto>(episodeToUpdate);
    }
    
    public async Task DeleteEpisode(string videoId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var episodeToRemove = _dbContext.Videos
                .Include(episode => episode.Season)
                .ThenInclude(season => season!.Serial).FirstOrDefault(episode => episode.VideoId == videoId)
            ?? throw new ArgumentException("Episode not found.");
        
        _dbContext.Videos.Remove(episodeToRemove);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    /* Helpful methods */
    
    private IQueryable<Serial> GetAllSerials()
    {
        return _dbContext.Serials
            // Include comments
            .Include(serial => serial.Seasons)
                .ThenInclude(season => season.Videos)
                    .ThenInclude(video => video.Comments)
                        .ThenInclude(comment => comment.User)
            // Include rating
            .Include(serial => serial.Seasons)
                .ThenInclude(season => season.Videos)
                    .ThenInclude(video => video.Ratings)
            // Include crew and role
            .Include(serial => serial.Seasons)
                .ThenInclude(season => season.Videos)
                    .ThenInclude(video => video.Crew)
                        .ThenInclude(crew => crew.Role)
            // Include crew and person
            .Include(serial => serial.Seasons)
                .ThenInclude(season => season.Videos)
                    .ThenInclude(video => video.Crew)
                        .ThenInclude(crew => crew.Person)
            // Include genre
            .Include(serial => serial.Seasons)
                .ThenInclude(season => season.Videos)
                    .ThenInclude(video => video.Genres)
                        .ThenInclude(videoGenre => videoGenre.Genre)
            // Include counties
            .Include(serial => serial.Seasons)
                .ThenInclude(season => season.Videos)
                    .ThenInclude(video => video.Countries)
                        .ThenInclude(videoGenre => videoGenre.Country);
    }
    private IQueryable<Serial> GetAllSerialsNoTracking()
    {
        return _dbContext.Serials.AsNoTracking()
            // Include comments
            .Include(serial => serial.Seasons)
                .ThenInclude(season => season.Videos)
                    .ThenInclude(video => video.Comments)
                        .ThenInclude(comment => comment.User)
            // Include rating
            .Include(serial => serial.Seasons)
                .ThenInclude(season => season.Videos)
                .ThenInclude(video => video.Ratings)
            // Include crew and role
            .Include(serial => serial.Seasons)
                .ThenInclude(season => season.Videos)
                    .ThenInclude(video => video.Crew)
                        .ThenInclude(crew => crew.Role)
            // Include crew and person
            .Include(serial => serial.Seasons)
                .ThenInclude(season => season.Videos)
                    .ThenInclude(video => video.Crew)
                        .ThenInclude(crew => crew.Person)
            // Include genre
            .Include(serial => serial.Seasons)
                .ThenInclude(season => season.Videos)
                    .ThenInclude(video => video.Genres)
                        .ThenInclude(videoGenre => videoGenre.Genre)
            // Include counties
            .Include(serial => serial.Seasons)
                .ThenInclude(season => season.Videos)
                    .ThenInclude(video => video.Countries)
                        .ThenInclude(videoGenre => videoGenre.Country);
    }
    
    T EpisodeToDto<T>(Video episode) where T : class
    {
        return typeof(T) switch
        {
            Type t when t == typeof(EpisodeDto) =>
                new EpisodeDto(
                    episode.VideoId,
                    episode.EpisodeNumber
                    ?? throw new($"Episode with id \"{episode.VideoId}\" don't have episode number"),
                    VideoService.DecryptStringFromBytes_Aes(
                        episode.VideoUrl,
                        Convert.FromBase64String(_configuration["AppSettings:CryptKey"]),
                        episode.VideoUrlIv),
                    episode.Year,
                    episode.Duration) as T,
            Type t when t == typeof(OnlyEpisodeDto) =>
                new OnlyEpisodeDto(
                    episode.VideoId,
                    episode.Season.Serial.SerialId,
                    episode.Season.NumberSeason,
                    episode.EpisodeNumber
                    ?? throw new($"Episode with id \"{episode.VideoId}\" don't have episode number"),
                    VideoService.DecryptStringFromBytes_Aes(
                        episode.VideoUrl,
                        Convert.FromBase64String(_configuration["AppSettings:CryptKey"]),
                        episode.VideoUrlIv),
                    episode.Year,
                    episode.Duration) as T,
            _ => throw new InvalidOperationException($"Unsupported type: {typeof(T)}")
        };
    }
    T EpisodeNeedSubToDto<T>(string userId, Video episode) where T : class
    {
        return typeof(T) switch
        {
            Type t when t == typeof(EpisodeDto) =>
                _videoService.ContainsInOrdersVideo(userId, episode.VideoId)
                    ? new EpisodeDto(
                        episode.VideoId,
                        episode.EpisodeNumber
                        ?? throw new($"Episode with id \"{episode.VideoId}\" don't have episode number"),
                        VideoService.DecryptStringFromBytes_Aes(
                            episode.VideoUrl,
                            Convert.FromBase64String(_configuration["AppSettings:CryptKey"]),
                            episode.VideoUrlIv),
                        episode.Year,
                        episode.Duration) as T
                    : new EpisodeDto(
                        episode.VideoId,
                        episode.EpisodeNumber
                        ?? throw new($"Episode with id \"{episode.VideoId}\" don't have episode number"),
                        null,
                        episode.Year,
                        episode.Duration) as T,
            Type t when t == typeof(OnlyEpisodeDto) =>
                _videoService.ContainsInOrdersVideo(userId, episode.VideoId)
                    ? new OnlyEpisodeDto(
                        episode.VideoId,
                        episode.Season.Serial.SerialId,
                        episode.Season.NumberSeason,
                        episode.EpisodeNumber
                        ?? throw new($"Episode with id \"{episode.VideoId}\" don't have episode number"),
                        VideoService.DecryptStringFromBytes_Aes(
                            episode.VideoUrl,
                            Convert.FromBase64String(_configuration["AppSettings:CryptKey"]),
                            episode.VideoUrlIv),
                        episode.Year,
                        episode.Duration) as T
                    : new OnlyEpisodeDto(
                        episode.VideoId,
                        episode.Season.Serial.SerialId,
                        episode.Season.NumberSeason,
                        episode.EpisodeNumber
                        ?? throw new($"Episode with id \"{episode.VideoId}\" don't have episode number"),
                        null,
                        episode.Year,
                        episode.Duration) as T,
            _ => throw new InvalidOperationException($"Unsupported type: {typeof(T)}")
        };
    }
    
    T SeasonToDto<T>(Season season) where T : class
    {
        return typeof(T) switch
        {
            Type t when t == typeof(SeasonDto) =>
                new SeasonDto(season.NumberSeason, season.Videos.Select(EpisodeToDto<EpisodeDto>).ToList()) as T,
            Type t when t == typeof(OnlySeasonDto) =>
                new OnlySeasonDto(season.SerialId, season.NumberSeason, season.Videos.Select(EpisodeToDto<EpisodeDto>).ToList()) as T,
            _ => throw new InvalidOperationException($"Unsupported type: {typeof(T)}")
        };
    }
    T SeasonNeedSubToDto<T>(string userId, Season season) where T : class
    {
        return typeof(T) switch
        {
            Type t when t == typeof(SeasonDto) =>
                new SeasonDto(season.NumberSeason,
                    season.Videos.Select(episode => EpisodeNeedSubToDto<EpisodeDto>(userId, episode)).ToList()) as T,
            Type t when t == typeof(OnlySeasonDto) =>
                new OnlySeasonDto(season.SerialId, season.NumberSeason, season.Videos.Select(EpisodeToDto<EpisodeDto>).ToList()) as T,
            _ => throw new InvalidOperationException($"Unsupported type: {typeof(T)}")
        };
    }
    
    SerialDto SerialToDto(string? userId, Serial serial)
    {
        var key = Convert.FromBase64String(_configuration["AppSettings:CryptKey"]);
        
        if (userId != null)
        {
            return new(
                serial.SerialId,
                serial.Title,
                serial.Slug,
                VideoService.DecryptStringFromBytes_Aes(serial.Poster, key, serial.PosterIv),
                VideoService.DecryptStringFromBytes_Aes(serial.BigPoster, key, serial.BigPosterIv),
                serial.Price ?? 0,
                serial.NeedSubscribe ?? false,
                serial.NeedSubscribe == true
                    ? serial.Seasons.Select(season => SeasonNeedSubToDto<SeasonDto>(userId, season)).ToList()
                    : serial.Seasons.Select(SeasonToDto<SeasonDto>).ToList(),
                GenreService.MapGenresToDto(serial.Seasons.FirstOrDefault().Videos.FirstOrDefault().Genres),
                PersonService.MapCrewToPersonCrewDto(serial.Seasons.FirstOrDefault().Videos.FirstOrDefault().Crew, key),
                CountryService.MapCountiesToCountryDto(serial.Seasons.FirstOrDefault().Videos.FirstOrDefault().Countries),
                CommentService.MapCommentsToVideoCommentDtos(serial.Seasons.FirstOrDefault().Videos.FirstOrDefault().Comments)
            );
        }
        serial = GetAllSerials().FirstOrDefault(s => s.SerialId == serial.SerialId);
        return new(
            serial.SerialId,
            serial.Title,
            serial.Slug,
            VideoService.DecryptStringFromBytes_Aes(serial.Poster, key, serial.PosterIv),
            VideoService.DecryptStringFromBytes_Aes(serial.BigPoster, key, serial.BigPosterIv),
            serial.Price ?? 0,
            serial.NeedSubscribe ?? false,
            serial.Seasons.Select(SeasonToDto<SeasonDto>).ToList(),
            GenreService.MapGenresToDto(serial.Seasons.FirstOrDefault().Videos.FirstOrDefault().Genres),
            PersonService.MapCrewToPersonCrewDto(serial.Seasons.FirstOrDefault().Videos.FirstOrDefault().Crew, key),
            CountryService.MapCountiesToCountryDto(serial.Seasons.FirstOrDefault().Videos.FirstOrDefault().Countries),
            CommentService.MapCommentsToVideoCommentDtos(serial.Seasons.FirstOrDefault().Videos.FirstOrDefault().Comments)
        );
    }
    
    private Serial CreateSerialHelper(SerialCreateCommand serialCreateCommand)
    {
        var newSerial = _dbContext.Serials.FirstOrDefault(seral => seral.Slug  ==  serialCreateCommand.slug) == null ?
            new Serial(
                serialCreateCommand.title,
                serialCreateCommand.slug,
                VideoService.EncryptStringToBytes_Aes(serialCreateCommand.poster,
                    Convert.FromBase64String(_configuration["AppSettings:CryptKey"]), out var posterUrlIv),
                posterUrlIv,
                VideoService.EncryptStringToBytes_Aes(serialCreateCommand.bigPoster,
                    Convert.FromBase64String(_configuration["AppSettings:CryptKey"]), out var bigPosterUrlIv),
                bigPosterUrlIv,
                serialCreateCommand.needSubscribe,
                serialCreateCommand.price) : 
                throw new ArgumentException($"Serial with slug \"{serialCreateCommand.slug}\" alredy exist");
        
        if (serialCreateCommand.seasons != null)
        {
            List<Season> newSeasons = new();
            
            foreach (var season in serialCreateCommand.seasons)
            {
                Season newSeason = new(newSerial.SerialId, season.seasonNumber);
                newSeason = CreateSeasonHelper(new(newSerial.SerialId, season.seasonNumber, season.episodes));
                newSeason.Serial = newSerial;
                newSeasons.Add(newSeason);
            }
            
            newSerial.Seasons = newSeasons;
        }
        
        return newSerial;
    }
    
    private Season CreateSeasonHelper(OnlySeasonCreateCommand seasonCreateCommand)
    {
        var newSeason = new Season(seasonCreateCommand.serialId, seasonCreateCommand.seasonNumber);
        _dbContext.Seasons.Add(newSeason);
        
        if (seasonCreateCommand.episodes != null)
        {
            List<Video> newEpisodes = new List<Video>();
            
            foreach (var episode in seasonCreateCommand.episodes)
            {
                var newEpisode = CreateEpisodeHelper(new(
                    newSeason.SeasonId,
                    episode.numberEpisode,
                    episode.videoUrl,
                    episode.year,
                    episode.duration,
                    episode.isSendTelegram,
                    episode.genres,
                    episode.crew,
                    episode.countries));
                newEpisode.Season = newSeason;
                newEpisodes.Add(newEpisode);
            }
            
            newSeason.Videos = newEpisodes;
        }
        
        return newSeason;
    }
    
    private async Task<List<SerialCreateCommand>> ReadSerialCreateCommandsFromFiles(IFormFileCollection files, CancellationToken cancellationToken)
    {
        List<SerialCreateCommand> serialCreateCommands = new();
        
        foreach (var file in files)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is not provided or empty.");
            }
            
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;
            
            var commands = await JsonSerializer.DeserializeAsync<List<SerialCreateCommand>>(stream, cancellationToken: cancellationToken);
            if (commands != null)
            {
                serialCreateCommands.AddRange(commands);
            }
        }
        
        return serialCreateCommands;
    }
    
    private void UpdateEpisodeHelper(ref Video episodeToUpdate, EpisodeUpdateCommand episodeUpdateCommand)
    {
        if (!string.IsNullOrEmpty(episodeUpdateCommand.videoUrl))
        {
            episodeToUpdate.VideoUrl = VideoService.EncryptStringToBytes_Aes(episodeUpdateCommand.videoUrl,
                Convert.FromBase64String(_configuration["AppSettings:CryptKey"]), out var videoUrlIv);
            episodeToUpdate.VideoUrlIv = videoUrlIv;
        }
        
        episodeToUpdate.Year = episodeUpdateCommand.year ?? episodeToUpdate.Year;

        episodeToUpdate.Duration = episodeUpdateCommand.duration ?? episodeToUpdate.Duration;
        
        if (!episodeUpdateCommand.genres.IsNullOrEmpty())
        {
            episodeToUpdate.Genres = _videoService.UpdateVideoGenres(episodeUpdateCommand.genres, episodeToUpdate.VideoId);
        }
        
        if (!episodeUpdateCommand.crew.IsNullOrEmpty())
        {
            episodeToUpdate.Crew = _videoService.UpdateVideoCrew(episodeUpdateCommand.crew, episodeToUpdate);
        }
        
        if (!episodeUpdateCommand.countries.IsNullOrEmpty())
        {
            episodeToUpdate.Countries = _videoService.UpdateVideoCountries(episodeUpdateCommand.countries, episodeToUpdate.VideoId);
        }

        episodeToUpdate.IsSendTelegram = episodeUpdateCommand.isSendTelegram ?? episodeToUpdate.IsSendTelegram;
    }
    
    private void UpdateSeasonHelper(ref Season seasonToUpdate, SeasonUpdateCommand seasonUpdateCommand)
    {
        seasonToUpdate.NumberSeason = seasonUpdateCommand.numberSeason ?? seasonToUpdate.NumberSeason;

        seasonToUpdate.NumberSeason = seasonUpdateCommand.numberSeason ?? seasonToUpdate.NumberSeason;
    }
    
    private void UpdateSerialHelper(ref Serial serialToUpdate, SerialUpdateCommand serialUpdateCommand, 
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(serialUpdateCommand.poster))
        {
            serialToUpdate.Poster = VideoService.EncryptStringToBytes_Aes(serialUpdateCommand.poster,
                Convert.FromBase64String(_configuration["AppSettings:CryptKey"]), out var posterIv);
            serialToUpdate.PosterIv = posterIv;
        }
        
        if (!string.IsNullOrEmpty(serialUpdateCommand.bigPoster))
        {
            serialToUpdate.BigPoster = VideoService.EncryptStringToBytes_Aes(serialUpdateCommand.bigPoster,
                Convert.FromBase64String(_configuration["AppSettings:CryptKey"]), out var bigPosterIv);
            serialToUpdate.BigPosterIv = bigPosterIv;
        }
        
        serialToUpdate.Title = string.IsNullOrEmpty(serialUpdateCommand.title) ? serialToUpdate.Title 
            : serialUpdateCommand.title;
        
        serialToUpdate.Slug = string.IsNullOrEmpty(serialUpdateCommand.slug) ? serialToUpdate.Slug 
            : serialUpdateCommand.slug.ToLower();
        
        serialToUpdate.NeedSubscribe = serialUpdateCommand.needSubscribe?? serialToUpdate.NeedSubscribe;
        
        serialToUpdate.Price = serialUpdateCommand.price ?? serialToUpdate.Price;
    }
    
    private Video CreateEpisodeHelper(OnlyEpisodeCreateCommand episodeCreateCommand)
    {
        Video newEpisode = new(
            VideoService.EncryptStringToBytes_Aes(
                episodeCreateCommand.videoUrl,
                Convert.FromBase64String(_configuration["AppSettings:CryptKey"]),
                out var videoUrlIv),
            videoUrlIv,
            episodeCreateCommand.year,
            episodeCreateCommand.duration,
            TypeVideo.Serial,
            episodeCreateCommand.isSendTelegram ?? false);
        
        newEpisode.SeasonId = episodeCreateCommand.seasonId;
        newEpisode.EpisodeNumber = episodeCreateCommand.numberEpisode;
        
        newEpisode.Genres = GenreService.MapGenresArrToList(newEpisode, episodeCreateCommand.genres);
        newEpisode.Crew = PersonService.MapPersonsArrToList(newEpisode, episodeCreateCommand.crew);
        newEpisode.Countries = CountryService.MapCountiesIdsToVideoCountryList(newEpisode, episodeCreateCommand.countries);
        
        return newEpisode;
    }
}