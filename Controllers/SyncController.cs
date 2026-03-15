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

    [HttpPost("sync-all")]
    public async Task<IActionResult> SyncAll([FromBody] SyncDataDto syncData)
    {
        if (syncData == null)
        {
            _logger.LogWarning("SyncAll called with null payload.");
            return BadRequest("Sync data is required.");
        }

        _logger.LogInformation("SyncAll called from device {DeviceId} ({DeviceName}) with {CallLogCount} call logs, {MessageCount} messages, {NotificationCount} notifications.",
            syncData.DeviceId,
            syncData.DeviceName,
            syncData.CallLogs.Count,
            syncData.Messages.Count,
            syncData.AppNotifications.Count);

        var errors = new List<string>();

        await SafeUpsertDeviceIdentifier(syncData.DeviceId, syncData.DeviceName, errors);

        // Process CallLogs
        foreach (var callLog in syncData.CallLogs)
        {
            await SafeUpsertCallLog(callLog, syncData.DeviceId, errors);
        }

        // Process Messages
        foreach (var message in syncData.Messages)
        {
            await SafeUpsertMessage(message, errors);
        }

        // Process AppNotifications
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
            _logger.LogError(ex, "Error saving changes after sync from device {DeviceId} ({DeviceName}).", syncData.DeviceId, syncData.DeviceName);
            errors.Add("Failed to save changes to the database.");
        }

        if (errors.Count > 0)
        {
            _logger.LogWarning("SyncAll completed with {ErrorCount} errors from device {DeviceId} ({DeviceName}).", errors.Count, syncData.DeviceId, syncData.DeviceName);
            return StatusCode(207, new { message = "Data synced with errors.", errors });
        }

        return Ok("Data synced successfully.");
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

        var existing = await _context.DeviceIdentifiers.FirstOrDefaultAsync(d => d.DeviceId == deviceId);
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (existing == null)
        {
            _context.DeviceIdentifiers.Add(new DeviceIdentifier
            {
                Id = Guid.NewGuid(),
                DeviceId = deviceId,
                DeviceName = deviceName,
                LastSeen = now
            });
        }
        else
        {
            existing.DeviceName = deviceName;
            existing.LastSeen = now;
        }
    }

    private async Task UpsertCallLog(CallLog incoming)
    {
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
                existing.Duration = incoming.Duration;
                existing.CallStatus = incoming.CallStatus;
                existing.SyncStatus = 1; // Mark as synced
            }
        }
    }

    private async Task UpsertMessage(Message incoming)
    {
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
                existing.SyncStatus = 1; // Mark as synced
            }
        }
    }

    private async Task UpsertAppNotification(AppNotification incoming)
    {
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
                existing.SyncStatus = 1; // Mark as synced
            }
        }
    }
}