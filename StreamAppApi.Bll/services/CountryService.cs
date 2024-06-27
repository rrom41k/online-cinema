using Microsoft.EntityFrameworkCore;
using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll;

public class CountryService
{
    private readonly StreamPlatformDbContext _dbContext;

    public CountryService(StreamPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public  static List<CountryDto> MapCountiesToCountryDto(ICollection<SubscribeCountry> countries)
    {
        var listCountries = new List<CountryDto>();

        foreach (var sc in countries)
        {
            CountryDto newCountryDto = new(
                sc.CountryId,
                sc.Country.Name,
                sc.Country.Slug);
            
            listCountries.Add(newCountryDto);
        }

        return listCountries;
    }
    public  static List<CountryDto> MapCountiesToCountryDto(ICollection<VideoCounty> countries)
    {
        var listCountries = new List<CountryDto>();

        foreach (var sc in countries)
        {
            CountryDto newCountryDto = new(
                sc.CountryId,
                sc.Country.Name,
                sc.Country.Slug);
            
            listCountries.Add(newCountryDto);
        }

        return listCountries;
    }
    public static List<VideoCounty> MapCountiesIdsToVideoCountryList(Video newVideo, string[] countries)
    {
        var listCounties = new List<VideoCounty>();

        foreach (var countryId in countries)
        {
            VideoCounty newVideoCounty = new VideoCounty
            {
                VideoId = newVideo.VideoId, 
                CountryId = countryId
            };
            
            listCounties.Add(newVideoCounty);
        }

        return listCounties;
    }
    public static List<SubscribeCountry> MapCountiesIdsToVideoCountryList(Subscribe subscribe, string[] countries)
    {
        var listCounties = new List<SubscribeCountry>();

        foreach (var countryId in countries)
        {
            var newSubscribeCountry = new SubscribeCountry
            {
                SubscribeId = subscribe.SubscribeId, 
                CountryId = countryId
            };
            
            listCounties.Add(newSubscribeCountry);
        }

        return listCounties;
    }
}