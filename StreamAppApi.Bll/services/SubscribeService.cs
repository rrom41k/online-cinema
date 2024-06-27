using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Commands.SubscribeCommands;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Interfaces;
using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll;

public class SubscribeService : ISubscribeService
{
    private readonly StreamPlatformDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly VideoService _videoService;
    
    public SubscribeService(StreamPlatformDbContext dbContext, IConfiguration configuration, VideoService videoService)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _videoService = videoService;
    }
    
    public async Task<SubscribeDto> GetSubscribeById(string id, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var existingSubscribe = await GetAllSubscribesNoTracking()
                .FirstOrDefaultAsync(existingSubscribe => existingSubscribe.SubscribeId == id, cancellationToken)
            ?? throw new ArgumentException("Subscribe not found.");
        
        return SubscribeToDTO(existingSubscribe, Convert.FromBase64String(_configuration["AppSettings:CryptKey"]));
    }
    
    public async Task<List<SubscribeDto>> GetAllSubscribes(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var subscribes = await GetAllSubscribesNoTracking().ToListAsync(cancellationToken);
        
        return subscribes.Select(subscribe=> SubscribeToDTO(subscribe, 
            Convert.FromBase64String(_configuration["AppSettings:CryptKey"]))).ToList();
    }
    
    /* Admin Rights */
    
    public async Task<SubscribeDto> CreateSubscribe(SubscribeCreateCommand subscribeCreateCommand, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var newSubscribe = CreateSubscribeHelper(subscribeCreateCommand, cancellationToken);
        
        await _dbContext.Subscribes.AddAsync(newSubscribe, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return await GetSubscribeById(newSubscribe.SubscribeId, cancellationToken);
    }
    
    public async Task<SubscribeDto> UpdateSubscribe(string id, SubscribeUpdateCommand subscribeUpdateCommand, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var existingSubscribe = await GetAllSubscribes()
                .FirstOrDefaultAsync(existingSubscribe => existingSubscribe.SubscribeId == id, cancellationToken)
            ?? throw new ArgumentException("Subscribe not found.");
        
        UpdateMovieHelper(ref existingSubscribe, subscribeUpdateCommand);
        
        return await GetSubscribeById(existingSubscribe.SubscribeId, cancellationToken);
    }
    
    public async Task DeleteSubscribe(string subscribeId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var existingMovie = await GetAllSubscribes()
            .FirstOrDefaultAsync(subscribe => subscribe.SubscribeId == subscribeId, cancellationToken);
        
        if (existingMovie == null)
        {
            throw new NullReferenceException("Movie not fount.");
        }
        
        _dbContext.Subscribes.Remove(existingMovie);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<OrderDto> BuySubscribeById(string userId, string subscribeId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        if (await _dbContext.Subscribes.FirstOrDefaultAsync(subscribe => subscribe.SubscribeId == subscribeId, cancellationToken) == null)
            throw new("Subscribe don't exist");
        
        if (await _dbContext.Orders.Include(order => order.Subscribe).FirstOrDefaultAsync(
                order => order.UserId == userId 
                    && order.SubscribeId == subscribeId 
                    && order.OrderDate.AddMonths(order.Subscribe.Duration) >= DateTime.UtcNow,
            cancellationToken) != null)
            throw new Exception("You have this active subscribe");
        
        var existingSubscribe = await GetAllSubscribesNoTracking()
                .FirstOrDefaultAsync(
                    existingMovie =>
                        existingMovie.SubscribeId == subscribeId,
                    cancellationToken)
            ?? throw new ArgumentException("Subscribe not found.");
        
        Order newOrder = new(
            existingSubscribe.Price,
            userId,
            subscribeId,
            null,
            null);
        
        await _dbContext.Orders.AddAsync(newOrder, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return new(newOrder.OrderId, newOrder.OrderDate, newOrder.Sum, newOrder.UserId, newOrder.SubscribeId, null, null);
    }

    // Helpful methods

    private IQueryable<Subscribe> GetAllSubscribes()
    {
        return _dbContext.Subscribes
            // Include persons
            .Include(subscribe => subscribe.Persons)
            .ThenInclude(sp => sp.Person)
            // Include counties
            .Include(subscribe => subscribe.Countries)
            .ThenInclude(sc => sc.Country)
            // Include genres
            .Include(subscribe => subscribe.Genres)
            .ThenInclude(sg => sg.Genre);
    }
    
    private  IQueryable<Subscribe> GetAllSubscribesNoTracking()
    {
        return _dbContext.Subscribes.AsNoTracking()
            // Include persons
            .Include(subscribe => subscribe.Persons)
            .ThenInclude(sp => sp.Person)
            // Include counties
            .Include(subscribe => subscribe.Countries)
            .ThenInclude(sc => sc.Country)
            // Include genres
            .Include(subscribe => subscribe.Genres)
            .ThenInclude(sg => sg.Genre);
    }
    
    public static SubscribeDto SubscribeToDTO(Subscribe subscribe, byte[] key)
    {
        return new(
            subscribe.SubscribeId,
            subscribe.Name,
            subscribe.Description,
            subscribe.Price,
            subscribe.Duration,
            PersonService.MapPersonsToPersonDto(subscribe.Persons, key),
            CountryService.MapCountiesToCountryDto(subscribe.Countries),
            GenreService.MapGenresToGenresDto(subscribe.Genres));
    }
    
    private void UpdateMovieHelper(ref Subscribe subscribeToUpdate, SubscribeUpdateCommand subscribeUpdateCommand)
    {
        var subscribeId = subscribeToUpdate.SubscribeId;
        
        subscribeToUpdate.Name = subscribeUpdateCommand.name ?? subscribeToUpdate.Name;
        
        subscribeToUpdate.Description = subscribeUpdateCommand.description ?? subscribeToUpdate.Description;
        
        subscribeToUpdate.Price = subscribeUpdateCommand.price ?? subscribeToUpdate.Price;
        
        subscribeToUpdate.Duration = subscribeUpdateCommand.duration ?? subscribeToUpdate.Duration;
        
        if (subscribeUpdateCommand.genres != null)
        {
            if (subscribeUpdateCommand.genres.Length == 0)
            {
                _dbContext.SubscribeGenres.RemoveRange(_dbContext.SubscribeGenres.Where(sg => sg.SubscribeId == subscribeId));
            }
            else
                subscribeToUpdate.Genres = _videoService.UpdateSubscribeGenre(subscribeUpdateCommand.genres, subscribeToUpdate.SubscribeId);
        }
        
        if (subscribeUpdateCommand.persons != null)
        {
            if (subscribeUpdateCommand.persons.Length == 0)
            {
                _dbContext.SubscribePersons.RemoveRange(_dbContext.SubscribePersons.Where(sp => sp.SubscribeId == subscribeId));
            }
            subscribeToUpdate.Persons = _videoService.UpdateSubscribePerson(subscribeUpdateCommand.persons, subscribeToUpdate.SubscribeId);
        }
        
        if (subscribeUpdateCommand.countries != null)
        {
            if (subscribeUpdateCommand.countries.Length == 0)
            {
                _dbContext.SubscribeCountries.RemoveRange(_dbContext.SubscribeCountries.Where(sc => sc.SubscribeId == subscribeId));
            }
            subscribeToUpdate.Countries = _videoService.UpdateSubscribeCountries(subscribeUpdateCommand.countries, subscribeToUpdate.SubscribeId);
        }

        _dbContext.SaveChanges();
    }
    
    private Subscribe CreateSubscribeHelper(SubscribeCreateCommand subscribeCreateCommand,
        CancellationToken cancellationToken)
    {
        Subscribe newSubscribe = new (
            subscribeCreateCommand.name,
            subscribeCreateCommand.price,
            subscribeCreateCommand.duration,
            subscribeCreateCommand.description);
        
        var genres = GenreService.MapGenresArrToList(newSubscribe, subscribeCreateCommand.genres);
        var persons = PersonService.MapPersonsArrToList(newSubscribe, subscribeCreateCommand.persons);
        var counties =  CountryService.MapCountiesIdsToVideoCountryList(newSubscribe, subscribeCreateCommand.countries);
        
        _dbContext.SubscribeGenres.AddRangeAsync(genres, cancellationToken);
        _dbContext.SubscribePersons.AddRangeAsync(persons, cancellationToken);
        _dbContext.SubscribeCountries.AddRangeAsync(counties, cancellationToken);
        
        newSubscribe.Genres = genres;
        newSubscribe.Persons = persons;
        newSubscribe.Countries = counties;
        
        return newSubscribe;
    }
}