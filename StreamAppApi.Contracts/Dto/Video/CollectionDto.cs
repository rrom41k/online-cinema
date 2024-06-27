namespace StreamAppApi.Contracts.Dto;

public record CollectionDto(
    string genreId,
    string videoId,
    string image,
    string title,
    string slug);