using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Application.Users;
using Ambev.DeveloperEvaluation.WebApi.Features.Users;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;

/// <summary>
/// Profile for mapping between Application and API CreateUser responses
/// </summary>
public class CreateUserProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for CreateUser feature
    /// </summary>
    public CreateUserProfile()
    {
        CreateMap<CreateUserRequest, CreateUserCommand>();
        CreateMap<UserGeolocationRequest, UserGeolocationModel>();
        CreateMap<UserAddressRequest, UserAddressModel>();
        CreateMap<UserGeolocationModel, UserGeolocationResponse>();
        CreateMap<UserAddressModel, UserAddressResponse>();
        CreateMap<CreateUserResult, CreateUserResponse>();
    }
}
