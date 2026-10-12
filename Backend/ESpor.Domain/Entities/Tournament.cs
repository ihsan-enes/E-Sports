using ESpor.Domain.Common;
using ESpor.Domain.Enums;

namespace ESpor.Domain.Entities;

public class Tournament : ExternalEntity
{
    public Guid GameId { get; set; }
    public Game Game { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public TournamentTier Tier { get; set; } = TournamentTier.Unknown;
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? LeagueSlug { get; set; }

    public DateTimeOffset? StartDateUtc { get; set; }
    public DateTimeOffset? EndDateUtc { get; set; }

    public ICollection<Match> Matches { get; set; } = new List<Match>();
}
