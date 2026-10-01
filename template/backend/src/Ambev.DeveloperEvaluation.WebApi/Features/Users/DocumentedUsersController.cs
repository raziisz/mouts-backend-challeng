using Ambev.DeveloperEvaluation.Application.Users;
using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users;

[ApiController]
[Route("users")]
public class DocumentedUsersController(IMediator mediator)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) => Ok(await mediator.Send(new ListUsersQuery(), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand { Username = request.Username, Password = request.Password, Phone = request.Phone, Email = request.Email, Status = request.Status, Role = request.Role };
        var result = await mediator.Send(command, cancellationToken);
        return Created($"/users/{result.Id}", result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken) => Ok(await mediator.Send(new Ambev.DeveloperEvaluation.Application.Users.GetUser.GetUserCommand(id), cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserRequest request, CancellationToken cancellationToken) => Ok(await mediator.Send(new UpdateUserCommand(id, request.Username, request.Email, request.Phone, request.Password, request.Status, request.Role), cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) { await mediator.Send(new Ambev.DeveloperEvaluation.Application.Users.DeleteUser.DeleteUserCommand(id), cancellationToken); return Ok(new { message = "User deleted successfully" }); }
}
