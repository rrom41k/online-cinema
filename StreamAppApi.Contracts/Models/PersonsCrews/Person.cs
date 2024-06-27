using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using NUlid;

namespace StreamAppApi.Contracts.Models;

public class Person
{
    public Person(string name, string surname, string slug, string? patronymic, byte[]? photo, byte[]? photoIv)
    {
        PersonId = Convert.ToString(Ulid.NewUlid());
        Name = name;
        Surname = surname;
        Patronymic = patronymic;
        Slug = slug.ToLower();
        Photo = photo;
        PhotoIv = photoIv;
        Crews = new HashSet<Crew>();
        Subscribes = new HashSet<SubscribePerson>();
    }

    [Key]
    [Column("id")]
    public string PersonId { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("surname")]
    public string Surname { get; set; }

    [Column("patronymic")]
    public string? Patronymic { get; set; }

    [Column("slug")]
    public string Slug { get; set; }

    [Column("photo")]
    public byte[]? Photo { get; set; }
    [Column("photoIv")]
    public byte[]? PhotoIv { get; set; }

    public ICollection<Crew> Crews { get; set; }
    public ICollection<SubscribePerson> Subscribes { get; set; }
}