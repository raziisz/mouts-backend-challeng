using Ambev.DeveloperEvaluation.Application.Users;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Application.Users.CreateUser;

/// <summary>
/// Represents the response returned after successfully creating a new user.
/// </summary>
public class CreateUserResult
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public UserAddressModel Address { get; set; } = new();
    public UserStatus Status { get; set; }
    public UserRole Role { get; set; }
}
