using HotelSearchLemax.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelSearchLemax.DataAccess.Configurations;

public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(h => h.Latitude)
            .IsRequired();

        builder.Property(h => h.Longitude)
            .IsRequired();

        builder.HasIndex(h => h.Name);
        builder.HasIndex(h => h.Price);
    }
}
