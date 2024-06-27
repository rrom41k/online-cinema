using StreamAppApi.Contracts.Commands.CrewCommands;

namespace StreamAppApi.Contracts.Commands.MovieCommands;

public record MovieCreateCommand(
    string poster,
    string bigPoster,
    string title,
    string videoUrl, // To video
    int year, // To video
    int duration, // To video
    string slug,
    string[] genres,
    CrewCreateCommand[] crew, // {personId}
    string[] countries,
    bool needSubscribe,
    decimal? price,
    bool? isSendTelegram); // To video
    
