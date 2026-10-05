using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.SaleNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.SaleNumber).IsUnique();
        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.OwnsOne(x => x.Customer, identity =>
        {
            identity.Property(x => x.Id).HasColumnName("CustomerId");
            identity.Property(x => x.Description).HasColumnName("CustomerDescription").HasMaxLength(200);
        });
        builder.OwnsOne(x => x.Branch, identity =>
        {
            identity.Property(x => x.Id).HasColumnName("BranchId");
            identity.Property(x => x.Description).HasColumnName("BranchDescription").HasMaxLength(200);
        });
        builder.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.ProductDescription).IsRequired().HasMaxLength(200);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.DiscountRate).HasPrecision(5, 4).IsRequired();
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2).IsRequired();
        builder.HasIndex(x => x.ProductId);
    }
}
