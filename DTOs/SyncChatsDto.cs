using SyncService.Models;

namespace SyncService.DTOs;

public class SyncChatsDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public List<Chat> Chats { get; set; } = new();
}
