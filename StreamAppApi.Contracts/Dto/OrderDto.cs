namespace StreamAppApi.Contracts.Dto;

public record OrderDto(
    string _id,
    DateTime orderDate,
    decimal sum,
    string userId,
    string? subscribeId,
    string? serialId,
    string? movieId);

public record UserOrderDto(
    string _id,
    DateTime orderDate,
    decimal sum,
    string? subscribeId,
    string? serialId,
    string? movieId);