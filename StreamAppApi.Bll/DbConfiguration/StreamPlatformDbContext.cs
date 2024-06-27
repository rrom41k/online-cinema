using Microsoft.EntityFrameworkCore;

using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll.DbConfiguration;

public class StreamPlatformDbContext : DbContext //IdentityDbContext<ApplicationUser, IdentityRole<string>, string>
{
    public StreamPlatformDbContext(DbContextOptions<StreamPlatformDbContext> options) : base(options)
    {
    }
    
    public DbSet<Genre> Genres { get; set; }
    public DbSet<VideoGenre> VideoGenres { get; set; }
    /// 
    public DbSet<Person> Persons { get; set; }
    public DbSet<Crew> Crews { get; set; }
    public DbSet<Role> Roles { get; set; }
    /// 
    public DbSet<CountriesGroup> CountriesGroups { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<VideoCounty> VideoCountries { get; set; }
    /// 
    public DbSet<Season> Seasons { get; set; }
    public DbSet<Serial> Serials { get; set; }
    /// 
    public DbSet<Subscribe> Subscribes { get; set; }
    public DbSet<SubscribeCountry> SubscribeCountries { get; set; }
    public DbSet<SubscribeGenre> SubscribeGenres { get; set; }
    public DbSet<SubscribePerson> SubscribePersons { get; set; }
    /// 
    public DbSet<User> Users { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    /// 
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Video> Videos { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Genre>(
            entity =>
            {
                entity.ToTable("Genres");
                entity.HasIndex(genre => genre.Slug).IsUnique();
            });
        // Многие ко многим Genre & Video
        modelBuilder.Entity<VideoGenre>(
            entity =>
            {
                entity.ToTable("VideoGenres");
                entity.HasKey(videoGenres => new { videoGenres.GenreId, videoGenres.VideoId });
            });
        
        modelBuilder.Entity<VideoGenre>(
            entity =>
            {
                entity
                    .HasOne(videoGenre => videoGenre.Genre)
                    .WithMany(genre => genre.Videos)
                    .HasForeignKey(videoGenre => videoGenre.GenreId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<VideoGenre>(
            entity =>
            {
                entity
                    .HasOne(videoGenre => videoGenre.Video)
                    .WithMany(movie => movie.Genres)
                    .HasForeignKey(genreMovie => genreMovie.VideoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        
        ///////////////////////////////////////////
        
        modelBuilder.Entity<Person>(
            entity =>
            {
                entity.ToTable("Persons");
                entity.HasIndex(actor => actor.Slug).IsUnique();
            });
        // Многие ко многим Person & Video & Role
        modelBuilder.Entity<Crew>(
            entity =>
            {
                entity.ToTable("Crews");
                entity.HasKey(crew => new { crew.PersonId, crew.VideoId, crew.RoleId });
            });

        modelBuilder.Entity<Crew>(
            entity =>
            {
                entity
                    .HasOne(crew => crew.Person)
                    .WithMany(person => person.Crews)
                    .HasForeignKey(crew => crew.PersonId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<Crew>(
            entity =>
            {
                entity
                    .HasOne(crew => crew.Video)
                    .WithMany(video => video.Crew)
                    .HasForeignKey(crew => crew.VideoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<Crew>(
            entity =>
            {
                entity
                    .HasOne(crew => crew.Role)
                    .WithMany(role => role.Crew)
                    .HasForeignKey(crew => crew.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        
        ///////////////////////////////////////////
        
        modelBuilder.Entity<CountriesGroup>(
            entity =>
            {
                entity.ToTable("CountriesGroups");
                entity.HasIndex(countriesGroup => countriesGroup.Name).IsUnique();
            });

        modelBuilder.Entity<Country>(
            entity =>
            {
                entity.ToTable("Countries");
                entity.HasIndex(country => country.Name).IsUnique();
                entity.HasIndex(actor => actor.Slug).IsUnique();
                entity
                    .HasOne(country => country.CountriesGroup)
                    .WithMany(countriesGroup => countriesGroup.Countries)
                    .HasForeignKey(country => country.CountriesGroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        // Многие ко многим Country & Video
        modelBuilder.Entity<VideoCounty>(
            entity =>
            {
                entity.ToTable("VideoCountries");
                entity.HasKey(videoCountry => new { videoCountry.CountryId, videoCountry.VideoId });
            });
        
        modelBuilder.Entity<VideoCounty>(
            entity =>
            {
                entity
                    .HasOne(videoCountry => videoCountry.Country)
                    .WithMany(country => country.Videos)
                    .HasForeignKey(vc => vc.CountryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<VideoCounty>(
            entity =>
            {
                entity
                    .HasOne(videoCountry => videoCountry.Video)
                    .WithMany(video => video.Countries)
                    .HasForeignKey(vc => vc.VideoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        
        ///////////////////////////////////////////
        
        modelBuilder.Entity<Serial>(
            entity =>
            {
                entity.ToTable("Serials");
                entity.HasIndex(serial => serial.Slug).IsUnique();
            });
        
        modelBuilder.Entity<Season>(
            entity =>
            {
                entity.ToTable("Seasons");
                entity.HasKey(season => season.SeasonId);
            });
        modelBuilder.Entity<Season>(
            entity =>
            {
                entity
                    .HasOne(season => season.Serial)
                    .WithMany(serial => serial.Seasons)
                    .HasForeignKey(season => season.SerialId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        
        /////////////////////////////////////////////
        
        modelBuilder.Entity<Subscribe>(
            entity =>
            {
                entity.ToTable("Subscribes");
            });
        // Многие ко многим Subscribe & Country
        modelBuilder.Entity<SubscribeCountry>(
            entity =>
            {
                entity.ToTable("SubscribeCountries");
                entity.HasKey(subscribeCountry => new { subscribeCountry.SubscribeId, subscribeCountry.CountryId });
            });
        
        modelBuilder.Entity<SubscribeCountry>(
            entity =>
            {
                entity
                    .HasOne(subscribeCountry => subscribeCountry.Subscribe)
                    .WithMany(subscribe => subscribe.Countries)
                    .HasForeignKey(subscribeCountry => subscribeCountry.SubscribeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<SubscribeCountry>(
            entity =>
            {
                entity
                    .HasOne(subscribeCountry => subscribeCountry.Country)
                    .WithMany(subscribe => subscribe.Subscribes)
                    .HasForeignKey(subscribeCountry => subscribeCountry.CountryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        // Многие ко многим Subscribe & Genre
        modelBuilder.Entity<SubscribeGenre>(
            entity =>
            {
                entity.ToTable("SubscribeGenres");
                entity.HasKey(subscribeGenre => new { subscribeGenre.SubscribeId, subscribeGenre.GenreId });
            });
        
        modelBuilder.Entity<SubscribeGenre>(
            entity =>
            {
                entity
                    .HasOne(subscribeGenre => subscribeGenre.Subscribe)
                    .WithMany(subscribe => subscribe.Genres)
                    .HasForeignKey(subscribeGenre => subscribeGenre.SubscribeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<SubscribeGenre>(
            entity =>
            {
                entity
                    .HasOne(subscribeGenre => subscribeGenre.Genre)
                    .WithMany(subscribe => subscribe.Subscribes)
                    .HasForeignKey(subscribeGenre => subscribeGenre.GenreId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        // Многие ко многим Subscribe & Person
        modelBuilder.Entity<SubscribePerson>(
            entity =>
            {
                entity.ToTable("SubscribePersons");
                entity.HasKey(subscribePerson => new { subscribePerson.SubscribeId, subscribePerson.PersonId });
            });
        
        modelBuilder.Entity<SubscribePerson>(
            entity =>
            {
                entity
                    .HasOne(subscribePerson => subscribePerson.Subscribe)
                    .WithMany(subscribe => subscribe.Persons)
                    .HasForeignKey(subscribePerson => subscribePerson.SubscribeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<SubscribePerson>(
            entity =>
            {
                entity
                    .HasOne(subscribePerson => subscribePerson.Person)
                    .WithMany(subscribe => subscribe.Subscribes)
                    .HasForeignKey(subscribePerson => subscribePerson.PersonId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        
        ///////////////////////////////////////////
        
        modelBuilder.Entity<User>(
            entity =>
            {
                entity.ToTable("Users");
                entity.HasIndex(user => user.Login).IsUnique();
                entity.HasIndex(user => user.Email).IsUnique();
                entity.HasIndex(user => user.Phone).IsUnique();
            });
        // Многие ко многим Favorite
        modelBuilder.Entity<Favorite>(
            entity =>
            {
                entity.ToTable("Favorites");
                entity.HasKey(favorite => new { favorite.UserId, favorite.VideoId });
            });

        modelBuilder.Entity<Favorite>(
            entity =>
            {
                entity
                    .HasOne(favorite => favorite.User)
                    .WithMany(user => user.Favorites)
                    .HasForeignKey(favorite => favorite.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<Favorite>(
            entity =>
            {
                entity
                    .HasOne(favorite => favorite.Video)
                    .WithMany(video => video.Favorites)
                    .HasForeignKey(favorite => favorite.VideoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        // Многие ко многим Comments
        modelBuilder.Entity<Comment>(
            entity =>
            {
                entity.ToTable("Comments");
                entity.HasKey(comment => new { comment.UserId, comment.VideoId });
            });

        modelBuilder.Entity<Comment>(
            entity =>
            {
                entity
                    .HasOne(comment => comment.User)
                    .WithMany(video => video.Comments)
                    .HasForeignKey(comment => comment.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<Comment>(
            entity =>
            {
                entity
                    .HasOne(comment => comment.Video)
                    .WithMany(video => video.Comments)
                    .HasForeignKey(comment => comment.VideoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        // Многие ко многим Ratings
        modelBuilder.Entity<Rating>(
            entity =>
            {
                entity.ToTable("Ratings");
                entity.HasKey(rating => new { rating.UserId, rating.VideoId });
            });

        modelBuilder.Entity<Rating>(
            entity =>
            {
                entity
                    .HasOne(rating => rating.User)
                    .WithMany(video => video.Ratings)
                    .HasForeignKey(rating => rating.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<Rating>(
            entity =>
            {
                entity
                    .HasOne(rating => rating.Video)
                    .WithMany(video => video.Ratings)
                    .HasForeignKey(rating => rating.VideoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        
        ///////////////////////////
        
        modelBuilder.Entity<Order>(
            entity =>
            {
                entity.ToTable("Orders");
                
                entity
                    .HasOne(order => order.User)
                    .WithMany(user => user.Orders)
                    .HasForeignKey(order => order.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity
                    .HasOne(order => order.Subscribe)
                    .WithMany(subscribe => subscribe.Orders)
                    .HasForeignKey(order => order.SubscribeId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity
                    .HasOne(order => order.Serial)
                    .WithMany(serial => serial.Orders)
                    .HasForeignKey(order => order.SerialId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity
                    .HasOne(order => order.Movie)
                    .WithMany(movie => movie.Orders)
                    .HasForeignKey(order => order.MovieId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        
        modelBuilder.Entity<Movie>(
            entity =>
            {
                entity.ToTable("Movies");
                entity.HasIndex(actor => actor.Slug).IsUnique();
                entity
                    .HasOne(m => m.Video)
                    .WithOne(v => v.Movie)
                    .HasForeignKey<Video>(v => v.MovieId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        
        modelBuilder.Entity<Video>(
            entity =>
            {
                entity.ToTable("Videos");
            });
        
        modelBuilder.Entity<Video>(
            entity =>
            {
                entity
                    .HasOne(season => season.Season)
                    .WithMany(season => season.Videos)
                    .HasForeignKey(video => video.SeasonId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
    }
}