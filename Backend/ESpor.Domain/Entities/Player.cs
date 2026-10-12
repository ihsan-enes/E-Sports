using ESpor.Domain.Common;

namespace ESpor.Domain.Entities;

public class Player : ExternalEntity
{
    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public string Nickname { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Role { get; set; }
    public string? Country { get; set; }
    public string? ImageUrl { get; set; }
}
