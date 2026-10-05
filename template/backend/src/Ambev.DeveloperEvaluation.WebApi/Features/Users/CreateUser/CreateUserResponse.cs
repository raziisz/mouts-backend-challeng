using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.WebApi.Features.Users;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;

/// <summary>
/// API response model for CreateUser operation
/// </summary>
public class CreateUserResponse
{
    /// <summary>
    /// The unique identifier of the created user
    /// </summary>
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The user's first and last name
    /// </summary>
    public UserNameResponse Name { get; set; } = new();

    /// <summary>
    /// The user's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's phone number
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    public UserAddressResponse Address { get; set; } = new();

    /// <summary>
    /// The user's role in the system
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// The current status of the user
    /// </summary>
    public UserStatus Status { get; set; }
}
