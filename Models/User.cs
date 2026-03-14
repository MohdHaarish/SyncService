using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncService.Models;

[Table("Users")]
public class User
{
    [Key]
    [Column(TypeName = "char(36)")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column(TypeName = "varchar(50)")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "varchar(255)")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "varchar(100)")]
    public string Email { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}