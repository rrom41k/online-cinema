using StreamAppApi.Contracts.Commands.CrewCommands;
using StreamAppApi.Contracts.Dto;

namespace StreamAppApi.Contracts.Commands.MovieCommands;

public record MovieUpdateCommand(
    string? poster,
    string? bigPoster,
    string? title,
    string? videoUrl, // To video
    int? year, // To video
    int? duration, // To video
    string? slug,
    string[]? genres,
    CrewUpdateCommand[]? crew, // {personId, roleId}
    string[]? countries,
    bool? needSubscribe,
    decimal? price,
    bool? isSendTelegram);
    
