namespace StreamAppApi.Contracts.Models;

public class SubscribePerson
{
    public string SubscribeId { get; set; }
    public Subscribe Subscribe { get; set; }

    public string PersonId { get; set; }
    public Person Person { get; set; }
}