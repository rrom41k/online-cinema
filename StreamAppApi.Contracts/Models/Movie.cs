using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using NUlid;

namespace StreamAppApi.Contracts.Models;

public class Movie
{
    public Movie(
        string title,
        string slug,
        byte[] poster,
        byte[] posterIv,
        byte[] bigPoster,
        byte[] bigPosterIv,
        bool needSubscribe,
        decimal? price)
    {
        MovieId = Convert.ToString(Ulid.NewUlid());
        Title = title;
        Slug = slug.ToLower();
        Poster = poster;
        PosterIv = posterIv;
        BigPoster = bigPoster;
        BigPosterIv = bigPosterIv;
        NeedSubscribe = needSubscribe;
        Price = price;
        Rating = 0;
        CountOpened = 0;
    }

    [Key]
    [Column("id")]
    public string MovieId { get; set; }
    
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
    
    [Column("needSubscribe")]
    public bool NeedSubscribe { get; set; }
    
    [Column("price")]
    public decimal? Price { get; set; } // decimal потому что важна дробная часть

    [Column("rating")]
    public double Rating { get; set; } = 0;

    [Column("countOpened")]
    public int CountOpened { get; set; }
    
    public Video Video { get; set; }
}