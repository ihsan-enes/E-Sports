using FluentValidation;

namespace ESpor.Application.Tournaments.Queries;

public record GetTournamentsQuery(
    string? Game,
    string? Status,
    int Page = 1,
    int PageSize = 20
);

public class GetTournamentsQueryValidator : AbstractValidator<GetTournamentsQuery>
{
    private static readonly string[] AllowedSlugs = ["lol", "league-of-legends", "cs2", "cs-2", "csgo", "valorant", "val"];
    private static readonly string[] AllowedStatuses = ["upcoming", "running", "live", "finished", "past", "all"];

    public GetTournamentsQueryValidator()
    {
        RuleFor(x => x.Game)
            .Must(g => string.IsNullOrWhiteSpace(g) || AllowedSlugs.Contains(g.Trim().ToLowerInvariant()))
            .WithMessage("Geçersiz oyun filtresi. Desteklenen oyunlar: lol, cs2, valorant.");

        RuleFor(x => x.Status)
            .Must(s => string.IsNullOrWhiteSpace(s) || AllowedStatuses.Contains(s.Trim().ToLowerInvariant()))
            .WithMessage("Geçersiz turnuva durumu. Desteklenen değerler: upcoming, running, finished, past, all.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Sayfa numarası (page) en az 1 olmalıdır.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Sayfa boyutu (pageSize) 1 ile 100 arasında olmalıdır.");
    }
}
