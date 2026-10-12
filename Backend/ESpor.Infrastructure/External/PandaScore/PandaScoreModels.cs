using System.Text.Json.Serialization;

namespace ESpor.Infrastructure.External.PandaScore;

public class PandaScoreMatch
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("scheduled_at")]
    public DateTimeOffset? ScheduledAt { get; set; }

    [JsonPropertyName("begin_at")]
    public DateTimeOffset? BeginAt { get; set; }

    [JsonPropertyName("end_at")]
    public DateTimeOffset? EndAt { get; set; }

    [JsonPropertyName("number_of_games")]
    public int NumberOfGames { get; set; } = 1;

    [JsonPropertyName("winner_id")]
    public long? WinnerId { get; set; }

    [JsonPropertyName("videogame")]
    public PandaScoreVideogame? Videogame { get; set; }

    [JsonPropertyName("tournament")]
    public PandaScoreTournament? Tournament { get; set; }

    [JsonPropertyName("league")]
    public PandaScoreLeague? League { get; set; }

    [JsonPropertyName("opponents")]
    public List<PandaScoreOpponentWrapper>? Opponents { get; set; }

    [JsonPropertyName("results")]
    public List<PandaScoreMatchResult>? Results { get; set; }
}

public class PandaScoreOpponentWrapper
{
    [JsonPropertyName("opponent")]
    public PandaScoreTeam? Opponent { get; set; }
}

public class PandaScoreMatchResult
{
    [JsonPropertyName("team_id")]
    public long TeamId { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }
}

public class PandaScoreTournament
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("tier")]
    public string? Tier { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("begin_at")]
    public DateTimeOffset? BeginAt { get; set; }

    [JsonPropertyName("end_at")]
    public DateTimeOffset? EndAt { get; set; }

    [JsonPropertyName("league")]
    public PandaScoreLeague? League { get; set; }
}

public class PandaScoreLeague
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }
}

public class PandaScoreTeam
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("acronym")]
    public string? Acronym { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("players")]
    public List<PandaScorePlayer>? Players { get; set; }
}

public class PandaScorePlayer
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("nationality")]
    public string? Nationality { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("current_team")]
    public PandaScoreTeam? CurrentTeam { get; set; }
}

public class PandaScoreVideogame
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }
}
