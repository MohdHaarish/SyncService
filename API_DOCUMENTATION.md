# SyncService API Documentation

## Overview

SyncService is a .NET 8 Web API that provides synchronization of Android app data (CallLogs, Messages, AppNotifications) to a MySQL database with JWT authentication.

## Authentication

All protected endpoints require a JWT token in the Authorization header: `Bearer {token}`.

### Register User

**Endpoint:** `POST /api/auth/register`

**Description:** Register a new user account.

**Request Body:**
```json
{
  "username": "string",
  "password": "string",
  "email": "string"
}
```

**Response (Success - 200):**
```json
{
  "message": "User registered successfully."
}
```

**Response (Error - 400):**
```json
{
  "message": "User already exists." // or validation errors
}
```

### Login

**Endpoint:** `POST /api/auth/login`

**Description:** Authenticate user and get JWT token.

**Request Body:**
```json
{
  "username": "string",
  "password": "string"
}
```

**Response (Success - 200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response (Error - 401):**
```json
{
  "message": "Invalid username or password."
}
```

## Synchronization

### Sync All Data (Bulk)

**Endpoint:** `POST /api/sync/sync-all`

**Description:** Bulk synchronize CallLogs, Messages, and AppNotifications. Performs upsert operations based on ID and timestamp.

**Authorization:** Required (JWT Bearer token)

**Request Body:**
```json
{
  "deviceId": "string", // Identifier of the device sending this sync request
  "deviceName": "string", // Human-readable device model/name
  "callLogs": [
    {
      "id": "uuid",
      "callerNumber": "string", // Who is calling
      "calleeNumber": "string", // Who is being called
      "deviceId": "string", // Optional: device id that generated the call event
      "callType": 1,
      "timestamp": 1640995200000,
      "duration": 300,
      "callStatus": "Ended",
      "syncStatus": 0
    }
  ],
  "messages": [
    {
      "id": "uuid",
      "address": "string",
      "body": "string",
      "type": 1,
      "dateSent": 1640995200000,
      "syncStatus": 0
    }
  ],
  "appNotifications": [
    {
      "id": "uuid",
      "packageName": "string",
      "title": "string",
      "content": "string",
      "postTime": 1640995200000,
      "syncStatus": 0
    }
  ]
}
```

**Response (Success - 200):**
```json
{
  "message": "Data synced successfully."
}
```

**Response (Partial Success - 207):**
```json
{
  "message": "Data synced with errors.",
  "errors": [
    "CallLog {id} failed to sync.",
    "Message {id} failed to sync."
  ]
}
```

**Response (Error - 401):**
```json
{
  "message": "Unauthorized"
}
```

### Sync Call Logs

**Endpoint:** `POST /api/sync/call-logs`

**Description:** Synchronize CallLogs only. Performs upsert operations based on ID and timestamp.

**Authorization:** Required (JWT Bearer token)

**Request Body:**
```json
{
  "deviceId": "string",
  "deviceName": "string",
  "callLogs": [
    {
      "id": "uuid",
      "callerNumber": "string",
      "calleeNumber": "string",
      "deviceId": "string",
      "callType": 1,
      "timestamp": 1640995200000,
      "duration": 300,
      "callStatus": "Ended",
      "syncStatus": 0
    }
  ]
}
```

**Response (Success - 200):**
```json
{
  "message": "Call logs synced successfully."
}
```

**Response (Partial Success - 207):**
```json
{
  "message": "Call logs synced with errors.",
  "errors": ["CallLog {id} failed to sync."]
}
```

### Sync Messages

**Endpoint:** `POST /api/sync/messages`

**Description:** Synchronize Messages only. Performs upsert operations based on ID and timestamp.

**Authorization:** Required (JWT Bearer token)

**Request Body:**
```json
{
  "deviceId": "string",
  "deviceName": "string",
  "messages": [
    {
      "id": "uuid",
      "address": "string",
      "body": "string",
      "type": 1,
      "dateSent": 1640995200000,
      "syncStatus": 0
    }
  ]
}
```

**Response (Success - 200):**
```json
{
  "message": "Messages synced successfully."
}
```

**Response (Partial Success - 207):**
```json
{
  "message": "Messages synced with errors.",
  "errors": ["Message {id} failed to sync."]
}
```

### Sync App Notifications

**Endpoint:** `POST /api/sync/app-notifications`

**Description:** Synchronize AppNotifications only. Performs upsert operations based on ID and timestamp.

**Authorization:** Required (JWT Bearer token)

**Request Body:**
```json
{
  "deviceId": "string",
  "deviceName": "string",
  "appNotifications": [
    {
      "id": "uuid",
      "packageName": "string",
      "title": "string",
      "content": "string",
      "postTime": 1640995200000,
      "syncStatus": 0
    }
  ]
}
```

**Response (Success - 200):**
```json
{
  "message": "App notifications synced successfully."
}
```

**Response (Partial Success - 207):**
```json
{
  "message": "App notifications synced with errors.",
  "errors": ["AppNotification {id} failed to sync."]
}
```

## Data Models

### User
```json
{
  "id": "uuid",
  "username": "string",
  "password": "string (plain text)",
  "email": "string",
  "createdAt": "2023-01-01T00:00:00Z"
}
```

### CallLog
```json
{
  "id": "uuid",
  "callerNumber": "string", // Who is calling
  "calleeNumber": "string", // Who is being called
  "deviceId": "string", // Device identifier that reported the call event (optional)
  "callType": 1, // 1: Incoming, 2: Outgoing, 3: Missed
  "timestamp": 1640995200000, // Epoch milliseconds
  "duration": 300, // Seconds
  "callStatus": "string", // Ongoing, Ended, Rejected
  "syncStatus": 0 // 0: Pending, 1: Synced
}
```

### DeviceIdentifier
```json
{
  "id": "uuid",
  "deviceId": "string", // Unique identifier for the device
  "deviceName": "string", // Human-readable device name or model
  "lastSeen": 1640995200000 // Epoch milliseconds of last sync
}
```

### Message
```json
{
  "id": "uuid",
  "address": "string", // Sender/Receiver number
  "body": "string", // Message content
  "type": 1, // 1: Received, 2: Sent
  "dateSent": 1640995200000, // Epoch milliseconds
  "syncStatus": 0 // 0: Pending, 1: Synced
}
```

### AppNotification
```json
{
  "id": "uuid",
  "packageName": "string", // e.g., "com.whatsapp"
  "title": "string", // Notification title
  "content": "string", // Notification content
  "postTime": 1640995200000, // Epoch milliseconds
  "syncStatus": 0 // 0: Pending, 1: Synced
}
```

## Error Responses

### 400 Bad Request
```json
{
  "message": "Validation error message"
}
```

### 401 Unauthorized
```json
{
  "message": "Invalid username or password." // or "Unauthorized"
}
```

### 500 Internal Server Error
```json
{
  "message": "An unexpected error occurred."
}
```

## Database Schema

### Tables
- `Users`: User accounts
- `CallLogs`: Call log entries
- `Messages`: SMS/MMS messages
- `AppNotifications`: App notifications

### Key Points
- All IDs are GUIDs stored as CHAR(36)
- Timestamps are stored as BIGINT (epoch milliseconds)
- SyncStatus: 0 = pending, 1 = synced
- Upsert logic: Insert if ID doesn't exist, update if timestamp is newer

## Testing

1. Start the service: `dotnet run`
2. Access Swagger UI at the provided URL
3. Register a user via `/api/auth/register`
4. Login via `/api/auth/login` to obtain JWT token
5. Use the token to authorize and test sync endpoints

## Security Notes

- JWT tokens expire after 1 hour
- Passwords are currently stored in plain text (not recommended for production)
- All sync operations require valid authentication