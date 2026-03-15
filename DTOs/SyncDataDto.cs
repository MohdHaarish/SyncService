using SyncService.Models;

namespace SyncService.DTOs;

public class SyncDataDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;

    public List<CallLog> CallLogs { get; set; } = new();
    public List<Message> Messages { get; set; } = new();
    public List<AppNotification> AppNotifications { get; set; } = new();
}