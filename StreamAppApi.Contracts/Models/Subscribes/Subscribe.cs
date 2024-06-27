using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NUlid;

namespace StreamAppApi.Contracts.Models;

public class Subscribe
{
    public Subscribe(string name, decimal price, int duration, string? description)
    {
        SubscribeId = Convert.ToString(Ulid.NewUlid());
        Name = name;
        Description = description;
        Price = price;
        Duration = duration;
        Orders = new HashSet<Order>();
        Persons = new HashSet<SubscribePerson>();
        Countries = new HashSet<SubscribeCountry>();
        Genres = new HashSet<SubscribeGenre>();
    }

    [Key]
    [Column("id")]
    public string SubscribeId { get; set; }
    
    [Column("orderDate")]
    public string Name { get; set; }
    
    [Column("description")]
    public string? Description { get; set; }
    
    [Column("price")]
    public decimal Price { get; set; }
    
    [Column("duration")]
    public int Duration { get; set; } // Duration in days

    public ICollection<Order> Orders { get; set; }
    public ICollection<SubscribePerson> Persons { get; set; }
    public ICollection<SubscribeCountry> Countries { get; set; }
    public ICollection<SubscribeGenre> Genres { get; set; }
    
}