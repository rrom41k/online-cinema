namespace StreamAppApi.Contracts.Models;

public class SubscribeGenre
{
    public string SubscribeId { get; set; }
    public Subscribe Subscribe { get; set; }

    public string GenreId { get; set; }
    public Genre Genre { get; set; }
}