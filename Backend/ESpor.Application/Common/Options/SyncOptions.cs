namespace ESpor.Application.Common.Options;

public class SyncOptions
{
    public const string SectionName = "SyncOptions";

    public int LiveIntervalSeconds { get; set; } = 60;
    public int FixturesIntervalHours { get; set; } = 24;
    public int StatsIntervalDays { get; set; } = 7;
}
