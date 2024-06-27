namespace StreamAppApi.Contracts.Dto;

public record VideoDto(
    string _id,
    string? videoUrl,
    int year,
    int duration);