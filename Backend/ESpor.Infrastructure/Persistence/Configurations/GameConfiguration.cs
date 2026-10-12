using ESpor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESpor.Infrastructure.Persistence.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public static readonly Guid LolId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Cs2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ValorantId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.Slug)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(g => g.Slug)
            .IsUnique();

        builder.Property(g => g.CreatedAtUtc)
            .HasColumnType("timestamptz");

        builder.Property(g => g.UpdatedAtUtc)
            .HasColumnType("timestamptz");

        // Seed 3 games
        builder.HasData(
            new Game
            {
                Id = LolId,
                Name = "League of Legends",
                Slug = "league-of-legends",
                IconUrl = "https://cdn.pandascore.co/images/videogame/image/1/600px-League_of_Legends_Logo.png",
                CreatedAtUtc = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
            },
            new Game
            {
                Id = Cs2Id,
                Name = "Counter-Strike 2",
                Slug = "cs-2",
                IconUrl = "https://cdn.pandascore.co/images/videogame/image/3/600px-Counter-Strike_2_logo.png",
                CreatedAtUtc = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
            },
            new Game
            {
                Id = ValorantId,
                Name = "Valorant",
                Slug = "valorant",
                IconUrl = "https://cdn.pandascore.co/images/videogame/image/26/Valorant_logo.png",
                CreatedAtUtc = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
            }
        );
    }
}
