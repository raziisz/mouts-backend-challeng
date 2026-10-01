using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users
{
    public record ListUsersQuery : IRequest<IReadOnlyList<UserListResult>>;
    public record UpdateUserCommand(int Id, string Username, string Email, string Phone, string Password, UserStatus Status, UserRole Role) : IRequest<UserListResult>;
    public class UserListResult { public int Id { get; set; } public string Username { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public string Phone { get; set; } = string.Empty; public UserStatus Status { get; set; } public UserRole Role { get; set; } }

    public static class UserMappings
    {
        public static UserListResult ToResult(User user) => new() { Id = user.Id, Username = user.Username, Email = user.Email, Phone = user.Phone, Status = user.Status, Role = user.Role };
    }

    public class ListUsersHandler(IUserRepository repository) : IRequestHandler<ListUsersQuery, IReadOnlyList<UserListResult>>
    {
        public async Task<IReadOnlyList<UserListResult>> Handle(ListUsersQuery request, CancellationToken cancellationToken) => (await repository.ListAsync(cancellationToken)).Select(UserMappings.ToResult).ToList();
    }

    public class UpdateUserHandler(IUserRepository repository, IPasswordHasher passwordHasher) : IRequestHandler<UpdateUserCommand, UserListResult>
    {
        public async Task<UserListResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await repository.GetByIdAsync(request.Id, cancellationToken) ?? throw new KeyNotFoundException($"User with ID {request.Id} not found");
            user.Username = request.Username; user.Email = request.Email; user.Phone = request.Phone; user.Status = request.Status; user.Role = request.Role; user.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(request.Password)) user.Password = passwordHasher.HashPassword(request.Password);
            await repository.UpdateAsync(user, cancellationToken);
            return UserMappings.ToResult(user);
        }
    }
}
