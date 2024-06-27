using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NUlid;

namespace StreamAppApi.Contracts.Models;

public class Role
{
    public Role(string name, string? description)
    {
        RoleId = Convert.ToString(Ulid.NewUlid());
        Name = name;
        Description = description;
        Crew = new HashSet<Crew>();
    }

    [Key]
    [Column("id")]
    public string RoleId { get; set; }
    [Column("name")]
    public string Name { get; set; }
    [Column("description")]
    public string? Description { get; set; }

    public ICollection<Crew> Crew { get; set; }
}