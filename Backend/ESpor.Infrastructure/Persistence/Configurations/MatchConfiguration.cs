using ESpor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESpor.Infrastructure.Persistence.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Provider).IsRequired().HasMaxLength(50);
        builder.Property(m => m.ExternalId).IsRequired().HasMaxLength(100);

        builder.HasIndex(m => new { m.Provider, m.ExternalId }).IsUnique();

        builder.Property(m => m.ScheduledAtUtc).HasColumnType("timestamptz");
        builder.Property(m => m.StartedAtUtc).HasColumnType("timestamptz");
        builder.Property(m => m.EndedAtUtc).HasColumnType("timestamptz");
        builder.Property(m => m.CreatedAtUtc).HasColumnType("timestamptz");
        builder.Property(m => m.UpdatedAtUtc).HasColumnType("timestamptz");

        builder.HasOne(m => m.Game)
            .WithMany(g => g.Matches)
            .HasForeignKey(m => m.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Tournament)
            .WithMany(t => t.Matches)
            .HasForeignKey(m => m.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Team1)
            .WithMany(t => t.HomeMatches)
            .HasForeignKey(m => m.Team1Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Team2)
            .WithMany(t => t.AwayMatches)
            .HasForeignKey(m => m.Team2Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.WinnerTeam)
            .WithMany(t => t.WonMatches)
            .HasForeignKey(m => m.WinnerTeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
