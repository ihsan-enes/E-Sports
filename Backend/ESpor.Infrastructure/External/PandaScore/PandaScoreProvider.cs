using System.Net.Http.Json;
using ESpor.Application.External.Interfaces;
using ESpor.Application.External.Models;
using ESpor.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ESpor.Infrastructure.External.PandaScore;

public class PandaScoreProvider : IEsportsDataProvider
{
    public string ProviderName => "PandaScore";

    private readonly HttpClient _httpClient;
    private readonly ILogger<PandaScoreProvider> _logger;

    // Supported games mapping: Internal slug -> PandaScore videogame endpoint slug
    private static readonly Dictionary<string, string> SupportedGames = new(StringComparer.OrdinalIgnoreCase)
    {
        { "league-of-legends", "lol" },
        { "cs-2", "csgo" },
        { "valorant", "valorant" }
    };

    public PandaScoreProvider(HttpClient httpClient, ILogger<PandaScoreProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProviderMatchDto>> GetLiveAndUpcomingMatchesAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<ProviderMatchDto>();
        var now = DateTimeOffset.UtcNow;
        var soonUtc = now.AddMinutes(30);

        foreach (var (internalSlug, endpointSlug) in SupportedGames)
        {
            // 1. Running matches
            var runningUrl = $"{endpointSlug}/matches/running?per_page=50";
            var runningMatches = await FetchMatchesAsync(runningUrl, internalSlug, cancellationToken);
            result.AddRange(runningMatches);

            // 2. Upcoming matches starting within 30 min
            var upcomingUrl = $"{endpointSlug}/matches/upcoming?range[scheduled_at]={now:yyyy-MM-ddTHH:mm:ssZ},{soonUtc:yyyy-MM-ddTHH:mm:ssZ}&per_page=50";
            var upcomingMatches = await FetchMatchesAsync(upcomingUrl, internalSlug, cancellationToken);
            result.AddRange(upcomingMatches);
        }

        return result
            .GroupBy(m => m.ExternalId)
            .Select(g => g.First())
            .ToList();
    }

    public async Task<IReadOnlyList<ProviderMatchDto>> GetFixturesAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<ProviderMatchDto>();

        foreach (var (internalSlug, endpointSlug) in SupportedGames)
        {
            var fixturesUrl = $"{endpointSlug}/matches/upcoming?per_page=100&sort=scheduled_at";
            var matches = await FetchMatchesAsync(fixturesUrl, internalSlug, cancellationToken);
            result.AddRange(matches);
        }

