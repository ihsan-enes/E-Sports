using FluentValidation;

namespace ESpor.Application.Matches.Queries;

public record GetLiveMatchesQuery(string? Game);

public class GetLiveMatchesQueryValidator : AbstractValidator<GetLiveMatchesQuery>
{
    private static readonly string[] AllowedSlugs = ["lol", "league-of-legends", "cs2", "cs-2", "csgo", "valorant", "val"];

    public GetLiveMatchesQueryValidator()
    {
        RuleFor(x => x.Game)
            .Must(g => string.IsNullOrWhiteSpace(g) || AllowedSlugs.Contains(g.Trim().ToLowerInvariant()))
            .WithMessage("Geçersiz oyun filtresi. Desteklenen oyunlar: lol, cs2, valorant.");
    }
}
