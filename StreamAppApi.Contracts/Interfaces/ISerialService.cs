using Microsoft.AspNetCore.Http;

using StreamAppApi.Contracts.Commands.SerialCommands;
using StreamAppApi.Contracts.Commands;
using StreamAppApi.Contracts.Dto;

namespace StreamAppApi.Contracts.Interfaces;

public interface ISerialService
{
    Task<List<SerialDto>> GetAllSerials(string userId, CancellationToken cancellationToken);
    Task<SerialDto> GetSerialBySlug(string userId, string slug, CancellationToken cancellationToken);

    Task<List<SerialDto>> GetSerialByGenres(
        string userId,
        List<string> genreId,
        CancellationToken cancellationToken);

    Task<List<SerialDto>> GetSerialByPersons(
        string userId, 
        List<string> personId,
        CancellationToken cancellationToken);

    Task UpdateCountOpenedAsync(
        string serial,
        CancellationToken cancellationToken);

    Task<List<SerialDto>> GetMostPopularSerialAsync(string userId, CancellationToken cancellationToken);
    
    Task<OrderDto> BuySerialById(string userId, string serialId, CancellationToken cancellationToken);
    Task<OnlySeasonDto> GetSeasonBySlug(string userId, string slug, int season, CancellationToken cancellationToken);
    Task<OnlyEpisodeDto> GetEpisodeById(string userId, string slug, int season, string episodeId, CancellationToken cancellationToken);

    /* Admin Rights */

    Task<SerialDto> CreateSerial(
        SerialCreateCommand serialCreateCommand,
        CancellationToken cancellationToken);
    
    Task<List<SerialDto>> BatchCreateSerials(IFormFileCollection files, CancellationToken cancellationToken);

    Task<SerialDto> GetSerialById(
        string serialId,
        CancellationToken cancellationToken);

    Task<SerialDto> UpdateSerial(
        string serialId,
        SerialUpdateCommand serialUpdateCommand,
        CancellationToken cancellationToken);

    Task DeleteSerial(
        string serialId,
        CancellationToken cancellationToken);
    
    Task<OnlySeasonDto> CreateSeason(OnlySeasonCreateCommand seasonCreateCommand, CancellationToken cancellationToken);
    
    Task<OnlySeasonDto> GetSeasonById(string serialId, CancellationToken cancellationToken);
    
    Task<OnlySeasonDto> UpdateSeason(
        string seasonId,
        SeasonUpdateCommand seasonUpdateCommand,
        CancellationToken cancellationToken);
    
    Task DeleteSeason(string seasonId, CancellationToken cancellationToken);
    
    Task<OnlyEpisodeDto> CreateEpisode(OnlyEpisodeCreateCommand episodeCreateCommand, CancellationToken cancellationToken);
    
    Task<OnlyEpisodeDto> UpdateEpisode(
        string videoId, EpisodeUpdateCommand episodeUpdateCommand, 
        CancellationToken cancellationToken);

    Task DeleteEpisode(
        string videoId, CancellationToken cancellationToken);
}