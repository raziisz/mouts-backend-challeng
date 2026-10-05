using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ambev.DeveloperEvaluation.ORM.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        DefaultContext context,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        if (!IsDevelopment(configuration))
            return;

        var username = configuration["Seed:Admin:Username"] ?? "admin";
        var email = configuration["Seed:Admin:Email"] ?? "admin@localhost";
        var password = configuration["Seed:Admin:Password"] ?? "Admin@123";

        var existingAdmin = await context.Users
            .FirstOrDefaultAsync(user => user.Username == username || user.Email == email, cancellationToken);

        if (existingAdmin is not null)
            return;

        var admin = new User
        {
            Username = username,
            Email = email,
            Password = passwordHasher.HashPassword(password),
            Name = new Name
            {
                Firstname = configuration["Seed:Admin:Firstname"] ?? "System",
                Lastname = configuration["Seed:Admin:Lastname"] ?? "Administrator"
            },
            Phone = configuration["Seed:Admin:Phone"] ?? "(92) 99999-9999",
            Address = new Address
            {
                City = configuration["Seed:Admin:City"] ?? "Manaus",
                Street = configuration["Seed:Admin:Street"] ?? "Development Street",
                Number = int.TryParse(configuration["Seed:Admin:Number"], out var number) ? number : 1,
                Zipcode = configuration["Seed:Admin:Zipcode"] ?? "69000-000",
                Geolocation = new Geolocation
                {
                    Lat = configuration["Seed:Admin:Latitude"] ?? "-3.1190",
                    Long = configuration["Seed:Admin:Longitude"] ?? "-60.0217"
                }
            },
            Role = UserRole.Admin,
            Status = UserStatus.Active
        };

        await context.Users.AddAsync(admin, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static bool IsDevelopment(IConfiguration configuration) =>
        string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Development", StringComparison.OrdinalIgnoreCase);
}
