namespace StreamAppApi.Contracts.Commands.PersonCommands;

public record PersonCreateCommand(string name, string surname, string? patronymic, string slug, string photo);