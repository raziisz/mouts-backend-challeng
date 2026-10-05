using Ambev.DeveloperEvaluation.Application.Users;
using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users;

[ApiController]
[Route("users")]
public class DocumentedUsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery(Name = "_page")] int page = 1,
        [FromQuery(Name = "_size")] int size = 10,
        [FromQuery(Name = "_order")] string? order = null,
        string? username = null,
        string? email = null,
        string? phone = null,
        UserStatus? status = null,
        UserRole? role = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new ListUsersQuery(page, size, order, username, email, phone, status, role), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand { Username = request.Username, Password = request.Password, Phone = request.Phone, Email = request.Email, Name = new UserNameModel { Firstname = request.Name.Firstname, Lastname = request.Name.Lastname }, Address = new UserAddressModel { City = request.Address.City, Street = request.Address.Street, Number = request.Address.Number, Zipcode = request.Address.Zipcode, Geolocation = new UserGeolocationModel { Lat = request.Address.Geolocation.Lat, Long = request.Address.Geolocation.Long } }, Status = request.Status, Role = request.Role };
        var result = await mediator.Send(command, cancellationToken);
        return Created($"/users/{result.Id}", result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken) => Ok(await mediator.Send(new Ambev.DeveloperEvaluation.Application.Users.GetUser.GetUserCommand(id), cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserRequest request, CancellationToken cancellationToken) => Ok(await mediator.Send(new UpdateUserCommand(id, request.Username, request.Email, new UserNameModel { Firstname = request.Name.Firstname, Lastname = request.Name.Lastname }, request.Phone, new UserAddressModel { City = request.Address.City, Street = request.Address.Street, Number = request.Address.Number, Zipcode = request.Address.Zipcode, Geolocation = new UserGeolocationModel { Lat = request.Address.Geolocation.Lat, Long = request.Address.Geolocation.Long } }, request.Status, request.Role), cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) => Ok(await mediator.Send(new Ambev.DeveloperEvaluation.Application.Users.DeleteUser.DeleteUserCommand(id), cancellationToken));
}
