using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncService.Models;

[Table("Messages")]
public class Message
{
    [Key]
    [Column(TypeName = "char(36)")]
    public Guid Id { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "text")]
    public string Body { get; set; } = string.Empty;

    [Required]
    public int Type { get; set; }

    [Required]
    public long DateSent { get; set; }

    [Required]
    public int SyncStatus { get; set; } = 0; // 0 for pending, 1 for synced
}