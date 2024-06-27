namespace StreamAppApi.Contracts.Commands.SerialCommands;

public record SeasonCreateCommand(int seasonNumber, List<EpisodeCreateCommand>? episodes);
public record OnlySeasonCreateCommand(string serialId, int seasonNumber, List<EpisodeCreateCommand>? episodes);