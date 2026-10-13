using ESpor.Domain.Enums;

namespace ESpor.Application.Matches.DTOs;

public record TournamentSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    TournamentTier Tier,
    string? LeagueSlug
);
