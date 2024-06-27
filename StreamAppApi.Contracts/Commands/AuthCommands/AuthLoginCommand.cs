namespace StreamAppApi.Contracts.Commands.AuthCommands;

public record AuthLoginCommand(string login, string password);