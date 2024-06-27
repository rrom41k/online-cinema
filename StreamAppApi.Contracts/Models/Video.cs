using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using NUlid;

namespace StreamAppApi.Contracts.Models;

public class Video
{
    public Video(
        byte[] videoUrl,
        byte[] videoUrlIv,
        int year, 
        int duration,
        TypeVideo type,
        bool isSendTelegram)
    {
        VideoId = Convert.ToString(Ulid.NewUlid());
        VideoUrl = videoUrl;
        VideoUrlIv = videoUrlIv;
        Year = year;
        Duration = duration;
        Type = type;
        IsSendTelegram = isSendTelegram;
        Favorites = new HashSet<Favorite>();
        Comments = new HashSet<Comment>();
        Genres = new HashSet<VideoGenre>();
        Crew = new HashSet<Crew>();
        Ratings = new HashSet<Rating>();
        Countries = new HashSet<VideoCounty>();
        Orders = new HashSet<Order>();
    }

    [Key]
    [Column("id")]
    public string VideoId { get; set; }
    
    [Column("videoUrl")]
    public byte[] VideoUrl { get; set; }
    
    [Column("videoUrlIv")]
    public byte[] VideoUrlIv { get; set; }

    [Column("year")]
    public int Year { get; set; }

    [Column("duration")]
    public int Duration { get; set; }

    [Column("type")]
    public TypeVideo Type { get; set; }

    [Column("isSendTelegram")]
    public bool IsSendTelegram { get; set; }
    
    
    public Movie? Movie { get; set; }
    [Column("movieId")]
    public string? MovieId { get; set; }
    public Season? Season { get; set; }
    [Column("episodeNumber")]
    public int? EpisodeNumber { get; set; }
    [Column("seasonId")]
    public string? SeasonId { get; set; }

    public ICollection<VideoCounty> Countries { get; set; }
    public ICollection<Crew> Crew { get; set; }
    public ICollection<VideoGenre> Genres { get; set; }
    
    //UserVideo
    public ICollection<Favorite> Favorites { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public ICollection<Rating> Ratings { get; set; }
    
    public ICollection<Order> Orders { get; set; }
    
}

public enum TypeVideo
{
    Movie,
    Serial
}