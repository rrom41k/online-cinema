using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Commands.CrewCommands;
using StreamAppApi.Contracts.Commands.PersonCommands;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Interfaces;
using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll;

public class PersonService : IPersonService
{
    private readonly StreamPlatformDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public PersonService(
        StreamPlatformDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public async Task<PersonDto> GetPersonBySlug(string slug, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingPerson = await _dbContext.Persons.AsNoTracking()
                .FirstOrDefaultAsync(existingPerson => existingPerson.Slug == slug, cancellationToken)
            ?? throw new ArgumentException("Person not found.");

        return PersonToDto(existingPerson, Convert.FromBase64String(_configuration["AppSettings:CryptKey"]));
    }

    public async Task<List<PersonDto>> GetAllPersons(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        string? searchTerm = _httpContextAccessor.HttpContext.Request.Query["searchTerm"];

        if (searchTerm == null)
        {
            searchTerm = "";
        }

        var persons = await _dbContext.Persons.AsNoTracking()
            .Where(
                person =>
                    person.Name.ToLower().Contains(searchTerm.ToLower())
                    || person.Slug.Contains(searchTerm.ToLower()))
            .ToListAsync(cancellationToken);

        return MapPersonsToPersonDto(persons, Convert.FromBase64String(_configuration["AppSettings:CryptKey"]));
    }

    /* Admin Rights */

    public async Task<PersonDto> CreatePerson(PersonCreateCommand personCreateCommand, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
        {
            throw new OperationCanceledException();
        }

        var findPerson = await _dbContext.Persons.FirstOrDefaultAsync(
            person =>
                person.Slug == personCreateCommand.slug.ToLower());

        if (findPerson != null)
        {
            throw new("Person with this slug contains in DB");
        }
        
        Person newPerson = new(
            personCreateCommand.name, 
            personCreateCommand.surname, 
            personCreateCommand.slug,
            personCreateCommand.patronymic,
            VideoService.EncryptStringToBytes_Aes(personCreateCommand.photo, 
                Convert.FromBase64String(_configuration["AppSettings:CryptKey"]), out byte[] urlPhotoIv),
            urlPhotoIv);

        _dbContext.Persons.Add(newPerson);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return PersonToDto(newPerson, Convert.FromBase64String(_configuration["AppSettings:CryptKey"]));
    }

    public async Task<PersonDto> GetPersonById(string id, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingPerson = await _dbContext.Persons.AsNoTracking()
                .FirstOrDefaultAsync(existingPerson => existingPerson.PersonId == id, cancellationToken)
            ?? throw new ArgumentException("Person not found.");

        return PersonToDto(existingPerson, Convert.FromBase64String(_configuration["AppSettings:CryptKey"]));
    }

    public async Task<PersonDto> UpdatePerson(
        string id,
        PersonUpdateCommand personUpdateCommand,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var personToUpdate = await _dbContext.Persons
            .FirstOrDefaultAsync(personToUpdate => personToUpdate.PersonId == id, cancellationToken);

        if (personToUpdate == null)
        {
            throw new ArgumentException("Person not found.");
        }

        UpdatePersonHelper(ref personToUpdate, personUpdateCommand);
        await _dbContext.SaveChangesAsync(cancellationToken);


        return PersonToDto(personToUpdate, Convert.FromBase64String(_configuration["AppSettings:CryptKey"]));
    }

    public async Task<PersonDto> DeletePerson(string id, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingPerson = await _dbContext.Persons.AsNoTracking()
            .FirstOrDefaultAsync(existingPerson => existingPerson.PersonId == id, cancellationToken);

        if (existingPerson == null)
        {
            throw new ArgumentException("Person not fount.");
        }

        _dbContext.Persons.Remove(existingPerson);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return PersonToDto(existingPerson, Convert.FromBase64String(_configuration["AppSettings:CryptKey"]));
    }

    private void UpdatePersonHelper(ref Person personToUpdate, PersonUpdateCommand personUpdateCommand)
    {
        personToUpdate.Name = personUpdateCommand.name ?? personToUpdate.Name;
        personToUpdate.Surname = personUpdateCommand.surname ?? personToUpdate.Surname;
        personToUpdate.Patronymic = personUpdateCommand.patronymic ?? personToUpdate.Patronymic;
        personToUpdate.Slug = personUpdateCommand.slug?.ToLower() ?? personToUpdate.Slug;
        
        if (string.IsNullOrEmpty(personUpdateCommand.photo))
        {
            personToUpdate.Photo = VideoService.EncryptStringToBytes_Aes(
                _configuration["AppSettings:EmptyPhoto"],
                Convert.FromBase64String(_configuration["AppSettings:CryptKey"]),
                out byte[] urlPhotoIv);
            
            personToUpdate.PhotoIv = urlPhotoIv;
        }
        else
        {
            personToUpdate.Photo = VideoService.EncryptStringToBytes_Aes(
                personUpdateCommand.photo,
                Convert.FromBase64String(_configuration["AppSettings:CryptKey"]),
                out byte[] urlPhotoIv);
            
            personToUpdate.PhotoIv = urlPhotoIv;
        }
    }

    public static PersonDto PersonToDto(Person person, byte[] key)
    {
        return new(
            person.PersonId,
            person.Name,
            person.Surname,
            person.Patronymic,
            person.Slug,
            VideoService.DecryptStringFromBytes_Aes(person.Photo, key, person.PhotoIv));
    }

    public static PersonCrewDto PersonToPersonCrewDto(Crew crew, byte[] key)
    {
        return new(
            crew.PersonId,
            crew.Person.Name,
            crew.Person.Surname,
            crew.Person.Patronymic,
            crew.Person.Slug,
            VideoService.DecryptStringFromBytes_Aes(crew.Person.Photo, key, crew.Person.PhotoIv),
            RoleToRoleDto(crew.Role));
    }

    public static List<PersonCrewDto> MapCrewToPersonCrewDto(ICollection<Crew> crew, byte[] key)
    {
        List<PersonCrewDto> result = new();
        
        foreach (var person in crew)
            result.Add(PersonToPersonCrewDto(person, key));
        
        return result;
    }

    public static RoleDto RoleToRoleDto(Role role)
    {
        return new(
            role.RoleId,
            role.Name,
            role.Description);
    }

    private List<object> MapPersonsToCrewDto(List<Person> persons)
    {
        List<object> personsListDto = new();

        foreach (var person in persons)
        {
            var newPerson =
                new
                {
                    _id = person.PersonId,
                    name = person.Name,
                    surname = person.Surname,
                    patronymic = person.Patronymic,
                    slug = person.Slug,
                    photo = person.Photo,
                    countCrews = _dbContext.Crews.Count(am => am.PersonId == person.PersonId)
                };
            personsListDto.Add(newPerson);
        }

        return personsListDto;
    }

    public static List<PersonDto> MapPersonsToPersonDto(List<Person> persons, byte[] key)
    {
        List<PersonDto> personsListDto = new();

        foreach (var person in persons)
        {
            PersonDto newPerson =
                new
                (person.PersonId,
                person.Name,
                person.Surname,
                person.Patronymic,
                person.Slug,
                VideoService.DecryptStringFromBytes_Aes(person.Photo, key, person.PhotoIv));
            personsListDto.Add(newPerson);
        }

        return personsListDto;
    }

    public static List<PersonDto> MapPersonsToPersonDto(ICollection<SubscribePerson> persons, byte[] key)
    {
        List<PersonDto> personsListDto = new();

        foreach (var sp in persons)
        {
            PersonDto newPerson =
                new
                (sp.Person.PersonId,
                sp.Person.Name,
                sp.Person.Surname,
                sp.Person.Patronymic,
                sp.Person.Slug,
                VideoService.DecryptStringFromBytes_Aes(sp.Person.Photo, key, sp.Person.PhotoIv));
            personsListDto.Add(newPerson);
        }

        return personsListDto;
    }

    public static List<PersonDto> MapCrewToDto(ICollection<Crew> crew, byte[] key)
    {
        List<PersonDto> personsListDto = new();

        foreach (var person in crew)
        {
            var personDto = PersonToDto(person.Person, key);
            personsListDto.Add(personDto);
        }

        return personsListDto;
    }
    
    public static List<Crew> MapPersonsArrToList(Video video, CrewCreateCommand[] persons)
    {
        var listCrews = new List<Crew>();

        foreach (var person in persons)
        {
            var newCrew = new Crew
            {
                Video = video,
                VideoId = video.VideoId,
                PersonId = person.personId,
                RoleId = person.roleId
            };
            listCrews.Add(newCrew);
        }

        return listCrews;
    }
    
    public static List<SubscribePerson> MapPersonsArrToList(Subscribe subscribe, string[] persons)
    {
        var listPersons = new List<SubscribePerson>();

        foreach (var person in persons)
        {
            var newCrew = new SubscribePerson
            {
                Subscribe = subscribe,
                SubscribeId = subscribe.SubscribeId,
                PersonId = person
            };
            listPersons.Add(newCrew);
        }

        return listPersons;
    }
}