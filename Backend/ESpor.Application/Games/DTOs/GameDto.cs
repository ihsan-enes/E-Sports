namespace ESpor.Application.Games.DTOs;

public record GameDto(
    Guid Id,
    string Name,
    string Slug,
    string? IconUrl
);
