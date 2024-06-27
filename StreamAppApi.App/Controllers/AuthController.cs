using System.Globalization;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc;

using StreamAppApi.Contracts.Commands.AuthCommands;
using StreamAppApi.Contracts.Interfaces;

namespace StreamAppApi.App.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] AuthRegisterCommand authRegisterCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        if (authRegisterCommand.password.Length < 6)
        {
            return BadRequest("Invalid password");
        }

        try
        {
            var result = await _authService.RegisterUser(authRegisterCommand, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] AuthLoginCommand authLoginCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        if (authLoginCommand.password.Length < 6)
        {
            return BadRequest("Invalid password");
        }

        try
        {
            var result = await _authService.LoginUser(authLoginCommand, cancellationToken);

            return Ok(result);
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    [HttpPost("login/access-token")]
    public async Task<ActionResult> GetNewTokens([FromBody] AuthGetNewTokensCommand? getNewTokensCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var result = await _authService.GetNewTokens(getNewTokensCommand, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}