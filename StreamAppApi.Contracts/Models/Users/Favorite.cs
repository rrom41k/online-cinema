namespace StreamAppApi.Contracts.Models;

public class Favorite
{
    public string UserId { get; set; }
    public User User { get; set; }

    public string VideoId { get; set; }
    public Video Video { get; set; }
}