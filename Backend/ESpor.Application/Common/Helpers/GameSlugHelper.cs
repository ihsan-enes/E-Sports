namespace ESpor.Application.Common.Helpers;

public static class GameSlugHelper
{
    public static string? NormalizeGameSlug(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var normalized = slug.Trim().ToLowerInvariant();

        return normalized switch
        {
            "lol" or "league-of-legends" => "league-of-legends",
            "cs2" or "cs-2" or "csgo" => "cs-2",
            "valorant" or "val" => "valorant",
            _ => normalized
        };
    }
}
