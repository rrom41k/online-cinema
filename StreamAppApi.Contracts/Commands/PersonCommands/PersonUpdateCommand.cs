namespace StreamAppApi.Contracts.Commands.PersonCommands;

public record PersonUpdateCommand(string? name, string? surname, string? patronymic, string? slug, string? photo);