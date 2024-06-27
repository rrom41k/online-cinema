using StreamAppApi.Contracts.Commands.CrewCommands;

namespace StreamAppApi.Contracts.Commands.SerialCommands;

public record SerialUpdateCommand(
    string? poster,
    string? bigPoster,
    string? title,
    List<EpisodeCreateCommand>? episodes,
    string? slug,
    bool? needSubscribe,
    decimal? price);