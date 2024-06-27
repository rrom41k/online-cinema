namespace StreamAppApi.Contracts.Commands.UserCommands;

public record UserCreateCommand(string login, string phone, string email, string password, bool isAdmin);