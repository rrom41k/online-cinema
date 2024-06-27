namespace StreamAppApi.Contracts.Commands.SerialCommands;

public record SeasonUpdateCommand(string? serialId, int? numberSeason);