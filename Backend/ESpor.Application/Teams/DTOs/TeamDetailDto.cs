using ESpor.Application.Matches.DTOs;
using ESpor.Application.Tournaments.DTOs;

namespace ESpor.Application.Teams.DTOs;

public record TeamDetailDto(
    Guid Id,
    string ExternalId,
    GameSummaryDto? Game,
    string Name,
    string Slug,
    string? Acronym,
    string? Country,
    string? LogoUrl,
    int TotalMatchesPlayed,
    int TotalWins,
    double WinRate,
    IReadOnlyList<string> Form,
    IReadOnlyList<PlayerSummaryDto> Roster,
    IReadOnlyList<MatchDto> RecentMatches,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc
);
