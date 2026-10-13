using ESpor.Domain.Enums;

namespace ESpor.Application.Matches.DTOs;

public record MatchDto(
    Guid Id,
    string ExternalId,
    string GameSlug,
    string GameName,
    TournamentSummaryDto Tournament,
    MatchStatus Status,
    DateTimeOffset ScheduledAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? EndedAtUtc,
    int BestOf,
    TeamSummaryDto? Team1,
    TeamSummaryDto? Team2,
    int Team1Score,
    int Team2Score,
    Guid? WinnerTeamId
);
