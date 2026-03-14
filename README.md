# SyncService

A .NET 8 Web API for synchronizing Android app data (CallLogs, Messages, AppNotifications) to MySQL database.

## Features

- JWT Authentication with user registration/login
- Bulk sync endpoint for CallLogs, Messages, and AppNotifications
- Upsert logic based on ID and timestamp
- Entity Framework Core with Pomelo MySQL provider
- Production-ready with HTTPS support

## Quick Start

1. Set environment variables: `DB_USER=your_db_user` and `DB_PASSWORD=your_db_password`
2. Update other settings in `appsettings.json` if needed.
3. Run migrations: `dotnet ef database update`
4. Run the application: `dotnet run`

## Production Deployment

1. Set environment variables: `DB_USER=prod_db_user` `DB_PASSWORD=prod_db_password` `ASPNETCORE_ENVIRONMENT=Production`
2. Configure SSL certificate in `appsettings.Production.json`
3. Update JWT key to a strong secret
4. Deploy to server with reverse proxy (nginx/IIS) handling SSL termination, or configure Kestrel with certificate
5. Run: `dotnet SyncService.dll`

## API Documentation

See [API_DOCUMENTATION.md](API_DOCUMENTATION.md) for detailed API reference including endpoints, request/response examples, and data models.

## Testing

1. Run the application: `dotnet run`
2. Open Swagger UI at the displayed URL.
3. Register a user with `/api/auth/register`.
4. Login with `/api/auth/login` to get a JWT token.
5. Click "Authorize" in Swagger UI and enter `Bearer {token}`.
6. Test the `/api/sync/sync-all` endpoint with sample data.