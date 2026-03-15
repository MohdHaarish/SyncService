using Microsoft.EntityFrameworkCore;
using SyncService.Models;

namespace SyncService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CallLog> CallLogs { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<AppNotification> AppNotifications { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<DeviceIdentifier> DeviceIdentifiers { get; set; }
}