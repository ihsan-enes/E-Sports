using ESpor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESpor.Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Provider).IsRequired().HasMaxLength(50);
        builder.Property(t => t.ExternalId).IsRequired().HasMaxLength(100);

        builder.HasIndex(t => new { t.Provider, t.ExternalId }).IsUnique();

        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Acronym).HasMaxLength(50);
        builder.Property(t => t.Country).HasMaxLength(10);

        builder.Property(t => t.CreatedAtUtc).HasColumnType("timestamptz");
        builder.Property(t => t.UpdatedAtUtc).HasColumnType("timestamptz");

        builder.HasOne(t => t.Game)
            .WithMany(g => g.Teams)
            .HasForeignKey(t => t.GameId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
