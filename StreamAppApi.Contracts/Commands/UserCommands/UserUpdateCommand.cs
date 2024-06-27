namespace StreamAppApi.Contracts.Commands.UserCommands;

public record UserUpdateCommand(
    string? email, 
    string? login, 
    string? phone, 
    string? password, 
    bool? isAdmin);