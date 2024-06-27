using StreamAppApi.Contracts.Commands.PersonCommands;
using StreamAppApi.Contracts.Dto;

namespace StreamAppApi.Contracts.Interfaces;

public interface IPersonService
{
    Task<PersonDto> GetPersonBySlug(string slug, CancellationToken cancellationToken);

    Task<List<PersonDto>> GetAllPersons(CancellationToken cancellationToken);

    /* Admin Rights */
    Task<PersonDto> CreatePerson(PersonCreateCommand personCreateCommand, CancellationToken cancellationToken);
    Task<PersonDto> GetPersonById(string id, CancellationToken cancellationToken);
    Task<PersonDto> UpdatePerson(string id, PersonUpdateCommand personUpdateCommand, CancellationToken cancellationToken);
    Task<PersonDto> DeletePerson(string id, CancellationToken cancellationToken);
}