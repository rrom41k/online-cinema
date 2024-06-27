namespace StreamAppApi.Contracts.Dto;

public record UserDto(
    string _id, 
    string email, 
    string login, 
    bool isAdmin,
    List<UserOrderDto>? orders);