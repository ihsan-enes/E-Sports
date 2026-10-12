namespace ESpor.Application.External.Models;

public record ProviderPlayerDto(
    string ExternalId,
    string? TeamExternalId,
    string Nickname,
    string? FirstName,
    string? LastName,
    string? Role,
    string? Country,
    string? ImageUrl
);
