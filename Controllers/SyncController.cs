using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SyncService.Data;
using SyncService.DTOs;
using SyncService.Models;

namespace SyncService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SyncController> _logger;

    public SyncController(ApplicationDbContext context, ILogger<SyncController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("call-logs")]
    public async Task<IActionResult> SyncCallLogs([FromBody] SyncCallLogsDto syncData)
    {
        if (syncData == null || syncData.CallLogs == null)
        {
            _logger.LogWarning("SyncCallLogs called with null payload.");
            return BadRequest("Call logs data is required.");
        }

        _logger.LogInformation("SyncCallLogs called from device {DeviceId} ({DeviceName}) with {CallLogCount} call logs.",
            syncData.DeviceId,
            syncData.DeviceName,
            syncData.CallLogs.Count);

        var errors = new List<string>();

        await SafeUpsertDeviceIdentifier(syncData.DeviceId, syncData.DeviceName, errors);

        foreach (var callLog in syncData.CallLogs)
        {
            await SafeUpsertCallLog(callLog, syncData.DeviceId, errors);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes after sync call logs from device {DeviceId} ({DeviceName}).", syncData.DeviceId, syncData.DeviceName);
            errors.Add("Failed to save changes to the database.");
        }

        if (errors.Count > 0)
        {
            _logger.LogWarning("SyncCallLogs completed with {ErrorCount} errors from device {DeviceId} ({DeviceName}).", errors.Count, syncData.DeviceId, syncData.DeviceName);
            return StatusCode(207, new { message = "Call logs synced with errors.", errors });
        }

        return Ok("Call logs synced successfully.");
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SyncMessages([FromBody] SyncMessagesDto syncData)
    {
        if (syncData == null || syncData.Messages == null)
        {
            _logger.LogWarning("SyncMessages called with null payload.");
            return BadRequest("Messages data is required.");
        }

        _logger.LogInformation("SyncMessages called from device {DeviceId} ({DeviceName}) with {MessageCount} messages.",
            syncData.DeviceId,
            syncData.DeviceName,
            syncData.Messages.Count);

        var errors = new List<string>();

        await SafeUpsertDeviceIdentifier(syncData.DeviceId, syncData.DeviceName, errors);

        foreach (var message in syncData.Messages)
        {
            await SafeUpsertMessage(message, errors);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes after sync messages from device {DeviceId} ({DeviceName}).", syncData.DeviceId, syncData.DeviceName);
            errors.Add("Failed to save changes to the database.");
        }

        if (errors.Count > 0)
        {
            _logger.LogWarning("SyncMessages completed with {ErrorCount} errors from device {DeviceId} ({DeviceName}).", errors.Count, syncData.DeviceId, syncData.DeviceName);
            return StatusCode(207, new { message = "Messages synced with errors.", errors });
        }

        return Ok("Messages synced successfully.");
    }

    [HttpPost("app-notifications")]
    public async Task<IActionResult> SyncAppNotifications([FromBody] SyncAppNotificationsDto syncData)
    {
        if (syncData == null || syncData.AppNotifications == null)
        {
            _logger.LogWarning("SyncAppNotifications called with null payload.");
            return BadRequest("App notifications data is required.");
        }

        _logger.LogInformation("SyncAppNotifications called from device {DeviceId} ({DeviceName}) with {NotificationCount} app notifications.",
            syncData.DeviceId,
            syncData.DeviceName,
            syncData.AppNotifications.Count);

        var errors = new List<string>();

        await SafeUpsertDeviceIdentifier(syncData.DeviceId, syncData.DeviceName, errors);

        foreach (var notification in syncData.AppNotifications)
        {
            await SafeUpsertAppNotification(notification, errors);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes after sync app notifications from device {DeviceId} ({DeviceName}).", syncData.DeviceId, syncData.DeviceName);
            errors.Add("Failed to save changes to the database.");
        }

        if (errors.Count > 0)
        {
            _logger.LogWarning("SyncAppNotifications completed with {ErrorCount} errors from device {DeviceId} ({DeviceName}).", errors.Count, syncData.DeviceId, syncData.DeviceName);
            return StatusCode(207, new { message = "App notifications synced with errors.", errors });
        }

        return Ok("App notifications synced successfully.");
    }

    private async Task SafeUpsertDeviceIdentifier(string deviceId, string deviceName, List<string> errors)
    {
        try
        {
            await UpsertDeviceIdentifier(deviceId, deviceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert device identifier {DeviceId} ({DeviceName}).", deviceId, deviceName);
            errors.Add("Device identifier upsert failed.");
        }
    }

    private async Task SafeUpsertCallLog(CallLog incoming, string deviceId, List<string> errors)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(incoming.DeviceId) && !string.IsNullOrWhiteSpace(deviceId))
            {
                incoming.DeviceId = deviceId;
            }

            await UpsertCallLog(incoming);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert call log {CallLogId} from device {DeviceId}.", incoming.Id, deviceId);
            errors.Add($"CallLog {incoming.Id} failed to sync.");
        }
    }

    private async Task SafeUpsertMessage(Message incoming, List<string> errors)
    {
        try
        {
            await UpsertMessage(incoming);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert message {MessageId}.", incoming.Id);
            errors.Add($"Message {incoming.Id} failed to sync.");
        }
    }

    private async Task SafeUpsertAppNotification(AppNotification incoming, List<string> errors)
    {
        try
        {
            await UpsertAppNotification(incoming);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert app notification {NotificationId}.", incoming.Id);
            errors.Add($"AppNotification {incoming.Id} failed to sync.");
        }
    }

    private async Task UpsertDeviceIdentifier(string deviceId, string deviceName)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var lastSeenDateTime = TruncateToSeconds(now.UtcDateTime);
        var lastSeenUnixMs = new DateTimeOffset(lastSeenDateTime).ToUnixTimeMilliseconds();

        var existing = await _context.DeviceIdentifiers.FirstOrDefaultAsync(d => d.DeviceId == deviceId);

        if (existing == null)
        {
            _context.DeviceIdentifiers.Add(new DeviceIdentifier
            {
                Id = Guid.NewGuid(),
                DeviceId = deviceId,
                DeviceName = deviceName,
                LastSeen = lastSeenUnixMs,
                LastSeenDateTime = lastSeenDateTime
            });
        }
        else
        {
            existing.DeviceName = deviceName;
            existing.LastSeen = lastSeenUnixMs;
            existing.LastSeenDateTime = lastSeenDateTime;
        }
    }

    private async Task UpsertCallLog(CallLog incoming)
    {
        NormalizeTimestamps(incoming);

        var existing = await _context.CallLogs.FindAsync(incoming.Id);
        if (existing == null)
        {
            // Insert
            incoming.SyncStatus = 1; // Mark as synced
            _context.CallLogs.Add(incoming);
        }
        else
        {
            // Update only if incoming timestamp is more recent
            if (incoming.Timestamp > existing.Timestamp)
            {
                existing.CallerNumber = string.IsNullOrEmpty(incoming.CallerNumber) ? existing.CallerNumber : incoming.CallerNumber;
                existing.CalleeNumber = string.IsNullOrEmpty(incoming.CalleeNumber) ? existing.CalleeNumber : incoming.CalleeNumber;
                existing.DeviceId = string.IsNullOrEmpty(incoming.DeviceId) ? existing.DeviceId : incoming.DeviceId;
                existing.CallType = incoming.CallType;
                existing.Timestamp = incoming.Timestamp;
                existing.TimestampDateTime = incoming.TimestampDateTime;
                existing.Duration = incoming.Duration;
                existing.CallStatus = incoming.CallStatus;
                existing.SyncStatus = 1; // Mark as synced
            }
        }
    }

    private async Task UpsertMessage(Message incoming)
    {
        NormalizeMessageTimestamps(incoming);

        var existing = await _context.Messages.FindAsync(incoming.Id);
        if (existing == null)
        {
            // Insert
            incoming.SyncStatus = 1; // Mark as synced
            _context.Messages.Add(incoming);
        }
        else
        {
            // Update only if incoming date_sent is more recent
            if (incoming.DateSent > existing.DateSent)
            {
                existing.Address = incoming.Address;
                existing.Body = incoming.Body;
                existing.Type = incoming.Type;
                existing.DateSent = incoming.DateSent;
                existing.DateSentDateTime = incoming.DateSentDateTime;
                existing.SyncStatus = 1; // Mark as synced
            }
        }
    }

    private async Task UpsertAppNotification(AppNotification incoming)
    {
        NormalizeAppNotificationTimestamps(incoming);

        var existing = await _context.AppNotifications.FindAsync(incoming.Id);
        if (existing == null)
        {
            // Insert
            incoming.SyncStatus = 1; // Mark as synced
            _context.AppNotifications.Add(incoming);
        }
        else
        {
            // Update only if incoming post_time is more recent
            if (incoming.PostTime > existing.PostTime)
            {
                existing.PackageName = incoming.PackageName;
                existing.Title = incoming.Title;
                existing.Content = incoming.Content;
                existing.PostTime = incoming.PostTime;
                existing.PostTimeDateTime = incoming.PostTimeDateTime;
                existing.SyncStatus = 1; // Mark as synced
            }
        }
    }

    private void NormalizeTimestamps(CallLog callLog)
    {
        if (callLog.TimestampDateTime == default && callLog.Timestamp > 0)
        {
            callLog.TimestampDateTime = ToUtcDateTime(callLog.Timestamp);
        }

        if (callLog.TimestampDateTime != default && callLog.Timestamp <= 0)
        {
            callLog.Timestamp = ToUnixMilliseconds(callLog.TimestampDateTime);
        }

        callLog.TimestampDateTime = TruncateToSeconds(callLog.TimestampDateTime);
        callLog.Timestamp = ToUnixMilliseconds(callLog.TimestampDateTime);
    }

    private void NormalizeMessageTimestamps(Message message)
    {
        if (message.DateSentDateTime == default && message.DateSent > 0)
        {
            message.DateSentDateTime = ToUtcDateTime(message.DateSent);
        }

        if (message.DateSentDateTime != default && message.DateSent <= 0)
        {
            message.DateSent = ToUnixMilliseconds(message.DateSentDateTime);
        }

        message.DateSentDateTime = TruncateToSeconds(message.DateSentDateTime);
        message.DateSent = ToUnixMilliseconds(message.DateSentDateTime);
    }

    private void NormalizeAppNotificationTimestamps(AppNotification notification)
    {
        if (notification.PostTimeDateTime == default && notification.PostTime > 0)
        {
            notification.PostTimeDateTime = ToUtcDateTime(notification.PostTime);
        }

        if (notification.PostTimeDateTime != default && notification.PostTime <= 0)
        {
            notification.PostTime = ToUnixMilliseconds(notification.PostTimeDateTime);
        }

        notification.PostTimeDateTime = TruncateToSeconds(notification.PostTimeDateTime);
        notification.PostTime = ToUnixMilliseconds(notification.PostTimeDateTime);
    }

    private void NormalizeDeviceIdentifierTimestamps(DeviceIdentifier deviceIdentifier)
    {
        if (deviceIdentifier.LastSeenDateTime == default && deviceIdentifier.LastSeen > 0)
        {
            deviceIdentifier.LastSeenDateTime = ToUtcDateTime(deviceIdentifier.LastSeen);
        }

        if (deviceIdentifier.LastSeenDateTime != default && deviceIdentifier.LastSeen <= 0)
        {
            deviceIdentifier.LastSeen = ToUnixMilliseconds(deviceIdentifier.LastSeenDateTime);
        }

        deviceIdentifier.LastSeenDateTime = TruncateToSeconds(deviceIdentifier.LastSeenDateTime);
        deviceIdentifier.LastSeen = ToUnixMilliseconds(deviceIdentifier.LastSeenDateTime);
    }

    private static long ToUnixMilliseconds(DateTime dateTime)
    {
        var utc = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();
        utc = TruncateToSeconds(utc);
        return new DateTimeOffset(utc).ToUnixTimeMilliseconds();
    }

    private static DateTime ToUtcDateTime(long unixMilliseconds)
    {
        try
        {
            return TruncateToSeconds(DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).UtcDateTime);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    private static DateTime TruncateToSeconds(DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue) return dateTime;

        var ticks = dateTime.Kind == DateTimeKind.Utc ? dateTime.Ticks : dateTime.ToUniversalTime().Ticks;
        var truncated = new DateTime(ticks - (ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
        return truncated;
    }
}
