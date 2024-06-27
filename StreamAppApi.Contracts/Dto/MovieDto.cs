namespace StreamAppApi.Contracts.Dto;

public record MovieDto(
    string _id,
    string poster,
    string bigPoster,
    string title,
    string? videoUrl,
    string slug,
    int year, // from video
    int duration, // from video
    int? countOpened,  // from video
    List<GenreDto> genres,
    List<PersonCrewDto> crew,
    List<CountryDto> counties,
    List<VideoCommentDto>? comments,
    bool needSubscribe,
    decimal? price,
    double? rating,
    bool? isSendTelegram);