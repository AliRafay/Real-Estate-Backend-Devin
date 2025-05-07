using Demo.WebApi.Application.Common.Interfaces;
using Demo.WebApi.Domain.Public;
using Demo.WebApi.Infrastructure.Persistence.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Demo.WebApi.Infrastructure.Persistence.Context;

public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(DbContextOptions options, ICurrentUser currentUser, ISerializerService serializer, IOptions<DatabaseSettings> dbSettings)
        : base(options, currentUser, serializer, dbSettings)
    {
    }

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();

    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    public DbSet<Document> Documents => Set<Document>();
    
    public DbSet<Buyer> Buyers => Set<Buyer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaNames.Public);
    }
}
