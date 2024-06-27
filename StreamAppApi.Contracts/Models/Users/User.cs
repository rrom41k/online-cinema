using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using NUlid;

namespace StreamAppApi.Contracts.Models;

public class User
{
    public User(
        string login,
        string phone,
        string email,
        byte[] passwordHash,
        byte[] passwordSalt,
        bool isAdmin = false)
    {
        UserId = Convert.ToString(Ulid.NewUlid());
        Login = login;
        Phone = phone;
        Email = email;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        IsAdmin = isAdmin;
        Orders = new HashSet<Order>();
        Ratings = new HashSet<Rating>();
        Favorites = new HashSet<Favorite>();
        Comments = new HashSet<Comment>();
    }

    [Key]
    [Column("id")]
    public string UserId { get; set; }
    
    [Column("login")]
    public string Login { get; set; }
    
    [Column("phone")]
    public string Phone { get; set; }

    [EmailAddress]
    [Column("email")]
    public string Email { get; set; }

    [Column("passwordHash")]
    public byte[] PasswordHash { get; set; }

    [Column("passwordSalt")]
    public byte[] PasswordSalt { get; set; }

    [Column("isAdmin")]
    public bool IsAdmin { get; set; }
    
    [Column("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;

    [Column("tokenCreated")]
    public DateTime TokenCreated { get; set; } = DateTime.UtcNow;

    [Column("tokenUpdated")]
    public DateTime TokenExpires { get; set; } = DateTime.UtcNow;
    
    public ICollection<Order> Orders { get; set; }
    public ICollection<Favorite> Favorites { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public ICollection<Rating> Ratings { get; set; }
}