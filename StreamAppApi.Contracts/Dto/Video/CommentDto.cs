namespace StreamAppApi.Contracts.Dto;

public record CommentDto(string videoId, string userId, string? comment);