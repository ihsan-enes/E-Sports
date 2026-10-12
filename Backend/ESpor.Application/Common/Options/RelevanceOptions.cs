namespace ESpor.Application.Common.Options;

public class RelevanceOptions
{
    public const string SectionName = "RelevanceOptions";

    public List<string> AllowedTiers { get; set; } = ["S", "A"];
    public bool IncludeTurkishTeams { get; set; } = true;
    public List<string> TurkishLeagueSlugs { get; set; } = [];
}