        return result
            .GroupBy(m => m.ExternalId)
            .Select(g => g.First())
            .ToList();
    }

    public async Task<IReadOnlyList<ProviderTeamDto>> GetTeamsAsync(string gameSlug, CancellationToken cancellationToken = default)
    {
        if (!SupportedGames.TryGetValue(gameSlug, out var endpointSlug))
        {
            return [];
        }

        try
        {
            var url = $"{endpointSlug}/teams?per_page=100&sort=-modified_at";
            var response = await _httpClient.GetFromJsonAsync<List<PandaScoreTeam>>(url, cancellationToken);
            if (response == null) return [];

            return response.Select(t => new ProviderTeamDto(
                ExternalId: t.Id.ToString(),
                GameSlug: gameSlug,
                Name: t.Name ?? "Bilinmeyen Takım",
                Slug: t.Slug ?? t.Id.ToString(),
                Acronym: t.Acronym,
                Country: t.Location,
                LogoUrl: t.ImageUrl
            )).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PandaScore'dan {Game} takımları alınamadı.", gameSlug);
            return [];
        }
    }

    public async Task<IReadOnlyList<ProviderPlayerDto>> GetPlayersAsync(string gameSlug, CancellationToken cancellationToken = default)
    {
        if (!SupportedGames.TryGetValue(gameSlug, out var endpointSlug))
        {
            return [];
        }

        try
        {
            var url = $"{endpointSlug}/players?per_page=100&sort=-modified_at";
            var response = await _httpClient.GetFromJsonAsync<List<PandaScorePlayer>>(url, cancellationToken);
            if (response == null) return [];

            return response.Select(p => new ProviderPlayerDto(
                ExternalId: p.Id.ToString(),
                TeamExternalId: p.CurrentTeam?.Id.ToString(),
                Nickname: string.IsNullOrWhiteSpace(p.Name) ? (p.FirstName ?? "Oyuncu") : p.Name,
                FirstName: p.FirstName,
                LastName: p.LastName,
                Role: p.Role,
                Country: p.Nationality,
                ImageUrl: p.ImageUrl
            )).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PandaScore'dan {Game} oyuncuları alınamadı.", gameSlug);
            return [];
        }
    }

    private async Task<List<ProviderMatchDto>> FetchMatchesAsync(string requestUrl, string internalGameSlug, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<PandaScoreMatch>>(requestUrl, cancellationToken);
            if (response == null) return [];

            return response.Select(m => MapMatch(m, internalGameSlug)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PandaScore URL '{Url}' çağrılırken hata meydana geldi.", requestUrl);
            return [];
        }
    }

    private static ProviderMatchDto MapMatch(PandaScoreMatch match, string internalGameSlug)
    {
        var status = MapStatus(match.Status);
        var tier = MapTier(match.Tournament?.Tier);

        var leagueSlug = match.Tournament?.League?.Slug ?? match.League?.Slug;

        ProviderTournamentDto? tournamentDto = null;
        if (match.Tournament != null)
        {
            tournamentDto = new ProviderTournamentDto(
                ExternalId: match.Tournament.Id.ToString(),
                GameSlug: internalGameSlug,
                Name: match.Tournament.Name ?? "Bilinmeyen Turnuva",
                Slug: match.Tournament.Slug ?? match.Tournament.Id.ToString(),
                Tier: tier,
                Region: match.Tournament.Region,
                Country: match.Tournament.Country,
                LeagueSlug: leagueSlug,
                StartDateUtc: match.Tournament.BeginAt,
                EndDateUtc: match.Tournament.EndAt
            );
        }

        var opponents = match.Opponents ?? [];
        ProviderTeamDto? team1Dto = null;
        ProviderTeamDto? team2Dto = null;
        var players = new List<ProviderPlayerDto>();

        if (opponents.Count > 0 && opponents[0].Opponent != null)
        {
            var t1 = opponents[0].Opponent!;
            team1Dto = new ProviderTeamDto(
                ExternalId: t1.Id.ToString(),
                GameSlug: internalGameSlug,
                Name: t1.Name ?? "Takım 1",
                Slug: t1.Slug ?? t1.Id.ToString(),
                Acronym: t1.Acronym,
                Country: t1.Location,
                LogoUrl: t1.ImageUrl
            );

            if (t1.Players != null)
            {
                players.AddRange(t1.Players.Select(p => new ProviderPlayerDto(
                    ExternalId: p.Id.ToString(),
                    TeamExternalId: t1.Id.ToString(),
                    Nickname: string.IsNullOrWhiteSpace(p.Name) ? (p.FirstName ?? "Oyuncu") : p.Name,
                    FirstName: p.FirstName,
                    LastName: p.LastName,
                    Role: p.Role,
                    Country: p.Nationality,
                    ImageUrl: p.ImageUrl
                )));
            }
        }

        if (opponents.Count > 1 && opponents[1].Opponent != null)
        {
            var t2 = opponents[1].Opponent!;
            team2Dto = new ProviderTeamDto(
                ExternalId: t2.Id.ToString(),
                GameSlug: internalGameSlug,
                Name: t2.Name ?? "Takım 2",
                Slug: t2.Slug ?? t2.Id.ToString(),
                Acronym: t2.Acronym,
                Country: t2.Location,
                LogoUrl: t2.ImageUrl
            );

            if (t2.Players != null)
            {
                players.AddRange(t2.Players.Select(p => new ProviderPlayerDto(
                    ExternalId: p.Id.ToString(),
                    TeamExternalId: t2.Id.ToString(),
                    Nickname: string.IsNullOrWhiteSpace(p.Name) ? (p.FirstName ?? "Oyuncu") : p.Name,
                    FirstName: p.FirstName,
                    LastName: p.LastName,
                    Role: p.Role,
                    Country: p.Nationality,
                    ImageUrl: p.ImageUrl
                )));
            }
        }

        var team1Score = 0;
        var team2Score = 0;
        if (match.Results != null)
        {
            if (team1Dto != null)
            {
                var r1 = match.Results.FirstOrDefault(r => r.TeamId.ToString() == team1Dto.ExternalId);
                if (r1 != null) team1Score = r1.Score;
            }

            if (team2Dto != null)
            {
                var r2 = match.Results.FirstOrDefault(r => r.TeamId.ToString() == team2Dto.ExternalId);
                if (r2 != null) team2Score = r2.Score;
            }
        }

        return new ProviderMatchDto(
            ExternalId: match.Id.ToString(),
            GameSlug: internalGameSlug,
            TournamentExternalId: match.Tournament?.Id.ToString() ?? "0",
            Status: status,
            ScheduledAtUtc: match.ScheduledAt ?? match.BeginAt ?? DateTimeOffset.UtcNow,
            StartedAtUtc: match.BeginAt,
            EndedAtUtc: match.EndAt,
            BestOf: match.NumberOfGames > 0 ? match.NumberOfGames : 1,
            Team1ExternalId: team1Dto?.ExternalId,
            Team2ExternalId: team2Dto?.ExternalId,
            Team1Score: team1Score,
            Team2Score: team2Score,
            WinnerTeamExternalId: match.WinnerId?.ToString(),
            Tournament: tournamentDto,
            Team1: team1Dto,
            Team2: team2Dto,
            Players: players
        );
    }

    private static MatchStatus MapStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "running" => MatchStatus.Live,
            "finished" => MatchStatus.Finished,
            "canceled" => MatchStatus.Canceled,
            "postponed" => MatchStatus.Scheduled,
            _ => MatchStatus.Scheduled
        };
    }

    private static TournamentTier MapTier(string? tier)
    {
        return tier?.ToUpperInvariant() switch
        {
            "S" => TournamentTier.S,
            "A" => TournamentTier.A,
            "B" => TournamentTier.B,
            "C" => TournamentTier.C,
            "D" => TournamentTier.D,
            _ => TournamentTier.Unranked
        };
    }
}
