namespace StreamAppApi.Contracts.Dto;

public record RoleDto(
    string RoleId,
    string Name,
    string? Description);