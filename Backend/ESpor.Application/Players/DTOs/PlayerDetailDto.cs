using ESpor.Application.Matches.DTOs;
using ESpor.Application.Tournaments.DTOs;

namespace ESpor.Application.Players.DTOs;

public record PlayerDetailDto(
    Guid Id,
    string ExternalId,
    TeamSummaryDto? Team,
    GameSummaryDto? Game,
    string Nickname,
    string? FirstName,
    string? LastName,
    string? Role,
    string? Country,
    string? ImageUrl,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc
);
