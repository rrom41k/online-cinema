namespace StreamAppApi.Contracts.Commands.RatingCommands;

public record SetRatingCommand(string videoId, double value);