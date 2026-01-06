# WowAchievementsApp - Overview

ASP.NET Core MVC web app integrating with Blizzard Battle.net API to display World of Warcraft character achievements. Uses OAuth 2.0 authentication.

## Tech Stack

- **.NET 10.0** / ASP.NET Core MVC
- **AspNet.Security.OAuth.BattleNet** (v7.0.0) - Battle.net OAuth
- **Newtonsoft.Json** (v13.0.4) - JSON serialization
- **Bootstrap/jQuery** - Frontend
- **xUnit/Moq** - Testing

## Architecture

MVC pattern:
- **Controllers**: `AccountController` (auth), `HomeController` (main logic)
- **Services**: `BlizzardService` (API integration)
- **Models**: Blizzard API response models, view models
- **Views**: Razor templates (Index, Achievements)

## Core Features

1. **OAuth Authentication**: Region selection → Battle.net login → access token stored in cookies
2. **Character Listing**: Fetches all WoW characters across accounts via `/profile/user/wow`
3. **Achievement Display**: Shows character achievements with pagination (100/page), sorted by completion
4. **Multi-Region**: Supports us, eu, kr, tw, cn

## Security

- **HTTPS-only**: Configured to listen exclusively on HTTPS port 5089 (`UseUrls("https://localhost:5089")`)
- **HSTS**: Enabled in all environments
- **Certificate**: Uses .NET development certificate (trust with `dotnet dev-certs https --trust`)
- **No HTTP**: HTTP connections are blocked; only HTTPS is available

## Configuration

```json
{
  "Blizzard": { "ClientId": "...", "ClientSecret": "..." },
  "BlizzardApi": { "BaseUrl": "https://{0}.api.blizzard.com" }
}
```

**Port**: 5089 (HTTPS only)

## Project Structure

```
WowAchievementsApp/
├── Controllers/     # AccountController, HomeController
├── Services/        # BlizzardService
├── Models/          # API models, ViewModels
├── Views/           # Razor views
└── wwwroot/         # Static files (Bootstrap, jQuery)
```

## API Endpoints

- `GET /profile/user/wow` - User profile with characters
- `GET /profile/wow/character/{realm}/{name}/achievements` - Character achievements

Both use Bearer token auth, namespace `profile-{region}`, locale `en_US`.

## Helper Scripts

- `docs/scripts/start.ps1` - Stop existing instance, start app
- `docs/scripts/stop.ps1` - Kill processes on port 5089
