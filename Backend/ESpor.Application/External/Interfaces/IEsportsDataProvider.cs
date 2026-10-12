using ESpor.Application.External.Models;

namespace ESpor.Application.External.Interfaces;

public interface IEsportsDataProvider
{
    string ProviderName { get; }

    Task<IReadOnlyList<ProviderMatchDto>> GetLiveAndUpcomingMatchesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProviderMatchDto>> GetFixturesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProviderTeamDto>> GetTeamsAsync(string gameSlug, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProviderPlayerDto>> GetPlayersAsync(string gameSlug, CancellationToken cancellationToken = default);
}
