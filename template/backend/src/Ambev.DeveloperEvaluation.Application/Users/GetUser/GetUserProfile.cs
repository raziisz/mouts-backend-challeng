using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Application.Users;

namespace Ambev.DeveloperEvaluation.Application.Users.GetUser;

/// <summary>
/// Profile for mapping between User entity and GetUserResponse
/// </summary>
public class GetUserProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for GetUser operation
    /// </summary>
    public GetUserProfile()
    {
        CreateMap<Geolocation, UserGeolocationModel>();
        CreateMap<Address, UserAddressModel>();
        CreateMap<Name, UserNameModel>();
        CreateMap<User, GetUserResult>();
    }
}
