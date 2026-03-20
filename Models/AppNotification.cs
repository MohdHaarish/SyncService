using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncService.Models;

[Table("AppNotifications")]
public class AppNotification
{
    [Key]
    [Column(TypeName = "char(36)")]
    public Guid Id { get; set; }

    [Required]
    [Column(TypeName = "varchar(100)")]
    public string PackageName { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "varchar(255)")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "text")]
    public string Content { get; set; } = string.Empty;

    [Required]
    public long PostTime { get; set; }

    [Required]
    [Column(TypeName = "datetime(0)")]
    public DateTime PostTimeDateTime { get; set; }

    [Required]
    public int SyncStatus { get; set; } = 0; // 0 for pending, 1 for synced
}