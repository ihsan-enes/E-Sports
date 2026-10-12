using ESpor.Domain.Enums;

namespace ESpor.Application.External.Models;

public record ProviderTournamentDto(
    string ExternalId,
    string GameSlug,
    string Name,
    string Slug,
    TournamentTier Tier,
    string? Region,
    string? Country,
    string? LeagueSlug,
    DateTimeOffset? StartDateUtc,
    DateTimeOffset? EndDateUtc
);
