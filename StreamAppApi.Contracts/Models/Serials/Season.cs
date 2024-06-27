using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using NUlid;

namespace StreamAppApi.Contracts.Models;

public class Season
{
    public Season(string serialId, int numberSeason)
    {
        SeasonId = Convert.ToString(Ulid.NewUlid());
        SerialId = serialId;
        NumberSeason = numberSeason;
    }
    
    [Key]
    public string SeasonId { get; set; }
    public string SerialId { get; set; }
    public Serial Serial { get; set; }

    [Column("numberSeason")]
    public int NumberSeason { get; set; }

    public List<Video> Videos { get; set; }
}