namespace StreamAppApi.Contracts.Dto;

public record FavoritesDto(List<MovieDto> movies, List<OnlyEpisodeDto> serialEpisodes);