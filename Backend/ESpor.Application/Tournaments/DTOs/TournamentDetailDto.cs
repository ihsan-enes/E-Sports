using ESpor.Domain.Enums;

namespace ESpor.Application.Tournaments.DTOs;

public record TournamentDetailDto(
    Guid Id,
    string ExternalId,
    GameSummaryDto Game,
    string Name,
    string Slug,
    TournamentTier Tier,
    string? Region,
    string? Country,
    string? LeagueSlug,
    DateTimeOffset? StartDateUtc,
    DateTimeOffset? EndDateUtc,
    string Status,
    int MatchCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc
);
