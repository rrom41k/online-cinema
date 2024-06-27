namespace StreamAppApi.Contracts.Commands.CrewCommands;

public record CrewUpdateCommand(
    string personId,
    string roleId);