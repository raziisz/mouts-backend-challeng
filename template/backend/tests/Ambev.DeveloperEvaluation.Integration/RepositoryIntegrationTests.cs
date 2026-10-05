using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Common.Persistence;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration;

[Collection("Integration database")]
public sealed class RepositoryIntegrationTests(IntegrationTestDatabase database)
{
    [Fact]
    public async Task User_repository_should_persist_update_query_and_delete_by_email()
    {
        await using var context = database.CreateContext();
        var repository = new UserRepository(context);
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var user = new User
        {
            Username = $"integration-user-{suffix}",
            Email = $"integration-user-{suffix}@example.com",
            Password = "hashed-password",
            Phone = "+5592987654321",
            Status = UserStatus.Active,
            Role = UserRole.Customer,
            Name = new Name { Firstname = "Integration", Lastname = "User" },
            Address = new Address
            {
                City = "Manaus",
                Street = "Integration Street",
                Number = 10,
                Zipcode = "69000-000",
                Geolocation = new Geolocation { Lat = "-3.1", Long = "-60.0" }
            }
        };

        try
        {
            var created = await repository.CreateAsync(user);
            var found = await repository.GetByEmailAsync(created.Email);

            Assert.NotNull(found);
            Assert.Equal(created.Id, found!.Id);

            created.Name.Firstname = "Updated";
            await repository.UpdateAsync(created);

            var page = await repository.ListAsync(new UserListQuery(new PageQuery(1, 10), Email: created.Email));
            Assert.Single(page.Data);
            Assert.Equal("Updated", page.Data[0].Name.Firstname);

            var filtered = await repository.ListAsync(new UserListQuery(
                new PageQuery(1, 10, "username asc"),
                Email: created.Email,
                Status: UserStatus.Active,
                Role: UserRole.Customer));
            Assert.Single(filtered.Data);
            Assert.Equal(created.Id, filtered.Data[0].Id);
        }
        finally
        {
            await repository.DeleteAsync(user.Id);
        }
    }

    [Fact]
    public async Task Product_repository_should_persist_filter_categories_update_and_delete()
    {
        await using var context = database.CreateContext();
        var repository = new ProductRepository(context);
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var product = new Product
        {
            Title = $"Integration Product {suffix}",
            Price = 25.50m,
            Description = "Integration product",
            Category = $"integration-{suffix}",
            Image = "http://localhost/product.png",
            RatingRate = 4.5m,
            RatingCount = 3
        };

        try
        {
            var created = await repository.CreateAsync(product);
            var page = await repository.ListAsync(new ProductListQuery(new PageQuery(1, 10), Category: product.Category));
            Assert.Contains(page.Data, item => item.Id == created.Id);
            Assert.Contains(product.Category, await repository.ListCategoriesAsync());

            var range = await repository.ListAsync(new ProductListQuery(
                new PageQuery(1, 1, "price desc"),
                Category: product.Category,
                MinPrice: 20m,
                MaxPrice: 30m));
            Assert.Single(range.Data);
            Assert.Equal(created.Id, range.Data[0].Id);

            created.Price = 30m;
            await repository.UpdateAsync(created);
            Assert.Equal(30m, (await repository.GetByIdAsync(created.Id))!.Price);
        }
        finally
        {
            await repository.DeleteAsync(product.Id);
        }
    }

    [Fact]
    public async Task Cart_repository_should_persist_items_filter_update_and_delete()
    {
        await using var context = database.CreateContext();
        var repository = new CartRepository(context);
        var cart = new Cart { UserId = int.MaxValue - Random.Shared.Next(1000), Date = DateTime.UtcNow };
        cart.AddItem(1, 2);

        try
        {
            var created = await repository.CreateAsync(cart);
            var found = await repository.GetByIdAsync(created.Id);

            Assert.NotNull(found);
            Assert.Single(found!.Items);
            Assert.Equal(2, found.Items[0].Quantity);

            var filtered = await repository.ListAsync(new CartListQuery(new PageQuery(1, 10), UserId: cart.UserId));
            Assert.Contains(filtered.Data, item => item.Id == created.Id);

            var dateFiltered = await repository.ListAsync(new CartListQuery(
                new PageQuery(1, 1, "date desc"),
                UserId: cart.UserId,
                Date: cart.Date,
                MinDate: cart.Date.AddDays(-1),
                MaxDate: cart.Date.AddDays(1)));
            Assert.Single(dateFiltered.Data);
            Assert.Equal(created.Id, dateFiltered.Data[0].Id);

            var update = new Cart { Id = created.Id, UserId = cart.UserId, Date = cart.Date.AddDays(1) };
            update.AddItem(1, 4);
            await repository.UpdateAsync(update);
            Assert.Equal(4, (await repository.GetByIdAsync(created.Id))!.Items[0].Quantity);
        }
        finally
        {
            await repository.DeleteAsync(cart.Id);
        }
    }

