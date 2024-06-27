using StreamAppApi.Contracts.Commands.CrewCommands;

namespace StreamAppApi.Contracts.Commands.SerialCommands;

public record EpisodeUpdateCommand(
    string? videoUrl, // To video
    int? year, // To video
    int? duration, // To video
    bool? isSendTelegram, // To video
    string?[] genres, // To video
    CrewUpdateCommand?[] crew, // {personId, roleId}
    string?[] countries); // To video
    