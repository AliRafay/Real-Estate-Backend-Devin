using Demo.WebApi.Domain.Common.Enums;

namespace Demo.WebApi.Domain.Identity;

public class UserSession : AuditableEntity, IAggregateRoot
{
    public virtual string? ApplicationUserId { get; set; }

    public string? Token { get; set; }

    public string? DeviceId { get; set; }

    public string? FCMToken { get; set; }

    public string? DeviceName { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public SessionType Type { get; set; }

    public bool IsRemember { get; set; }

    public bool ExplicitLogout { get; set; }

    public string? Version { get; set; }

    public virtual ApplicationUser? ApplicationUser { get; set; }
}
