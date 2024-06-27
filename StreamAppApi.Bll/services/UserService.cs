using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Commands.UserCommands;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Interfaces;
using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll;

public class UserService : IUserService
{
    private readonly StreamPlatformDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMovieService _movieService;
    private readonly ISerialService _serialService;
    private readonly ISubscribeService _subscribeService;

    public UserService(StreamPlatformDbContext dbContext, IHttpContextAccessor httpContextAccessor, 
        IMovieService movieService,
        ISerialService serialService, ISubscribeService subscribeService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _movieService = movieService;
        _serialService = serialService;
        _subscribeService = subscribeService;
    }

    public async Task<UserDto> CreateUser(UserCreateCommand user, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
        {
            throw new OperationCanceledException();
        }

        if (!IsValidEmail(user.email)) // Валидация полей
        {
            throw new ArgumentException("Invalid email");
        }

        if (user.password.Length < 6) // Валидация полей
        {
            throw new ArgumentException("Invalid password length");
        }

        var userContains = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.email, 
            cancellationToken);
        
        if (userContains != null)
        {
            throw new("User with this email contains in DB");
        }

        userContains = await _dbContext.Users.FirstOrDefaultAsync(u => u.Login == user.login, 
            cancellationToken);
        
        if (userContains != null)
        {
            throw new("User with this login contains in DB");
        }

        CreatePasswordHash(user.password, out var passwordHash, out var passwordSalt);
        User newUser = new(
            user.login,
            user.phone,
            user.email,
            passwordHash,
            passwordSalt,
            user.isAdmin);

        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new (newUser.UserId, newUser.Email, newUser.Login, newUser.IsAdmin, null);
    }

    public async Task<int> GetUsersCount(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var usersCount = await _dbContext.Users.CountAsync(cancellationToken);

        return usersCount;
    }

    public async Task<List<UserDto>> GetAllUsers(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        string? searchTerm = _httpContextAccessor.HttpContext.Request.Query["searchTerm"];

        searchTerm = searchTerm ?? "";
        
        var users = await _dbContext.Users.AsNoTracking()
            .Include(user => user.Orders)
                .Where(user => user.Email.ToLower().Contains(searchTerm.ToLower()) 
                               || user.Login.ToLower().Contains(searchTerm.ToLower()) 
                               || user.Phone.Contains(searchTerm))
                    .ToListAsync(cancellationToken);

        return users.Select(user => UserToDto(user)).ToList();
    }

    public async Task<UserDto> GetUserById(string id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingUser = await _dbContext.Users.AsNoTracking()
                .Include(user => user.Orders)
                .FirstOrDefaultAsync(existingUser => existingUser.UserId == id, cancellationToken)
            ?? throw new ArgumentException("User not found.");

        return UserToDto(existingUser);
    }
    
    public async Task<FavoritesDto> GetFavorites(string userId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var favoriteVideos = await _dbContext.Favorites.AsNoTracking()
            .Include(favorite => favorite.Video)
                .ThenInclude(video => video.Season)
                    .ThenInclude(season => season!.Serial)
            .Include(favorite => favorite.Video)
                .ThenInclude(video => video.Movie)
            .Where(favorite => favorite.UserId == userId)
            .Select(favorite => favorite.Video)
            .ToListAsync(cancellationToken);

        return await FavoritesToDto(userId, favoriteVideos, cancellationToken);
    }

    async Task<FavoritesDto> FavoritesToDto(string userId, List<Video> favorites, CancellationToken cancellationToken)
    {
        FavoritesDto result = new(new(),new());
        foreach (var video in favorites)
        {
            switch (video.Type)
            {
                case TypeVideo.Movie:
                    result.movies.Add(await _movieService.GetMovieById(video.VideoId, cancellationToken));
                    break;
                case TypeVideo.Serial:
                    result.serialEpisodes.Add(
                        await _serialService.GetEpisodeById(
                            userId, 
                            video.Season.Serial.Slug,
                            video.Season.NumberSeason,
                            video.VideoId, 
                            cancellationToken)
                    );
                    break;
                default:
                    throw new Exception($"Undefined type video with id \"{video.VideoId}\"");
            }
        }
        
        return result;
    }

    public async Task UpdateFavorites(
        string userId,
        UserFavoritesUpdateCommand userFavoritesUpdateCommand,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        
        var existingVideo = await _dbContext.Videos
            .FirstOrDefaultAsync(video => video.VideoId == userFavoritesUpdateCommand.videoId, 
                cancellationToken) 
                            ?? throw new NullReferenceException("Did not find movie or serial episode with this Id");
        
        var checkVideo = await _dbContext.Favorites
            .Where(favorite => favorite.UserId == userId)
            .FirstOrDefaultAsync(favorite => favorite.VideoId == existingVideo.VideoId, cancellationToken);

        if (checkVideo != null)
        {
            _dbContext.Favorites.Remove(checkVideo);
        }
        else
        {
            var newFavorVideo = new Favorite { UserId = userId, VideoId = existingVideo.VideoId };
            _dbContext.Favorites.Add(newFavorVideo);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto> UpdateUser(
        string id,
        UserUpdateCommand updateUserData,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var userToUpdate = await _dbContext.Users
            .FirstOrDefaultAsync(userToUpdate => userToUpdate.UserId == id, cancellationToken);

        if (userToUpdate == null)
        {
            throw new ArgumentException("User not found.");
        }

        UpdateUserHelper(ref userToUpdate, updateUserData);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return UserToDto(userToUpdate);
    }

    public async Task<UserDto> DeleteUser(string id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingUser = await _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(existingUser => existingUser.UserId == id, cancellationToken);

        if (existingUser == null)
        {
            throw new ArgumentException("User not fount.");
        }

        var removedUser = UserToDto(existingUser);

        _dbContext.Users.Remove(existingUser);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return removedUser;
    }
    
    // Helpful methods

    private void UpdateUserHelper(ref User user, UserUpdateCommand updateUserData)
    {
        user.Email = string.IsNullOrEmpty(updateUserData.email) ? user.Email : updateUserData.email;
        user.Login = string.IsNullOrEmpty(updateUserData.login) ? user.Login : updateUserData.login;
        user.Phone = string.IsNullOrEmpty(updateUserData.phone)? user.Phone : updateUserData.phone;
        
        if (!string.IsNullOrEmpty(updateUserData.password))
        {
            CreatePasswordHash(updateUserData.password, out var passwordHash, out var passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
        }

        user.IsAdmin = updateUserData.isAdmin ?? user.IsAdmin;
    }

    private UserDto UserToDto(User user)
    {
        return new(
            user.UserId,
            user.Email,
            user.Login,
            user.IsAdmin,
            user.Orders.Select(order => OrderToDto(order)).ToList());
    }

    private UserOrderDto OrderToDto(Order order)
    {
        return new(
            order.OrderId,
            order.OrderDate,
            order.Sum,
            order.SubscribeId,
            order.SerialId,
            order.MovieId);
    }

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            // Normalize the domain
            email = Regex.Replace(
                email,
                @"(@)(.+)$",
                DomainMapper,
                RegexOptions.None,
                TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                var domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException e)
        {
            return false;
        }
        catch (ArgumentException e)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}