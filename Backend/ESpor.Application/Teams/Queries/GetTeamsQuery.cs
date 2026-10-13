using FluentValidation;

namespace ESpor.Application.Teams.Queries;

public record GetTeamsQuery(
    string? Game,
    string? Search,
    int Page = 1,
    int PageSize = 20
);

public class GetTeamsQueryValidator : AbstractValidator<GetTeamsQuery>
{
    private static readonly string[] AllowedSlugs = ["lol", "league-of-legends", "cs2", "cs-2", "csgo", "valorant", "val"];

    public GetTeamsQueryValidator()
    {
        RuleFor(x => x.Game)
            .Must(g => string.IsNullOrWhiteSpace(g) || AllowedSlugs.Contains(g.Trim().ToLowerInvariant()))
            .WithMessage("Geçersiz oyun filtresi. Desteklenen oyunlar: lol, cs2, valorant.");

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .WithMessage("Arama sorgusu en fazla 100 karakter olabilir.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Sayfa numarası (page) en az 1 olmalıdır.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Sayfa boyutu (pageSize) 1 ile 100 arasında olmalıdır.");
    }
}
