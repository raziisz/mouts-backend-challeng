using Ambev.DeveloperEvaluation.Common.Persistence;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users
{
    public record ListUsersQuery(int Page = 1, int Size = 10, string? Order = null, string? Username = null, string? Email = null, string? Phone = null, UserStatus? Status = null, UserRole? Role = null) : IRequest<PagedResult<UserListResult>>;
    public record UpdateUserCommand(int Id, string Username, string Email, UserNameModel Name, string Phone, UserAddressModel Address, UserStatus Status, UserRole Role) : IRequest<UserListResult>;
    public class UserListResult { public int Id { get; set; } public string Username { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public UserNameModel Name { get; set; } = new(); public string Phone { get; set; } = string.Empty; public UserAddressModel Address { get; set; } = new(); public UserStatus Status { get; set; } public UserRole Role { get; set; } }

    public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(user => user.Username).NotEmpty().Length(3, 50);
            RuleFor(user => user.Email).SetValidator(new EmailValidator());
            RuleFor(user => user.Name)
                .NotNull()
                .ChildRules(name =>
                {
                    name.RuleFor(value => value.Firstname).NotEmpty().MaximumLength(100);
                    name.RuleFor(value => value.Lastname).NotEmpty().MaximumLength(100);
                });
            RuleFor(user => user.Phone).SetValidator(new PhoneValidator());
            RuleFor(user => user.Status).NotEqual(UserStatus.Unknown);
            RuleFor(user => user.Role).NotEqual(UserRole.None);
            RuleFor(user => user.Address)
                .NotNull()
                .ChildRules(address =>
                {
                    address.RuleFor(value => value.City).NotEmpty().MaximumLength(100);
                    address.RuleFor(value => value.Street).NotEmpty().MaximumLength(150);
                    address.RuleFor(value => value.Number).GreaterThan(0);
                    address.RuleFor(value => value.Zipcode).NotEmpty().MaximumLength(20);
                    address.RuleFor(value => value.Geolocation)
                        .NotNull()
                        .ChildRules(geolocation =>
                        {
                            geolocation.RuleFor(value => value.Lat).NotEmpty().MaximumLength(50);
                            geolocation.RuleFor(value => value.Long).NotEmpty().MaximumLength(50);
                        });
                });
        }
    }

    public static class UserMappings
    {
        public static UserListResult ToResult(User user) => new() { Id = user.Id, Username = user.Username, Email = user.Email, Name = new UserNameModel { Firstname = user.Name.Firstname, Lastname = user.Name.Lastname }, Phone = user.Phone, Address = new UserAddressModel { City = user.Address.City, Street = user.Address.Street, Number = user.Address.Number, Zipcode = user.Address.Zipcode, Geolocation = new UserGeolocationModel { Lat = user.Address.Geolocation.Lat, Long = user.Address.Geolocation.Long } }, Status = user.Status, Role = user.Role };
        public static PagedResult<UserListResult> ToPage(PagedResult<User> page) => new() { Data = page.Data.Select(ToResult).ToList(), TotalItems = page.TotalItems, CurrentPage = page.CurrentPage, TotalPages = page.TotalPages };
    }

    public class ListUsersHandler(IUserRepository repository) : IRequestHandler<ListUsersQuery, PagedResult<UserListResult>>
    {
        public async Task<PagedResult<UserListResult>> Handle(ListUsersQuery request, CancellationToken cancellationToken) => UserMappings.ToPage(await repository.ListAsync(new UserListQuery(new PageQuery(request.Page, request.Size, request.Order), request.Username, request.Email, request.Phone, request.Status, request.Role), cancellationToken));
    }

    public class UpdateUserHandler(IUserRepository repository) : IRequestHandler<UpdateUserCommand, UserListResult>
    {
        public async Task<UserListResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await new UpdateUserCommandValidator().ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var user = await repository.GetByIdAsync(request.Id, cancellationToken) ?? throw new KeyNotFoundException($"User with ID {request.Id} not found");

            var existingEmail = await repository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingEmail is not null && existingEmail.Id != request.Id)
                throw new InvalidOperationException($"User with email {request.Email} already exists");

            var existingUsername = await repository.GetByUsernameAsync(request.Username, cancellationToken);
            if (existingUsername is not null && existingUsername.Id != request.Id)
                throw new InvalidOperationException($"User with username {request.Username} already exists");

            user.Username = request.Username; user.Email = request.Email; user.Name = new Domain.Entities.Name { Firstname = request.Name.Firstname, Lastname = request.Name.Lastname }; user.Phone = request.Phone; user.Status = request.Status; user.Role = request.Role;
            user.Address = new Domain.Entities.Address { City = request.Address.City, Street = request.Address.Street, Number = request.Address.Number, Zipcode = request.Address.Zipcode, Geolocation = new Domain.Entities.Geolocation { Lat = request.Address.Geolocation.Lat, Long = request.Address.Geolocation.Long } };
            user.UpdatedAt = DateTime.UtcNow;
            await repository.UpdateAsync(user, cancellationToken);
            return UserMappings.ToResult(user);
        }
    }
}
