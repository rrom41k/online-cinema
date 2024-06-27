namespace StreamAppApi.Contracts.Models;

public class Rating
{
    public double Value { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }

    public string VideoId { get; set; }
    public Video Video { get; set; }
}