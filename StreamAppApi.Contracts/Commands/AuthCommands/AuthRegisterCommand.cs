namespace StreamAppApi.Contracts.Commands.AuthCommands;

public record AuthRegisterCommand(string email, string login, string phone, string password);