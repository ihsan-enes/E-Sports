using ESpor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESpor.Infrastructure.Persistence.Configurations;

public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Provider).IsRequired().HasMaxLength(50);
        builder.Property(p => p.ExternalId).IsRequired().HasMaxLength(100);

        builder.HasIndex(p => new { p.Provider, p.ExternalId }).IsUnique();

        builder.Property(p => p.Nickname).IsRequired().HasMaxLength(100);
        builder.Property(p => p.FirstName).HasMaxLength(100);
        builder.Property(p => p.LastName).HasMaxLength(100);
        builder.Property(p => p.Role).HasMaxLength(50);
        builder.Property(p => p.Country).HasMaxLength(10);

        builder.Property(p => p.CreatedAtUtc).HasColumnType("timestamptz");
        builder.Property(p => p.UpdatedAtUtc).HasColumnType("timestamptz");

        builder.HasOne(p => p.Team)
            .WithMany(t => t.Players)
            .HasForeignKey(p => p.TeamId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
