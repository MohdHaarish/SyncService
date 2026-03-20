using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncService.Models;

[Table("DeviceIdentifiers")]
public class DeviceIdentifier
{
    [Key]
    [Column(TypeName = "char(36)")]
    public Guid Id { get; set; }

    [Required]
    [Column(TypeName = "varchar(100)")]
    public string DeviceId { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "varchar(100)")]
    public string DeviceName { get; set; } = string.Empty;

    [Required]
    public long LastSeen { get; set; }

    [Required]
    [Column(TypeName = "datetime(0)")]
    public DateTime LastSeenDateTime { get; set; }
}
