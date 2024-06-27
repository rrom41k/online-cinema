using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Commands.RatingCommands;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Interfaces;
using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll;

public class RatingService : IRatingService
{
    private readonly StreamPlatformDbContext _dbContext;

    public RatingService(StreamPlatformDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
    }

    public async Task<RatingDto> SetRating(
        string userId,
        SetRatingCommand setRatingCommand,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
            throw new OperationCanceledException();

        if (setRatingCommand.value > 10)
            throw new ArgumentException("Rating value must be under 10");
        
        var existingVideo = await _dbContext.Videos
            .FirstOrDefaultAsync(video => video.VideoId == setRatingCommand.videoId, cancellationToken);
        
        var rating = await _dbContext.Ratings
            .FirstOrDefaultAsync(
                rating =>
                    rating.VideoId == existingVideo.VideoId && rating.UserId == userId,
                cancellationToken);

        if (rating != null)
        {
            rating.Value = setRatingCommand.value;
        }
        else
        {
            rating = new()
            {
                UserId = userId,
                VideoId = existingVideo.VideoId,
                Value = setRatingCommand.value
            };

            _dbContext.Ratings.Add(rating);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Рассчитываем средний рейтинг для фильма.
        var averageRating = await AverageRatingByVideoAsync(setRatingCommand.videoId, cancellationToken);

        // Обновляем средний рейтинг для фильма.
        await UpdateRatingAsync(setRatingCommand.videoId, averageRating, cancellationToken);

        return UserVideoToRatingDto(rating);
    }

    public async Task<double> GetVideoRatingByUser(
        string userId,
        string videoId,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
        {
            throw new OperationCanceledException();
        }

        var rating = await _dbContext.Ratings
            .Where(rating => rating.VideoId == videoId && rating.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception("User didn't set rating to this film");

        return rating.Value;
    }

    private async Task UpdateRatingAsync(
        string videoId,
        double newRating,
        CancellationToken cancellationToken = default)
    {
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(video => video.VideoId == videoId, cancellationToken);

        if (video != null)
        {
            switch (video.Type)
            {
                case TypeVideo.Movie:
                {
                    video.Movie.Rating = newRating;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                } break;
                case TypeVideo.Serial:
                {
                    video.Season.Serial.Rating = newRating;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                } break;
            }
        }
        else
        {
            throw new ArgumentException("Video not fount.");
        }
    }

    private async Task<double> AverageRatingByVideoAsync(string videoId, CancellationToken cancellationToken)
    {
        var ratingsVideo = await _dbContext.Ratings
                // Include Movie
            .Include(rating => rating.Video)
            .ThenInclude(video => video.Movie)
                // Include Seasons
            .Include(rating => rating.Video)
            .ThenInclude(video => video.Season)
                // Filter
            .Where(rating => rating.VideoId == videoId)
            .ToListAsync(cancellationToken);
        
        if (ratingsVideo.Count > 0)
        {
            switch (ratingsVideo.FirstOrDefault().Video.Type)
            {
                case TypeVideo.Movie:
                    return ratingsVideo.Select(rating => rating.Value).Average();
                case TypeVideo.Serial:
                {
                    var serial = _dbContext.Serials
                        .Include(serial => serial.Seasons)
                        .ThenInclude(season => season.Videos)
                        .FirstOrDefault(serial => serial.Seasons.Select(season => season.Videos)
                            .Any(videos => videos.Any(video => video.VideoId == videoId)));
                    
                    var videosIds = _dbContext.Videos.Where(video => video.Season.SerialId == serial.SerialId)
                        .Select(video => video.VideoId);
                    
                    return _dbContext.Ratings.Where(rating => videosIds.Contains(rating.VideoId))
                        .Select(rating => rating.Value)
                        .Average();
                }
                default:
                    throw new Exception($"Undefined type of video with id: \"{ratingsVideo.FirstOrDefault().VideoId}\"");
            }
        }
        // Возвращайте значение по умолчанию, например, 0, если нет рейтингов для данного фильма.
        return 0;
    }

    private RatingDto UserVideoToRatingDto(Rating rating)
    {
        return new(rating.UserId, rating.VideoId, rating.Value);
    }
}