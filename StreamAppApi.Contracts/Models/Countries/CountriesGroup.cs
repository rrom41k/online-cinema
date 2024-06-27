using NUlid;

namespace StreamAppApi.Contracts.Models;

public class CountriesGroup
{
    public CountriesGroup(string name, string? description)
    {
        CountriesGroupId = Convert.ToString(Ulid.NewUlid());
        Name = name;
        Countries = new HashSet<Country>();
        Description = description;
    }

    public string CountriesGroupId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<Country> Countries { get; set; }
}