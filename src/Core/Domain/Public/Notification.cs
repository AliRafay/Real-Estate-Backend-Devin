using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.WebApi.Domain.Public;

public class Notification : AuditableEntity, IAggregateRoot
{
    public string Title { get; set; } = default!;

    public string Description { get; set; } = default!;

    [ForeignKey(nameof(Image))]
    public int? ImageId { get; set; }

    public string IconLink { get; set; }

    public string? Url { get; set; }

    public int Code { get; set; }

    public virtual Document? Image { get; set; }
}
