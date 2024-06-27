namespace StreamAppApi.Contracts.Commands.SerialCommands;

public record UpdateCountOpenedEpisodeCommand(string slug, int season, int episode);