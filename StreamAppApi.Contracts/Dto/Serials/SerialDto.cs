namespace StreamAppApi.Contracts.Dto;

public record SerialDto(
    string _id,
    string title,
    string slug,
    string poster,
    string bigPoster,
    decimal price,
    bool needSubscribe,
    List<SeasonDto> seasons,
    List<GenreDto> genres,
    List<PersonCrewDto> crew,
    List<CountryDto> countries,
    List<VideoCommentDto>? comments);