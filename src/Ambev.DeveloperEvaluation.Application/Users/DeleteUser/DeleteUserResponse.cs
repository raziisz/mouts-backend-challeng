namespace Ambev.DeveloperEvaluation.Application.Users.DeleteUser;

using Ambev.DeveloperEvaluation.Application.Users;
using Ambev.DeveloperEvaluation.Domain.Enums;

/// <summary>
/// Response model for DeleteUser operation
/// </summary>
public class DeleteUserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserNameModel Name { get; set; } = new();
    public string Phone { get; set; } = string.Empty;
    public UserAddressModel Address { get; set; } = new();
    public UserStatus Status { get; set; }
    public UserRole Role { get; set; }
}
