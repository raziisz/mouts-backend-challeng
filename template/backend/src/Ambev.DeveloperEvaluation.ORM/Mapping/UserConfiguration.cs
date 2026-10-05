using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd();

        builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Password).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Phone).HasMaxLength(20);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Username).IsUnique();

        builder.OwnsOne(u => u.Name, name =>
        {
            name.Property(x => x.Firstname).HasColumnName("Name_Firstname").HasMaxLength(100).IsRequired();
            name.Property(x => x.Lastname).HasColumnName("Name_Lastname").HasMaxLength(100).IsRequired();
        });

        builder.OwnsOne(u => u.Address, address =>
        {
            address.Property(x => x.City).HasMaxLength(100).IsRequired();
            address.Property(x => x.Street).HasMaxLength(150).IsRequired();
            address.Property(x => x.Number).IsRequired();
            address.Property(x => x.Zipcode).HasMaxLength(20).IsRequired();
            address.OwnsOne(x => x.Geolocation, geolocation =>
            {
                geolocation.Property(x => x.Lat).HasMaxLength(50).IsRequired();
                geolocation.Property(x => x.Long).HasMaxLength(50).IsRequired();
            });
        });

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

    }
}
