namespace ESpor.Application.Matches.DTOs;

public record TeamSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Acronym,
    string? Country,
    string? LogoUrl
);
