namespace StreamAppApi.Contracts.Models;

public class Crew
{
    public string PersonId { get; set; }
    public Person Person { get; set; }

    public string VideoId { get; set; }
    public Video Video { get; set; }

    public string RoleId { get; set; }
    public Role Role { get; set; }
}