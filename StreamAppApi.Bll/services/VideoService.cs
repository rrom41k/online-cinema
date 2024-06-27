using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Models;
using System.Security.Cryptography;

using Microsoft.EntityFrameworkCore;

using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Commands.CrewCommands;

namespace StreamAppApi.Bll;

public class VideoService
{
    private readonly StreamPlatformDbContext _dbContext;
    
    public VideoService(StreamPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public static List<VideoDto> MapVideosToVideoDtos(List<Video> videos, byte[] key)
    {
        List<VideoDto> videoDtos = new();

        foreach (var video in videos)
        {
            videoDtos.Add(new (
                video.VideoId,
                DecryptStringFromBytes_Aes(video.VideoUrl, key, video.VideoUrlIv),
                video.Year,
                video.Duration));
        }
        
        return videoDtos;
    }
    
    public List<Crew> UpdateVideoCrew(CrewUpdateCommand[] persons, Video video)
    {
        List<Crew> crewUpdate = new();
        // Удаляем всех актёров, обновляемого фильма
        _dbContext.Crews.RemoveRange( _dbContext.Crews.Where(crew => crew.VideoId == video.VideoId).ToList());
        
        // Получаем список команды
        foreach (var person in persons)
        {
            var newCrew = new Crew
            {
                Video = video,
                
                Person = _dbContext.Persons.FirstOrDefault(p => p.PersonId.Contains(person.personId))
                         ?? throw new ArgumentException("Not found Person"),
                
                Role = _dbContext.Roles.FirstOrDefault(p => p.RoleId.Contains(person.roleId))
                       ?? throw new ArgumentException("Not found Role with Id: " + person.roleId),
            };
            _dbContext.Crews.Add(newCrew);
            crewUpdate.Add(newCrew);
        }
        
        _dbContext.Crews.AddRange(crewUpdate);
        
        return crewUpdate;
    }
    
    public List<SubscribePerson> UpdateSubscribePerson(string[] persons, string subscribeId)
    {
        List<SubscribePerson> crewUpdate = new();
        // Удаляем всех актёров, обновляемого фильма
        _dbContext.SubscribePersons.RemoveRange( _dbContext.SubscribePersons.Where(sp => sp.SubscribeId == subscribeId).ToList());
        
        // Получаем список команды
        foreach (var person in persons)
        {
            var newSubscribePerson = new SubscribePerson
            {
                SubscribeId = subscribeId,
                
                PersonId = person,
                
                Person = _dbContext.Persons.FirstOrDefault(p => p.PersonId.Contains(person))
                         ?? throw new ArgumentException("Not found Person")
            };
            _dbContext.SubscribePersons.Add(newSubscribePerson);
            crewUpdate.Add(newSubscribePerson);
        }
        
        _dbContext.SubscribePersons.AddRange(crewUpdate);
        
        return crewUpdate;
    }
    
    public List<VideoGenre> UpdateVideoGenres(string[] genresIds, string videoId)
    {
        List<VideoGenre> genresToUpdate = new();
        
        // Удаляем все жанры, обновляемого фильма
        _dbContext.VideoGenres.RemoveRange(
            _dbContext.VideoGenres.Where(video => video.VideoId == videoId).ToList());
        
        // Получаем список жанров
        var genres = _dbContext.Genres.Where(genre => genresIds.Contains(genre.GenreId)).ToList();
        
        // Добавляем новые жанры
        foreach (var genre in genres)
            genresToUpdate.Add(new() { GenreId = genre.GenreId, VideoId = videoId, Genre = genre });
        
        _dbContext.VideoGenres.AddRange(genresToUpdate);
        
        return genresToUpdate;
    }
    
    public List<SubscribeGenre> UpdateSubscribeGenre(string[] genresIds, string subscribeId)
    {
        List<SubscribeGenre> genresToUpdate = new();
        
        // Удаляем все жанры, обновляемого фильма
        _dbContext.SubscribeGenres.RemoveRange(
            _dbContext.SubscribeGenres.Where(sg => sg.SubscribeId == subscribeId).ToList());
        
        // Получаем список жанров
        var genres = _dbContext.Genres.Where(genre => genresIds.Contains(genre.GenreId)).ToList();
        
        // Добавляем новые жанры
        foreach (var genre in genres)
            genresToUpdate.Add(new() { GenreId = genre.GenreId, SubscribeId = subscribeId, Genre = genre });
        
        _dbContext.SubscribeGenres.AddRange(genresToUpdate);
        
        return genresToUpdate;
    }
    
    public List<VideoCounty> UpdateVideoCountries(string[] countriesIds, string videoId)
    {
        List<VideoCounty> countriesToUpdate = new();
        
        // Удаляем все регионы, обновляемого фильма
        _dbContext.VideoCountries.RemoveRange( 
            _dbContext.VideoCountries.Where(video => video.VideoId == videoId).ToList());
        
        // Получаем список регионов
        var countries = _dbContext.Countries.Where(country => countriesIds.Contains(country.CountryId)).ToList();
        
        // Добавляем новые регионы
        foreach (var country in countries)
            countriesToUpdate.Add(new() { CountryId = country.CountryId, VideoId = videoId, Country = country });
        
        _dbContext.VideoCountries.AddRange(countriesToUpdate);

        return countriesToUpdate;
    }
    
    public List<SubscribeCountry> UpdateSubscribeCountries(string[] countriesIds, string subscribeId)
    {
        List<SubscribeCountry> countriesToUpdate = new();
        
        // Удаляем все регионы, обновляемого фильма
        _dbContext.SubscribeCountries.RemoveRange( 
            _dbContext.SubscribeCountries.Where(sc => sc.SubscribeId == subscribeId).ToList());
        
        // Получаем список регионов
        var countries = _dbContext.Countries.Where(country => countriesIds.Contains(country.CountryId)).ToList();
        
        // Добавляем новые регионы
        foreach (var country in countries)
            countriesToUpdate.Add(new() { CountryId = country.CountryId, SubscribeId = subscribeId, Country = country });
        
        _dbContext.SubscribeCountries.AddRange(countriesToUpdate);

        return countriesToUpdate;
    }
    
    public bool ContainsInOrdersVideo(string userId, string movieId)
    {
        bool result = false;
        
        var orders = _dbContext.Orders.Include(order => order.Subscribe).Where(order => order.UserId == userId);
        
        // Проверяем покал ли пользователь фильм
        result = orders.Select(order => order.MovieId).Contains(movieId);
        if (result)
            return result;
        
        // Проверяем нахождение фильма в одной из приобретённых подписок
        var userSubscribes = _dbContext.Subscribes
            // Include genres -> videos
            .Include(subscribe => subscribe.Genres)
                .ThenInclude(sg => sg.Genre)
                    .ThenInclude(genre => genre.Videos)
            // Include counties -> videos
            .Include(subscribe => subscribe.Countries)
                .ThenInclude(sc => sc.Country)
                    .ThenInclude(county => county.Videos)
            // Include persons -> videos
            .Include(subscribe => subscribe.Persons)
                .ThenInclude(sp => sp.Person)
                    .ThenInclude(person => person.Crews)
                        .ThenInclude(crew => crew.Video)
            // Include orders -> videos
            .Include(subscribe => subscribe.Orders)
            .Where(subscribe => subscribe.Orders.Select(order => order.UserId).Contains(userId));
        
        // Есть ли у фильма подписка, жанр 
        var videoSG = _dbContext.VideoGenres.Where(vg => vg.VideoId == movieId).Select(vg => vg.GenreId); // получаем id жанров фильма
        var subscribesGenres = _dbContext.SubscribeGenres.Where(sc => videoSG.Contains(sc.GenreId)).Select(sg => sg.SubscribeId); // получаем подписки, которые включают в себя хоть один из жанров
        if (orders.Any(order => subscribesGenres.Contains(order.SubscribeId) && order.OrderDate <= order.OrderDate.AddMonths(order.Subscribe.Duration)))
            return true;
        
        var videoSC = _dbContext.VideoCountries.Where(vg => vg.VideoId == movieId).Select(vg => vg.CountryId); // получаем  id жанров фильма
        var subscribesCountries = _dbContext.SubscribeCountries.Where(sc => videoSC.Contains(sc.CountryId)).Select(sg => sg.SubscribeId); // получаем подписки, которые включают в себя хоть один из жанров
        if (orders.Any(order => subscribesCountries.Contains(order.SubscribeId) && order.OrderDate <= order.OrderDate.AddMonths(order.Subscribe.Duration)))
            return true;
        
        var videoCrew = _dbContext.Crews.Where(crew => crew.VideoId == movieId).Select(crew => crew.PersonId); // получаем  id жанров фильма
        var subscribesPersons = _dbContext.SubscribePersons.Where(sp => videoCrew.Contains(sp.PersonId)).Select(sg => sg.SubscribeId); // получаем подписки, которые включают в себя хоть один из жанров
        if (orders.Any(order => subscribesPersons.Contains(order.SubscribeId) && order.OrderDate <= order.OrderDate.AddMonths(order.Subscribe.Duration)))
            return true;
        
        return result;
    }

    public static byte[] EncryptStringToBytes_Aes(string path, byte[] key, out byte[] iv)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentNullException(nameof(path));
        
        // Создаем ключ и вектор инициализации
        using var aes = Aes.Create();
        iv = aes.IV;
        
        ICryptoTransform encryptor = aes.CreateEncryptor(key, iv);

        using var msEncrypt = new MemoryStream();
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(path);
        }
        return msEncrypt.ToArray();
    }

    public static string DecryptStringFromBytes_Aes(byte[] encryptedPath, byte[] key, byte[] iv)
    {
        if (encryptedPath == null || encryptedPath.Length <= 0)
            throw new ArgumentNullException(nameof(encryptedPath));
        if (key == null || key.Length <= 0)
            throw new ArgumentNullException(nameof(key));
        if (iv == null || iv.Length <= 0)
            throw new ArgumentNullException(nameof(iv));

        using var aesAlg = Aes.Create();
        aesAlg.Key = key;
        aesAlg.IV = iv;

        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using var msDecrypt = new MemoryStream(encryptedPath);
        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
        using (var srDecrypt = new StreamReader(csDecrypt))
        {
            return srDecrypt.ReadToEnd();
        }
    }
}