using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StreamAppApi.Contracts.Commands.PersonCommands;
using StreamAppApi.Contracts.Interfaces;

namespace StreamAppApi.App.Controllers;

[Route("api/persons")]
[ApiController]
public class PersonController : ControllerBase
{
    private readonly IPersonService _personService;

    public PersonController(IPersonService personService)
    {
        _personService = personService;
    }

    // GET: api/persons/by-slug/:slug
    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetPersonBySlug(string slug)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var person = await _personService.GetPersonBySlug(slug, cancellationToken);

            if (person == null)
            {
                return NotFound("Person was not found");
            }

            return Ok(person);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/persons/
    [HttpGet]
    public async Task<IActionResult> GetAllPersons()
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var persons = await _personService.GetAllPersons(cancellationToken);

            return Ok(persons);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /* Admin Rights */

    // POST: api/persons
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post([FromBody] PersonCreateCommand personCreateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var createdPerson = await _personService.CreatePerson(personCreateCommand, cancellationToken);

            return Ok(createdPerson);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/persons/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPersonById(string id)
    {
        try
        {
            var cancellationToken = HttpContext?.RequestAborted ?? default;
            var person = await _personService.GetPersonById(id, cancellationToken);

            return Ok(person);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/persons/:id
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutPersonById(string id, [FromBody] PersonUpdateCommand personUpdateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var updatedPerson = await _personService.UpdatePerson(id, personUpdateCommand, cancellationToken);

            return Ok(updatedPerson);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/persons/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var removedPerson = await _personService.DeletePerson(id, cancellationToken);

            return Ok(removedPerson);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}