using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using NUlid;

namespace StreamAppApi.Contracts.Models;

public class Genre
{
    public Genre(
        string name,
        string slug,
        string? description,
        string icon)
    {
        GenreId = Convert.ToString(Ulid.NewUlid());
        Name = name;
        Slug = slug.ToLower();
        Description = description;
        Icon = icon;
        Videos = new HashSet<VideoGenre>();
        Subscribes = new HashSet<SubscribeGenre>();
    }

    [Key]
    [Column("id")]
    public string GenreId { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("slug")]
    public string Slug { get; set; }

    [Column("description")]
    public string? Description { get; set; } = string.Empty;

    [Column("icon")]
    public string Icon { get; set; }

    public ICollection<VideoGenre> Videos { get; set; }
    public ICollection<SubscribeGenre> Subscribes { get; set; }
}