using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Auth.AuthenticateUserFeature;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Auth;

[ApiController]
[Route("auth")]
public class DocumentedAuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(AuthenticateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AuthenticateUserCommand { Username = request.Username, Email = request.Email, Password = request.Password }, cancellationToken);
        return Ok(new { token = result.Token });
    }
}
