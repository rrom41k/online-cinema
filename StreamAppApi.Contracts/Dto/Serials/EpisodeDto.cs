namespace StreamAppApi.Contracts.Dto;

public record OnlyEpisodeDto(
    string _id,
    string serialId,
    int seasonNumber,
    int seasonEpisode,
    string videoUrl,
    int year,
    int duration);

public record EpisodeDto(
    string _id,
    int episodeNumber,
    string videoUrl,
    int year,
    int duration);