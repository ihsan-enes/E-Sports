using ESpor.Application.Common;
using ESpor.Application.Common.Interfaces;
using ESpor.Application.Common.Options;
using ESpor.Application.External.Interfaces;
using ESpor.Application.External.Models;
using ESpor.Application.Sync.Interfaces;
using ESpor.Domain.Entities;
using ESpor.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESpor.Application.Sync.Services;

public class EsportsSyncService : IEsportsSyncService
{
    private readonly IApplicationDbContext _context;
    private readonly IEsportsDataProvider _dataProvider;
    private readonly RelevanceOptions _relevanceOptions;
    private readonly ILogger<EsportsSyncService> _logger;

    public EsportsSyncService(
        IApplicationDbContext context,
        IEsportsDataProvider dataProvider,
        IOptions<RelevanceOptions> relevanceOptions,
        ILogger<EsportsSyncService> logger)
    {
        _context = context;
        _dataProvider = dataProvider;
        _relevanceOptions = relevanceOptions.Value;
        _logger = logger;
    }

    public async Task<Result> SyncLiveMatchesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var matches = await _dataProvider.GetLiveAndUpcomingMatchesAsync(cancellationToken);
            var resultCount = await UpsertMatchesAsync(matches, cancellationToken);
            _logger.LogInformation("Canlı ve yaklaşan maçlar senkronize edildi. İşlenen maç sayısı: {Count}", resultCount);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Canlı maç senkronizasyonunda hata oluştu.");
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> SyncFixturesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var matches = await _dataProvider.GetFixturesAsync(cancellationToken);
            var resultCount = await UpsertMatchesAsync(matches, cancellationToken);
            _logger.LogInformation("Fikstür senkronize edildi. İşlenen maç sayısı: {Count}", resultCount);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fikstür senkronizasyonunda hata oluştu.");
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> SyncStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var games = await _context.Games.ToListAsync(cancellationToken);
            var now = DateTimeOffset.UtcNow;
            var providerName = _dataProvider.ProviderName;

