using StreamAppApi.Contracts.Commands.CrewCommands;

namespace StreamAppApi.Contracts.Commands.SerialCommands;

public record EpisodeCreateCommand(
    int numberEpisode,
    string videoUrl,
    int year,
    int duration,
    bool? isSendTelegram,
    string[] genres,
    CrewCreateCommand[] crew, // {personId}
    string[] countries);
public record OnlyEpisodeCreateCommand(
    string seasonId,
    int numberEpisode,
    string videoUrl,
    int year,
    int duration,
    bool? isSendTelegram,
    string[] genres,
    CrewCreateCommand[] crew, // {personId}
    string[] countries);
    