namespace StreamAppApi.Contracts.Commands.SubscribeCommands;

public record SubscribeCreateCommand(
    string name,
    string? description,
    decimal price,
    int duration, // в месяцах
    string[]? persons, // {personId}
    string[]? countries, // {countryId}
    string[]? genres); // {genreId}