            foreach (var game in games)
            {
                // 1. Teams Upsert
                var rawTeams = await _dataProvider.GetTeamsAsync(game.Slug, cancellationToken);
                var distinctTeams = rawTeams
                    .Where(t => !string.IsNullOrWhiteSpace(t.ExternalId))
                    .DistinctBy(t => t.ExternalId)
                    .ToList();

                var teamExtIds = distinctTeams.Select(t => t.ExternalId).ToList();
                var existingTeams = await _context.Teams
                    .Where(t => t.Provider == providerName && teamExtIds.Contains(t.ExternalId))
                    .ToDictionaryAsync(t => t.ExternalId, cancellationToken);

                foreach (var teamDto in distinctTeams)
                {
                    if (existingTeams.TryGetValue(teamDto.ExternalId, out var existingTeam))
                    {
                        if (existingTeam.GameId == null)
                        {
                            existingTeam.GameId = game.Id;
                        }
                        existingTeam.Name = teamDto.Name;
                        existingTeam.Slug = teamDto.Slug;
                        existingTeam.Acronym = teamDto.Acronym;
                        existingTeam.Country = teamDto.Country;
                        existingTeam.LogoUrl = teamDto.LogoUrl;
                        existingTeam.UpdatedAtUtc = now;
                    }
                    else
                    {
                        var newTeam = new Team
                        {
                            Id = Guid.NewGuid(),
                            Provider = providerName,
                            ExternalId = teamDto.ExternalId,
                            GameId = game.Id,
                            Name = teamDto.Name,
                            Slug = teamDto.Slug,
                            Acronym = teamDto.Acronym,
                            Country = teamDto.Country,
                            LogoUrl = teamDto.LogoUrl,
                            CreatedAtUtc = now,
                            UpdatedAtUtc = now
                        };
                        _context.Teams.Add(newTeam);
                        existingTeams[teamDto.ExternalId] = newTeam;
                    }
                }

                // 2. Players Upsert
                var rawPlayers = await _dataProvider.GetPlayersAsync(game.Slug, cancellationToken);
                var distinctPlayers = rawPlayers
                    .Where(p => !string.IsNullOrWhiteSpace(p.ExternalId))
                    .DistinctBy(p => p.ExternalId)
                    .ToList();

                var playerExtIds = distinctPlayers.Select(p => p.ExternalId).ToList();
                var existingPlayers = await _context.Players
                    .Where(p => p.Provider == providerName && playerExtIds.Contains(p.ExternalId))
                    .ToDictionaryAsync(p => p.ExternalId, cancellationToken);

                foreach (var playerDto in distinctPlayers)
                {
                    Guid? teamId = null;
                    if (!string.IsNullOrEmpty(playerDto.TeamExternalId) &&
                        existingTeams.TryGetValue(playerDto.TeamExternalId, out var playerTeam))
                    {
                        teamId = playerTeam.Id;
                    }

                    if (existingPlayers.TryGetValue(playerDto.ExternalId, out var existingPlayer))
                    {
                        if (teamId.HasValue)
                        {
                            existingPlayer.TeamId = teamId;
                        }
                        existingPlayer.Nickname = playerDto.Nickname;
                        existingPlayer.FirstName = playerDto.FirstName;
                        existingPlayer.LastName = playerDto.LastName;
                        existingPlayer.Role = playerDto.Role;
                        existingPlayer.Country = playerDto.Country;
                        existingPlayer.ImageUrl = playerDto.ImageUrl;
                        existingPlayer.UpdatedAtUtc = now;
                    }
                    else
                    {
                        var newPlayer = new Player
                        {
                            Id = Guid.NewGuid(),
                            Provider = providerName,
                            ExternalId = playerDto.ExternalId,
                            TeamId = teamId,
                            Nickname = playerDto.Nickname,
                            FirstName = playerDto.FirstName,
                            LastName = playerDto.LastName,
                            Role = playerDto.Role,
                            Country = playerDto.Country,
                            ImageUrl = playerDto.ImageUrl,
                            CreatedAtUtc = now,
                            UpdatedAtUtc = now
                        };
                        _context.Players.Add(newPlayer);
                        existingPlayers[playerDto.ExternalId] = newPlayer;
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("İstatistikler, takımlar ve oyuncular başarıyla senkronize edildi.");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İstatistik senkronizasyonunda hata oluştu.");
            return Result.Failure(ex.Message);
        }
    }

    private async Task<int> UpsertMatchesAsync(IReadOnlyList<ProviderMatchDto> matchDtos, CancellationToken cancellationToken)
    {
        var games = await _context.Games.ToDictionaryAsync(g => g.Slug, g => g.Id, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var providerName = _dataProvider.ProviderName;

        // 1. İlgili maçları filtrele ve ExternalId bazında hafızada tekilleştir
        var relevantMatches = matchDtos
            .Where(m => !string.IsNullOrWhiteSpace(m.ExternalId) && IsMatchRelevant(m) && games.ContainsKey(m.GameSlug))
            .DistinctBy(m => m.ExternalId)
            .ToList();

        if (relevantMatches.Count == 0)
        {
            return 0;
        }

        // 2. Turnuvaları topla, tekilleştir ve Upsert et
        var distinctTournaments = relevantMatches
            .Where(m => m.Tournament != null && !string.IsNullOrWhiteSpace(m.Tournament.ExternalId))
            .Select(m => m.Tournament!)
            .DistinctBy(t => t.ExternalId)
            .ToList();

        var tournamentExtIds = distinctTournaments.Select(t => t.ExternalId).ToList();
        var existingTournaments = await _context.Tournaments
            .Where(t => t.Provider == providerName && tournamentExtIds.Contains(t.ExternalId))
            .ToDictionaryAsync(t => t.ExternalId, cancellationToken);

        foreach (var tourDto in distinctTournaments)
        {
            if (!games.TryGetValue(tourDto.GameSlug, out var gameId))
            {
                continue;
            }

            if (existingTournaments.TryGetValue(tourDto.ExternalId, out var existingTour))
            {
                existingTour.GameId = gameId;
                existingTour.Name = tourDto.Name;
                existingTour.Slug = tourDto.Slug;
                existingTour.Tier = tourDto.Tier;
                existingTour.Region = tourDto.Region;
                existingTour.Country = tourDto.Country;
                existingTour.LeagueSlug = tourDto.LeagueSlug;
                existingTour.StartDateUtc = tourDto.StartDateUtc;
                existingTour.EndDateUtc = tourDto.EndDateUtc;
                existingTour.UpdatedAtUtc = now;
            }
            else
            {
                var newTour = new Tournament
                {
                    Id = Guid.NewGuid(),
                    Provider = providerName,
                    ExternalId = tourDto.ExternalId,
                    GameId = gameId,
                    Name = tourDto.Name,
                    Slug = tourDto.Slug,
                    Tier = tourDto.Tier,
                    Region = tourDto.Region,
                    Country = tourDto.Country,
                    LeagueSlug = tourDto.LeagueSlug,
                    StartDateUtc = tourDto.StartDateUtc,
                    EndDateUtc = tourDto.EndDateUtc,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                };
                _context.Tournaments.Add(newTour);
                existingTournaments[tourDto.ExternalId] = newTour;
            }
        }

        // 3. Takımları topla, tekilleştir ve Upsert et
        var teamDtos = new List<ProviderTeamDto>();
        foreach (var match in relevantMatches)
        {
            if (match.Team1 != null && !string.IsNullOrWhiteSpace(match.Team1.ExternalId)) teamDtos.Add(match.Team1);
            if (match.Team2 != null && !string.IsNullOrWhiteSpace(match.Team2.ExternalId)) teamDtos.Add(match.Team2);
        }

        var distinctTeams = teamDtos.DistinctBy(t => t.ExternalId).ToList();
        var teamExtIds = distinctTeams.Select(t => t.ExternalId)
            .Concat(relevantMatches.Where(m => !string.IsNullOrEmpty(m.Team1ExternalId)).Select(m => m.Team1ExternalId!))
            .Concat(relevantMatches.Where(m => !string.IsNullOrEmpty(m.Team2ExternalId)).Select(m => m.Team2ExternalId!))
            .Concat(relevantMatches.Where(m => !string.IsNullOrEmpty(m.WinnerTeamExternalId)).Select(m => m.WinnerTeamExternalId!))
            .Distinct()
            .ToList();

        var existingTeams = await _context.Teams
            .Where(t => t.Provider == providerName && teamExtIds.Contains(t.ExternalId))
            .ToDictionaryAsync(t => t.ExternalId, cancellationToken);

        foreach (var teamDto in distinctTeams)
        {
            Guid? gameId = games.TryGetValue(teamDto.GameSlug, out var gId) ? gId : null;

            if (existingTeams.TryGetValue(teamDto.ExternalId, out var existingTeam))
            {
                if (gameId.HasValue && existingTeam.GameId == null)
                {
                    existingTeam.GameId = gameId;
                }
                existingTeam.Name = teamDto.Name;
                existingTeam.Slug = teamDto.Slug;
                existingTeam.Acronym = teamDto.Acronym;
                existingTeam.Country = teamDto.Country;
                existingTeam.LogoUrl = teamDto.LogoUrl;
                existingTeam.UpdatedAtUtc = now;
            }
            else
            {
                var newTeam = new Team
                {
                    Id = Guid.NewGuid(),
                    Provider = providerName,
                    ExternalId = teamDto.ExternalId,
                    GameId = gameId,
                    Name = teamDto.Name,
                    Slug = teamDto.Slug,
                    Acronym = teamDto.Acronym,
                    Country = teamDto.Country,
                    LogoUrl = teamDto.LogoUrl,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                };
                _context.Teams.Add(newTeam);
                existingTeams[teamDto.ExternalId] = newTeam;
            }
        }

        // 4. Oyuncuları topla, tekilleştir ve Upsert et
        var playerDtos = relevantMatches
            .Where(m => m.Players != null)
            .SelectMany(m => m.Players!)
            .Where(p => !string.IsNullOrWhiteSpace(p.ExternalId))
            .DistinctBy(p => p.ExternalId)
            .ToList();

        if (playerDtos.Count > 0)
        {
            var playerExtIds = playerDtos.Select(p => p.ExternalId).ToList();
            var existingPlayers = await _context.Players
                .Where(p => p.Provider == providerName && playerExtIds.Contains(p.ExternalId))
                .ToDictionaryAsync(p => p.ExternalId, cancellationToken);

            foreach (var pDto in playerDtos)
            {
                Guid? teamId = null;
                if (!string.IsNullOrEmpty(pDto.TeamExternalId) &&
                    existingTeams.TryGetValue(pDto.TeamExternalId, out var plTeam))
                {
                    teamId = plTeam.Id;
                }

                if (existingPlayers.TryGetValue(pDto.ExternalId, out var existingPl))
                {
                    if (teamId.HasValue)
                    {
                        existingPl.TeamId = teamId;
                    }
                    existingPl.Nickname = pDto.Nickname;
                    existingPl.FirstName = pDto.FirstName;
                    existingPl.LastName = pDto.LastName;
                    existingPl.Role = pDto.Role;
                    existingPl.Country = pDto.Country;
                    existingPl.ImageUrl = pDto.ImageUrl;
                    existingPl.UpdatedAtUtc = now;
                }
                else
                {
                    var newPl = new Player
                    {
                        Id = Guid.NewGuid(),
                        Provider = providerName,
                        ExternalId = pDto.ExternalId,
                        TeamId = teamId,
                        Nickname = pDto.Nickname,
                        FirstName = pDto.FirstName,
                        LastName = pDto.LastName,
                        Role = pDto.Role,
                        Country = pDto.Country,
                        ImageUrl = pDto.ImageUrl,
                        CreatedAtUtc = now,
                        UpdatedAtUtc = now
                    };
                    _context.Players.Add(newPl);
                    existingPlayers[pDto.ExternalId] = newPl;
                }
            }
        }

        // 5. Maçları Upsert et
        var matchExtIds = relevantMatches.Select(m => m.ExternalId).ToList();
        var existingMatches = await _context.Matches
            .Where(m => m.Provider == providerName && matchExtIds.Contains(m.ExternalId))
            .ToDictionaryAsync(m => m.ExternalId, cancellationToken);

        var processedCount = 0;
        foreach (var matchDto in relevantMatches)
        {
            var gameId = games[matchDto.GameSlug];

            // Turnuva referansı
            var tourExtId = matchDto.Tournament?.ExternalId ?? matchDto.TournamentExternalId;
            if (!existingTournaments.TryGetValue(tourExtId, out var tournament))
            {
                continue;
            }

            // Takım 1 referansı
            var team1ExtId = matchDto.Team1?.ExternalId ?? matchDto.Team1ExternalId;
            Guid? team1Id = null;
            if (!string.IsNullOrEmpty(team1ExtId) && existingTeams.TryGetValue(team1ExtId, out var t1))
            {
                team1Id = t1.Id;
            }

            // Takım 2 referansı
            var team2ExtId = matchDto.Team2?.ExternalId ?? matchDto.Team2ExternalId;
            Guid? team2Id = null;
            if (!string.IsNullOrEmpty(team2ExtId) && existingTeams.TryGetValue(team2ExtId, out var t2))
            {
                team2Id = t2.Id;
            }

            // Galip takım referansı
            Guid? winnerTeamId = null;
            if (!string.IsNullOrEmpty(matchDto.WinnerTeamExternalId) &&
                existingTeams.TryGetValue(matchDto.WinnerTeamExternalId, out var winnerTeam))
            {
                winnerTeamId = winnerTeam.Id;
            }

            if (existingMatches.TryGetValue(matchDto.ExternalId, out var match))
            {
                match.GameId = gameId;
                match.TournamentId = tournament.Id;
                match.Status = matchDto.Status;
                match.ScheduledAtUtc = matchDto.ScheduledAtUtc;
                match.StartedAtUtc = matchDto.StartedAtUtc;
                match.EndedAtUtc = matchDto.EndedAtUtc;
                match.BestOf = matchDto.BestOf;
                match.Team1Id = team1Id;
                match.Team2Id = team2Id;
                match.Team1Score = matchDto.Team1Score;
                match.Team2Score = matchDto.Team2Score;
                match.WinnerTeamId = winnerTeamId;
                match.UpdatedAtUtc = now;
            }
            else
            {
                match = new Match
                {
                    Id = Guid.NewGuid(),
                    Provider = providerName,
                    ExternalId = matchDto.ExternalId,
                    GameId = gameId,
                    TournamentId = tournament.Id,
                    Status = matchDto.Status,
                    ScheduledAtUtc = matchDto.ScheduledAtUtc,
                    StartedAtUtc = matchDto.StartedAtUtc,
                    EndedAtUtc = matchDto.EndedAtUtc,
                    BestOf = matchDto.BestOf,
                    Team1Id = team1Id,
                    Team2Id = team2Id,
                    Team1Score = matchDto.Team1Score,
                    Team2Score = matchDto.Team2Score,
                    WinnerTeamId = winnerTeamId,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                };
                _context.Matches.Add(match);
                existingMatches[matchDto.ExternalId] = match;
            }

            processedCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return processedCount;
    }

    private bool IsMatchRelevant(ProviderMatchDto matchDto)
    {
        // 1. Tournament tier is S or A
        if (matchDto.Tournament != null)
        {
            var tierString = matchDto.Tournament.Tier switch
            {
                TournamentTier.S => "S",
                TournamentTier.A => "A",
                TournamentTier.B => "B",
                TournamentTier.C => "C",
                TournamentTier.D => "D",
                _ => "Unranked"
            };

            if (_relevanceOptions.AllowedTiers.Any(t => string.Equals(t, tierString, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // 3. Tournament belongs to a Turkish league
            if (!string.IsNullOrEmpty(matchDto.Tournament.LeagueSlug) &&
                _relevanceOptions.TurkishLeagueSlugs.Any(slug => string.Equals(slug, matchDto.Tournament.LeagueSlug, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        // 2. At least one participant team is Turkish (country = TR)
        if (_relevanceOptions.IncludeTurkishTeams)
        {
            if (string.Equals(matchDto.Team1?.Country, "TR", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(matchDto.Team2?.Country, "TR", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
