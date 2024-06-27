namespace StreamAppApi.Contracts.Dto;

public record PersonDto(
    string _id,
    string name,
    string surname,
    string? patronymic,
    string slug,
    string photo);