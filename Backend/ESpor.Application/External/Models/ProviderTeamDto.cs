namespace ESpor.Application.External.Models;

public record ProviderTeamDto(
    string ExternalId,
    string GameSlug,
    string Name,
    string Slug,
    string? Acronym,
    string? Country,
    string? LogoUrl
);
