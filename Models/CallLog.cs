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
    [Column(TypeName = "varchar(20)")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public int CallType { get; set; }

    [Required]
    public long Timestamp { get; set; }

    [Required]
    public long Duration { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    public string CallStatus { get; set; } = string.Empty;

    [Required]
    public int SyncStatus { get; set; } = 0; // 0 for pending, 1 for synced
}