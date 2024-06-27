namespace StreamAppApi.Contracts.Dto;

public record OnlySeasonDto(
    string serialId,
    int numberSeason,
    List<EpisodeDto>? episodes);

public record SeasonDto(
    int numberSeason,
    List<EpisodeDto>? episodes);