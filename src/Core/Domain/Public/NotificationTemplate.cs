using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.WebApi.Domain.Public;

public class NotificationTemplate : BaseEntity<int>, ISoftDelete
{
    public int Id { get; set; }

    public int Code { get; set; }

    public string Title { get; set; }

    public string Body { get; set; }

    public string IconLink { get; set; }

    [ForeignKey(nameof(Image))]
    public int? ImageId { get; set; }

    public string? Url { get; set; }

    public DateTime? DeletedOn { get; set; }

    public string? DeletedBy { get; set; }

    public Document? Image { get; set; }
}
