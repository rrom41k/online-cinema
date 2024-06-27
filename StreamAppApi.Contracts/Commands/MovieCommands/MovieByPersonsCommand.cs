namespace StreamAppApi.Contracts.Commands.MovieCommands;

public record MovieByPersonsCommand(List<string> personIds);