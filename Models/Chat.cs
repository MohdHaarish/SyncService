using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncService.Models;

[Table("Chats")]
public class Chat
{
    [Key]
    [Column(TypeName = "char(36)")]
    public Guid Id { get; set; }

    [Required]
    [Column(TypeName = "varchar(100)")]
    public string SentFrom { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "varchar(100)")]
    public string SendTo { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "text")]
    public string MessageContent { get; set; } = string.Empty;

    [Required]
    public long Timestamp { get; set; }

    [Required]
    [Column(TypeName = "datetime(0)")]
    public DateTime TimestampDateTime { get; set; }

    [Required]
    public bool HasSeen { get; set; } = false;

    [Required]
    public bool HasDelivered { get; set; } = false;

    [Required]
    public bool HasSent { get; set; } = false;

    [Required]
    public int SyncStatus { get; set; } = 0; // 0 for pending, 1 for synced
}
