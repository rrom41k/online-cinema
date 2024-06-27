namespace StreamAppApi.Contracts.Models;

public class VideoGenre
{
    public string GenreId { get; set; }
    public Genre Genre { get; set; }

    public string VideoId { get; set; }
    public Video Video { get; set; }
}