namespace StreamAppApi.Contracts.Dto;

public record PersonCrewDto(
    string _id,
    string name,
    string surname,
    string? patronymic,
    string slug,
    string? photo,
    RoleDto role);