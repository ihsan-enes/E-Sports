using ESpor.Domain.Enums;

namespace ESpor.Application.Tournaments.DTOs;

public record TournamentDto(
    Guid Id,
    string ExternalId,
    string GameSlug,
    string GameName,
    string Name,
    string Slug,
    TournamentTier Tier,
    string? Region,
    string? Country,
    string? LeagueSlug,
    DateTimeOffset? StartDateUtc,
    DateTimeOffset? EndDateUtc,
    string Status
);
