using Ambev.DeveloperEvaluation.Application.Users;
using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users;

[ApiController]
[Route("users")]
public class DocumentedUsersController(IMediator mediator)
    : ControllerBase
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
        CancellationToken cancellationToken = default)
    {
        var users = (await mediator.Send(new ListUsersQuery(), cancellationToken)).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(username)) users = users.Where(x => TextFilter.Matches(x.Username, username));
        if (!string.IsNullOrWhiteSpace(email)) users = users.Where(x => TextFilter.Matches(x.Email, email));
        if (!string.IsNullOrWhiteSpace(phone)) users = users.Where(x => TextFilter.Matches(x.Phone, phone));
        if (status.HasValue) users = users.Where(x => x.Status == status.Value);
        if (role.HasValue) users = users.Where(x => x.Role == role.Value);

        users = ApplyOrder(users, order);
        return Ok(PagedResultFactory.Create(users, page, size));
    }

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

    private static IEnumerable<UserListResult> ApplyOrder(IEnumerable<UserListResult> source, string? order)
    {
        if (string.IsNullOrWhiteSpace(order)) return source.OrderBy(x => x.Id);

        IOrderedEnumerable<UserListResult>? result = null;
        foreach (var part in order.Replace("\"", string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var desc = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            var field = tokens[0].ToLowerInvariant();
            result = result is null ? Order(source, field, desc) : ThenOrder(result, field, desc);
        }

        return result ?? source;
    }

    private static IOrderedEnumerable<UserListResult> Order(IEnumerable<UserListResult> source, string field, bool desc) => field switch
    {
        "username" => desc ? source.OrderByDescending(x => x.Username) : source.OrderBy(x => x.Username),
        "email" => desc ? source.OrderByDescending(x => x.Email) : source.OrderBy(x => x.Email),
        "phone" => desc ? source.OrderByDescending(x => x.Phone) : source.OrderBy(x => x.Phone),
        "status" => desc ? source.OrderByDescending(x => x.Status) : source.OrderBy(x => x.Status),
        "role" => desc ? source.OrderByDescending(x => x.Role) : source.OrderBy(x => x.Role),
        _ => desc ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
    };

    private static IOrderedEnumerable<UserListResult> ThenOrder(IOrderedEnumerable<UserListResult> source, string field, bool desc) => field switch
    {
        "username" => desc ? source.ThenByDescending(x => x.Username) : source.ThenBy(x => x.Username),
        "email" => desc ? source.ThenByDescending(x => x.Email) : source.ThenBy(x => x.Email),
        "phone" => desc ? source.ThenByDescending(x => x.Phone) : source.ThenBy(x => x.Phone),
        "status" => desc ? source.ThenByDescending(x => x.Status) : source.ThenBy(x => x.Status),
        "role" => desc ? source.ThenByDescending(x => x.Role) : source.ThenBy(x => x.Role),
        _ => desc ? source.ThenByDescending(x => x.Id) : source.ThenBy(x => x.Id)
    };
}
