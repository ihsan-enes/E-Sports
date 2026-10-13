namespace ESpor.Application.Tournaments.DTOs;

public record GameSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string? IconUrl
);
