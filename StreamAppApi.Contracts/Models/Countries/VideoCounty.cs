namespace StreamAppApi.Contracts.Models;

public class VideoCounty
{
    public string CountryId { get; set; }
    public Country Country { get; set; }

    public string VideoId { get; set; }
    public Video Video { get; set; }
}