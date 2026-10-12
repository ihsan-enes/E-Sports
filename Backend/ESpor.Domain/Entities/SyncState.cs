namespace ESpor.Domain.Entities;

public class SyncState
{
    public string JobName { get; set; } = string.Empty;
    public DateTimeOffset? LastSuccessUtc { get; set; }
    public DateTimeOffset? LastAttemptUtc { get; set; }
    public string? LastError { get; set; }
}
