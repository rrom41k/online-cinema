namespace StreamAppApi.Contracts.Dto;

public record SubscribeDto(
    string _id,
    string name,
    string? description,
    decimal Price, 
    int Duration, // в месяцах
    List<PersonDto> persons,
    List<CountryDto> countries,
    List<GenreDto> genres);