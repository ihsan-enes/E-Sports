namespace ESpor.Domain.Common;

public abstract class ExternalEntity : BaseEntity
{
    public string Provider { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
}
