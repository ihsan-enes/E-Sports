using ESpor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESpor.Infrastructure.Persistence.Configurations;

public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Provider).IsRequired().HasMaxLength(50);
        builder.Property(t => t.ExternalId).IsRequired().HasMaxLength(100);

        builder.HasIndex(t => new { t.Provider, t.ExternalId }).IsUnique();

        builder.Property(t => t.Name).IsRequired().HasMaxLength(250);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(250);
        builder.Property(t => t.Region).HasMaxLength(100);
        builder.Property(t => t.Country).HasMaxLength(10);
        builder.Property(t => t.LeagueSlug).HasMaxLength(150);

        builder.Property(t => t.StartDateUtc).HasColumnType("timestamptz");
        builder.Property(t => t.EndDateUtc).HasColumnType("timestamptz");
        builder.Property(t => t.CreatedAtUtc).HasColumnType("timestamptz");
        builder.Property(t => t.UpdatedAtUtc).HasColumnType("timestamptz");

        builder.HasOne(t => t.Game)
            .WithMany(g => g.Tournaments)
            .HasForeignKey(t => t.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
