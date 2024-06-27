namespace StreamAppApi.Contracts.Commands.CrewCommands;

public record CrewCreateCommand(
    string personId,
    string roleId);