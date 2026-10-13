namespace ESpor.Application.Teams.DTOs;

public record PlayerSummaryDto(
    Guid Id,
    string ExternalId,
    string Nickname,
    string? FirstName,
    string? LastName,
    string? Role,
    string? Country,
    string? ImageUrl
);
