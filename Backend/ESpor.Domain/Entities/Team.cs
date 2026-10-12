using ESpor.Domain.Common;

namespace ESpor.Domain.Entities;

public class Team : ExternalEntity
{
    public Guid? GameId { get; set; }
    public Game? Game { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Acronym { get; set; }
    public string? Country { get; set; }
    public string? LogoUrl { get; set; }

    public ICollection<Player> Players { get; set; } = new List<Player>();
    public ICollection<Match> HomeMatches { get; set; } = new List<Match>();
    public ICollection<Match> AwayMatches { get; set; } = new List<Match>();
    public ICollection<Match> WonMatches { get; set; } = new List<Match>();
}
