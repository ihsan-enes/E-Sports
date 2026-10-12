using ESpor.Domain.Common;
using ESpor.Domain.Enums;

namespace ESpor.Domain.Entities;

public class Match : ExternalEntity
{
    public Guid GameId { get; set; }
    public Game Game { get; set; } = null!;

    public Guid TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
    public DateTimeOffset ScheduledAtUtc { get; set; }
    public DateTimeOffset? StartedAtUtc { get; set; }
    public DateTimeOffset? EndedAtUtc { get; set; }

    public int BestOf { get; set; } = 1;

    public Guid? Team1Id { get; set; }
    public Team? Team1 { get; set; }

    public Guid? Team2Id { get; set; }
    public Team? Team2 { get; set; }

    public int Team1Score { get; set; }
    public int Team2Score { get; set; }

    public Guid? WinnerTeamId { get; set; }
    public Team? WinnerTeam { get; set; }
}
