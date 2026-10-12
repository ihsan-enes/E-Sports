using ESpor.Domain.Common;

namespace ESpor.Domain.Entities;

public class Game : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? IconUrl { get; set; }

    public ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
}
