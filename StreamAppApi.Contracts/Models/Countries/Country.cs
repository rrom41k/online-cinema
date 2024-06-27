using System.Collections;
using NUlid;

namespace StreamAppApi.Contracts.Models;

public class Country
{
    public Country(string name, string countriesGroupId, string slug)
    {
        CountryId = Convert.ToString(Ulid.NewUlid());
        Name = name;
        CountriesGroupId = countriesGroupId;
        Slug = slug.ToLower();
        Videos = new HashSet<VideoCounty>();
        Subscribes = new HashSet<SubscribeCountry>();
    }

    public string CountryId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string CountriesGroupId { get; set; }
    public CountriesGroup CountriesGroup { get; set; }
    public ICollection<VideoCounty> Videos { get; set; }
    public ICollection<SubscribeCountry> Subscribes { get; set; }
}