    [Fact]
    public async Task Sale_repository_should_persist_items_discounts_filters_and_update()
    {
        await using var context = database.CreateContext();
        var repository = new SaleRepository(context);
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var sale = new Sale
        {
            SaleNumber = $"INTEGRATION-{suffix}",
            Date = DateTime.UtcNow,
            Customer = new ExternalIdentity { Id = 42, Description = "Integration customer" },
            Branch = new ExternalIdentity { Id = 7, Description = "Integration branch" }
        };
        sale.AddItem(1, "Integration product", 10m, 10);

        try
        {
            var created = await repository.CreateAsync(sale);
            var found = await repository.GetByIdAsync(created.Id);

            Assert.NotNull(found);
            Assert.Equal(0.20m, found!.Items.Single().DiscountRate);
            Assert.Equal(80m, found.TotalAmount);

            var filtered = await repository.ListAsync(new SaleListQuery(new PageQuery(1, 10), SaleNumber: sale.SaleNumber));
            Assert.Contains(filtered.Data, item => item.Id == created.Id);

            var dateAndStatusFiltered = await repository.ListAsync(new SaleListQuery(
                new PageQuery(1, 1, "date desc"),
                SaleNumber: sale.SaleNumber,
                Status: "Active",
                Date: sale.Date,
                MinDate: sale.Date.AddDays(-1),
                MaxDate: sale.Date.AddDays(1)));
            Assert.Single(dateAndStatusFiltered.Data);
            Assert.Equal(created.Id, dateAndStatusFiltered.Data[0].Id);

            var update = new Sale
            {
                Id = created.Id,
                SaleNumber = sale.SaleNumber,
                Date = sale.Date,
                Customer = sale.Customer,
                Branch = sale.Branch
            };
            update.AddItem(1, "Integration product", 10m, 4);
            await repository.UpdateAsync(update);

            Assert.Equal(0.10m, (await repository.GetByIdAsync(created.Id))!.Items.Single().DiscountRate);
        }
        finally
        {
            var persisted = await context.Sales.FindAsync(sale.Id);
            if (persisted is not null)
            {
                context.Sales.Remove(persisted);
                await context.SaveChangesAsync();
            }
        }
    }

    [Fact]
    public async Task User_database_should_enforce_unique_email_and_username()
    {
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var firstUser = new User
        {
            Username = $"unique-user-{suffix}",
            Email = $"unique-user-{suffix}@example.com",
            Password = "hashed-password",
            Phone = "+5592987654321",
            Status = UserStatus.Active,
            Role = UserRole.Customer,
            Name = new Name { Firstname = "Unique", Lastname = "User" },
            Address = new Address
            {
                City = "Manaus",
                Street = "Unique Street",
                Number = 10,
                Zipcode = "69000-000",
                Geolocation = new Geolocation { Lat = "-3.1", Long = "-60.0" }
            }
        };

        await using var context = database.CreateContext();
        var repository = new UserRepository(context);
        await repository.CreateAsync(firstUser);

        try
        {
            await using (var duplicateEmailContext = database.CreateContext())
            {
                var duplicateEmail = CloneUser(firstUser, $"unique-email-{suffix}", firstUser.Email);
                await duplicateEmailContext.Users.AddAsync(duplicateEmail);
                await Assert.ThrowsAsync<DbUpdateException>(() => duplicateEmailContext.SaveChangesAsync());
            }

            await using (var duplicateUsernameContext = database.CreateContext())
            {
                var duplicateUsername = CloneUser(firstUser, firstUser.Username, $"unique-username-{suffix}@example.com");
                await duplicateUsernameContext.Users.AddAsync(duplicateUsername);
                await Assert.ThrowsAsync<DbUpdateException>(() => duplicateUsernameContext.SaveChangesAsync());
            }
        }
        finally
        {
            await repository.DeleteAsync(firstUser.Id);
        }
    }

    private static User CloneUser(User source, string username, string email) => new()
    {
        Username = username,
        Email = email,
        Password = source.Password,
        Phone = "+5592987654322",
        Status = source.Status,
        Role = source.Role,
        Name = new Name { Firstname = source.Name.Firstname, Lastname = source.Name.Lastname },
        Address = new Address
        {
            City = source.Address.City,
            Street = source.Address.Street,
            Number = source.Address.Number,
            Zipcode = source.Address.Zipcode,
            Geolocation = new Geolocation { Lat = source.Address.Geolocation.Lat, Long = source.Address.Geolocation.Long }
        }
    };
}
