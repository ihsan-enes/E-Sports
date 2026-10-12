using ESpor.Domain.Enums;

namespace ESpor.Application.External.Models;

public record ProviderMatchDto(
    string ExternalId,
    string GameSlug,
    string TournamentExternalId,
    MatchStatus Status,
    DateTimeOffset ScheduledAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? EndedAtUtc,
    int BestOf,
    string? Team1ExternalId,
    string? Team2ExternalId,
    int Team1Score,
    int Team2Score,
    string? WinnerTeamExternalId,
    ProviderTournamentDto? Tournament,
    ProviderTeamDto? Team1,
    ProviderTeamDto? Team2,
    IReadOnlyList<ProviderPlayerDto>? Players
);
