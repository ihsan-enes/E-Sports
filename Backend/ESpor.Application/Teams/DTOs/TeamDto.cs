using ESpor.Application.Tournaments.DTOs;

namespace ESpor.Application.Teams.DTOs;

public record TeamDto(
    Guid Id,
    string ExternalId,
    GameSummaryDto? Game,
    string Name,
    string Slug,
    string? Acronym,
    string? Country,
    string? LogoUrl
);
