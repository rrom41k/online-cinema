namespace StreamAppApi.Contracts.Models;

public class SubscribeCountry
{
    public string SubscribeId { get; set; }
    public Subscribe Subscribe { get; set; }

    public string CountryId { get; set; }
    public Country Country { get; set; }
}