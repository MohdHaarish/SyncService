using SyncService.Models;

namespace SyncService.DTOs;

public class SyncCallLogsDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public List<CallLog> CallLogs { get; set; } = new();
}

public class SyncMessagesDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public List<Message> Messages { get; set; } = new();
}

public class SyncAppNotificationsDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public List<AppNotification> AppNotifications { get; set; } = new();
}
