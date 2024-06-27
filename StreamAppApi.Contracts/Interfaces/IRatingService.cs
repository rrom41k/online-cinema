using StreamAppApi.Contracts.Commands.RatingCommands;
using StreamAppApi.Contracts.Dto;

namespace StreamAppApi.Contracts.Interfaces;

public interface IRatingService
{
    Task<RatingDto> SetRating(string userId, SetRatingCommand setRatingCommand, CancellationToken cancellationToken);
    Task<double> GetVideoRatingByUser(string userId, string videoId, CancellationToken cancellationToken);
}