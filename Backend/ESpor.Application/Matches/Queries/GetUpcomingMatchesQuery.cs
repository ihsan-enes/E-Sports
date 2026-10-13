using FluentValidation;

namespace ESpor.Application.Matches.Queries;

public record GetUpcomingMatchesQuery(
    string? Game,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page = 1,
    int PageSize = 20
);

public class GetUpcomingMatchesQueryValidator : AbstractValidator<GetUpcomingMatchesQuery>
{
    private static readonly string[] AllowedSlugs = ["lol", "league-of-legends", "cs2", "cs-2", "csgo", "valorant", "val"];

    public GetUpcomingMatchesQueryValidator()
    {
        RuleFor(x => x.Game)
            .Must(g => string.IsNullOrWhiteSpace(g) || AllowedSlugs.Contains(g.Trim().ToLowerInvariant()))
            .WithMessage("Geçersiz oyun filtresi. Desteklenen oyunlar: lol, cs2, valorant.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Sayfa numarası (page) en az 1 olmalıdır.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Sayfa boyutu (pageSize) 1 ile 100 arasında olmalıdır.");

        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From.Value <= x.To.Value)
            .WithMessage("'from' tarihi 'to' tarihinden sonra olamaz.");
    }
}
