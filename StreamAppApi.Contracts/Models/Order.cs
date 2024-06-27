using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NUlid;

namespace StreamAppApi.Contracts.Models;

public class Order
{
    public Order(
        decimal sum, 
        string userId, 
        string? subscribeId, 
        string? serialId, 
        string? movieId)
    {
        OrderId = Convert.ToString(Ulid.NewUlid());
        OrderDate = DateTime.UtcNow;
        Sum = sum;
        UserId = userId;
        SubscribeId = subscribeId;
        SerialId = serialId;
        MovieId = movieId;
    }

    [Key]
    [Column("id")]
    public string OrderId { get; set; }
    
    [Column("orderDate")]
    public DateTime OrderDate { get; set; }
    
    [Column("sum")]
    public decimal Sum { get; set; }
    
    [Column("userId")]
    public string UserId { get; set; }
    public User User { get; set; }
    
    [Column("subscribeId")]
    public string? SubscribeId { get; set; }
    public Subscribe? Subscribe { get; set; }
    
    [Column("serialId")]
    public string? SerialId { get; set; }
    public Serial? Serial { get; set; }
    
    [Column("movieId")]
    public string? MovieId { get; set; }
    public Video? Movie { get; set; }
}