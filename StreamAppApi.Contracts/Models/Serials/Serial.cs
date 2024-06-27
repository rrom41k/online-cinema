using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NUlid;

namespace StreamAppApi.Contracts.Models;

public class Serial
{
    public Serial(
        string title,
        string slug,
        byte[] poster,
        byte[] posterIv,
        byte[] bigPoster,
        byte[] bigPosterIv,
        bool? needSubscribe,
        decimal? price)
    {
        SerialId = Convert.ToString(Ulid.NewUlid());
        Title = title;
        Slug = slug.ToLower();
        Poster = poster;
        PosterIv = posterIv;
        BigPoster = bigPoster;
        BigPosterIv = bigPosterIv;
        NeedSubscribe = needSubscribe ?? false;
        Price = price;
    }

    [Key]
    [Column("id")]
    public string SerialId { get; set; }
    
    [Column("title")]
    public string Title { get; set; }
    
    [Column("slug")]
    public string Slug { get; set; }
    
    [Column("poster")]
    public byte[] Poster { get; set; }
    [Column("posterIv")]
    public byte[] PosterIv { get; set; }

    [Column("bigPoster")]
    public byte[] BigPoster { get; set; }
    [Column("bigPosterIv")]
    public byte[] BigPosterIv { get; set; }
    
    [Column("price")]
    public decimal? Price { get; set; } // decimal потому что важна дробная часть

    [Column("rating")]
    public double Rating { get; set; } = 0;

    [Column("countOpened")]
    [DefaultValue(0)]
    public int CountOpened { get; set; } = 0;
    
    [Column("needSubscribe")]
    public bool? NeedSubscribe { get; set; }

    public ICollection<Season> Seasons { get; set; }
    public ICollection<Order> Orders { get; set; }
}