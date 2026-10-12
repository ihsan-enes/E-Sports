using ESpor.Application.Common;
using ESpor.Application.Common.Interfaces;
using ESpor.Application.Common.Options;
using ESpor.Application.External.Interfaces;
using ESpor.Application.External.Models;
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

            foreach (var game in games)
            {
                var teams = await _dataProvider.GetTeamsAsync(game.Slug, cancellationToken);
                foreach (var teamDto in teams)
                {
                    await UpsertTeamAsync(teamDto, game.Id, now, cancellationToken);
                }

                var players = await _dataProvider.GetPlayersAsync(game.Slug, cancellationToken);
                foreach (var playerDto in players)
                {
                    await UpsertPlayerAsync(playerDto, now, cancellationToken);
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
        var relevantCount = 0;

        foreach (var matchDto in matchDtos)
        {
            if (!IsMatchRelevant(matchDto))
            {
                continue;
            }

            if (!games.TryGetValue(matchDto.GameSlug, out var gameId))
            {
                continue;
            }

            // Tournament upsert
            Tournament? tournament = null;
            if (matchDto.Tournament != null)
            {
                tournament = await UpsertTournamentAsync(matchDto.Tournament, gameId, now, cancellationToken);
            }
            else
            {
                tournament = await _context.Tournaments
                    .FirstOrDefaultAsync(t => t.Provider == _dataProvider.ProviderName && t.ExternalId == matchDto.TournamentExternalId, cancellationToken);
            }

            if (tournament == null)
            {
                continue;
            }

            // Team 1 upsert
            Guid? team1Id = null;
            if (matchDto.Team1 != null)
            {
                var team1 = await UpsertTeamAsync(matchDto.Team1, gameId, now, cancellationToken);
                team1Id = team1.Id;
            }
            else if (!string.IsNullOrEmpty(matchDto.Team1ExternalId))
            {
                var team1 = await _context.Teams
                    .FirstOrDefaultAsync(t => t.Provider == _dataProvider.ProviderName && t.ExternalId == matchDto.Team1ExternalId, cancellationToken);
                team1Id = team1?.Id;
            }

            // Team 2 upsert
            Guid? team2Id = null;
            if (matchDto.Team2 != null)
            {
                var team2 = await UpsertTeamAsync(matchDto.Team2, gameId, now, cancellationToken);
                team2Id = team2.Id;
            }
            else if (!string.IsNullOrEmpty(matchDto.Team2ExternalId))
            {
                var team2 = await _context.Teams
                    .FirstOrDefaultAsync(t => t.Provider == _dataProvider.ProviderName && t.ExternalId == matchDto.Team2ExternalId, cancellationToken);
                team2Id = team2?.Id;
            }

            // Winner Team
            Guid? winnerTeamId = null;
            if (!string.IsNullOrEmpty(matchDto.WinnerTeamExternalId))
            {
                if (matchDto.Team1 != null && matchDto.Team1.ExternalId == matchDto.WinnerTeamExternalId)
                {
                    winnerTeamId = team1Id;
                }
                else if (matchDto.Team2 != null && matchDto.Team2.ExternalId == matchDto.WinnerTeamExternalId)
                {
                    winnerTeamId = team2Id;
                }
                else
                {
                    var winnerTeam = await _context.Teams
                        .FirstOrDefaultAsync(t => t.Provider == _dataProvider.ProviderName && t.ExternalId == matchDto.WinnerTeamExternalId, cancellationToken);
                    winnerTeamId = winnerTeam?.Id;
                }
            }

            // Players upsert
            if (matchDto.Players != null)
            {
                foreach (var playerDto in matchDto.Players)
                {
                    await UpsertPlayerAsync(playerDto, now, cancellationToken);
                }
            }

            // Match upsert
            var match = await _context.Matches
                .FirstOrDefaultAsync(m => m.Provider == _dataProvider.ProviderName && m.ExternalId == matchDto.ExternalId, cancellationToken);

            if (match == null)
            {
                match = new Match
                {
                    Id = Guid.NewGuid(),
                    Provider = _dataProvider.ProviderName,
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
            }
            else
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

            relevantCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return relevantCount;
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

    private async Task<Tournament> UpsertTournamentAsync(
        ProviderTournamentDto dto,
        Guid gameId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var tournament = await _context.Tournaments
            .FirstOrDefaultAsync(t => t.Provider == _dataProvider.ProviderName && t.ExternalId == dto.ExternalId, cancellationToken);

        if (tournament == null)
        {
            tournament = new Tournament
            {
                Id = Guid.NewGuid(),
                Provider = _dataProvider.ProviderName,
                ExternalId = dto.ExternalId,
                GameId = gameId,
                Name = dto.Name,
                Slug = dto.Slug,
                Tier = dto.Tier,
                Region = dto.Region,
                Country = dto.Country,
                LeagueSlug = dto.LeagueSlug,
                StartDateUtc = dto.StartDateUtc,
                EndDateUtc = dto.EndDateUtc,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            _context.Tournaments.Add(tournament);
        }
        else
        {
            tournament.GameId = gameId;
            tournament.Name = dto.Name;
            tournament.Slug = dto.Slug;
            tournament.Tier = dto.Tier;
            tournament.Region = dto.Region;
            tournament.Country = dto.Country;
            tournament.LeagueSlug = dto.LeagueSlug;
            tournament.StartDateUtc = dto.StartDateUtc;
            tournament.EndDateUtc = dto.EndDateUtc;
            tournament.UpdatedAtUtc = now;
        }

        return tournament;
    }

    private async Task<Team> UpsertTeamAsync(
        ProviderTeamDto dto,
        Guid? gameId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var team = await _context.Teams
            .FirstOrDefaultAsync(t => t.Provider == _dataProvider.ProviderName && t.ExternalId == dto.ExternalId, cancellationToken);

        if (team == null)
        {
            team = new Team
            {
                Id = Guid.NewGuid(),
                Provider = _dataProvider.ProviderName,
                ExternalId = dto.ExternalId,
                GameId = gameId,
                Name = dto.Name,
                Slug = dto.Slug,
                Acronym = dto.Acronym,
                Country = dto.Country,
                LogoUrl = dto.LogoUrl,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            _context.Teams.Add(team);
        }
        else
        {
            if (gameId.HasValue && team.GameId == null)
            {
                team.GameId = gameId;
            }
            team.Name = dto.Name;
            team.Slug = dto.Slug;
            team.Acronym = dto.Acronym;
            team.Country = dto.Country;
            team.LogoUrl = dto.LogoUrl;
            team.UpdatedAtUtc = now;
        }

        return team;
    }

    private async Task<Player> UpsertPlayerAsync(
        ProviderPlayerDto dto,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        Guid? teamId = null;
        if (!string.IsNullOrEmpty(dto.TeamExternalId))
        {
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Provider == _dataProvider.ProviderName && t.ExternalId == dto.TeamExternalId, cancellationToken);
            teamId = team?.Id;
        }

        var player = await _context.Players
            .FirstOrDefaultAsync(p => p.Provider == _dataProvider.ProviderName && p.ExternalId == dto.ExternalId, cancellationToken);

        if (player == null)
        {
            player = new Player
            {
                Id = Guid.NewGuid(),
                Provider = _dataProvider.ProviderName,
                ExternalId = dto.ExternalId,
                TeamId = teamId,
                Nickname = dto.Nickname,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = dto.Role,
                Country = dto.Country,
                ImageUrl = dto.ImageUrl,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            _context.Players.Add(player);
        }
        else
        {
            if (teamId.HasValue)
            {
                player.TeamId = teamId;
            }
            player.Nickname = dto.Nickname;
            player.FirstName = dto.FirstName;
            player.LastName = dto.LastName;
            player.Role = dto.Role;
            player.Country = dto.Country;
            player.ImageUrl = dto.ImageUrl;
            player.UpdatedAtUtc = now;
        }

        return player;
    }
}
