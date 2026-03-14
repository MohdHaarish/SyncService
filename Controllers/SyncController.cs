using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    public SyncController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("sync-all")]
    public async Task<IActionResult> SyncAll([FromBody] SyncDataDto syncData)
    {
        if (syncData == null)
        {
            return BadRequest("Sync data is required.");
        }

        // Process CallLogs
        foreach (var callLog in syncData.CallLogs)
        {
            await UpsertCallLog(callLog);
        }

        // Process Messages
        foreach (var message in syncData.Messages)
        {
            await UpsertMessage(message);
        }

        // Process AppNotifications
        foreach (var notification in syncData.AppNotifications)
        {
            await UpsertAppNotification(notification);
        }

        await _context.SaveChangesAsync();

        return Ok("Data synced successfully.");
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
                existing.PhoneNumber = incoming.PhoneNumber;
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