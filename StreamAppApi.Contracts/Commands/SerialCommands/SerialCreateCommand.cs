using StreamAppApi.Contracts.Commands.CrewCommands;

namespace StreamAppApi.Contracts.Commands.SerialCommands;

public record SerialCreateCommand(
    string poster,
    string bigPoster,
    string title,
    List<SeasonCreateCommand>? seasons,
    string slug,
    bool needSubscribe,
    decimal? price);