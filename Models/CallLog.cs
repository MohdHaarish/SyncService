using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncService.Models;

[Table("CallLogs")]
public class CallLog
{
    [Key]
    [Column(TypeName = "char(36)")]
    public Guid Id { get; set; }

    [Required]
    [Column(TypeName = "varchar(100)")]
    public string CallerNumber { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "varchar(100)")]
    public string CalleeNumber { get; set; } = string.Empty;

    [Column(TypeName = "varchar(100)")]
    public string? DeviceId { get; set; }

    [Required]
    public int CallType { get; set; }

    [Required]
    public long Timestamp { get; set; }

    [Required]
    [Column(TypeName = "datetime(0)")]
    public DateTime TimestampDateTime { get; set; }

    [Required]
    public long Duration { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    public string CallStatus { get; set; } = string.Empty;

    [Required]
    public int SyncStatus { get; set; } = 0; // 0 for pending, 1 for synced
}