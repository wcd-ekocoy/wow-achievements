# WoW Achievements App - Debugging Session Summary

## Overview
This document summarizes the debugging session for the WoW Achievements application, focusing on fixing character fetching and achievements loading issues.

## Initial Problem
The application was not fetching characters from the Blizzard API after user authentication. The page showed "No characters found" even though the user was logged in.

## Issues Discovered and Fixed

### 1. Character Fetching Issues

#### Problem 1: HttpClient Authorization Header Bug
**Issue**: The code was setting `DefaultRequestHeaders.Authorization` directly on the HttpClient instance, which can fail when the client is reused (as with `AddHttpClient` registration).

**Solution**: Changed to set the Authorization header per-request using `HttpRequestMessage`:
```csharp
var request = new HttpRequestMessage(HttpMethod.Get, url);
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
var response = await _httpClient.SendAsync(request);
```

#### Problem 2: Model Structure Mismatch
**Issue**: The JSON response structure from Blizzard API didn't match our C# models. The API returns nested objects:
- `realm.key` is an object `{"href":"..."}`, not a string
- `playable_class` and `playable_race` have nested `key` objects
- `gender` and `faction` use `type` and `name` properties, not `id` and `key`

**Solution**: Updated models to match the actual API response:
- Created `RealmInfo` class with `Key` (LinkObject), `Name`, `Id`, and `Slug`
- Created `PlayableInfo` class for class/race with nested `Key` object
- Created `TypeNamePair` class for gender/faction with `Type` and `Name` properties
- Added proper `[JsonProperty]` attributes with snake_case names (e.g., `wow_accounts`, `playable_race`)

#### Problem 3: Missing Error Logging
**Issue**: API failures were returning `null` silently without any logging, making debugging impossible.

**Solution**: Added comprehensive logging:
- Added `ILogger<BlizzardService>` and `ILogger<HomeController>`
- Log API requests, responses, errors, and deserialization results
- Log raw JSON responses for debugging

### 2. Achievements Loading Issues

#### Problem 1: 404 Not Found Errors
**Issue**: The Blizzard API was returning 404 errors when fetching achievements, even with valid character names and realm slugs.

**Root Cause**: The Blizzard API requires character names and realm slugs to be **lowercase** in URLs, regardless of how they're stored or displayed.

**Solution**: 
- Convert character names and realm slugs to lowercase before URL encoding
- Added URL encoding for special characters
- Enhanced logging to show original and encoded values

```csharp
var lowerCharacterName = characterName.ToLowerInvariant();
var encodedRealmSlug = Uri.EscapeDataString(realmSlug.ToLowerInvariant());
var encodedCharacterName = Uri.EscapeDataString(lowerCharacterName);
```

#### Problem 2: Achievement Model Structure
**Issue**: The Achievement model didn't match the Blizzard API response structure. Achievements have nested `achievement` objects and `criteria` objects.

**Solution**: Updated the Achievement model:
- `Achievement` class with `Id`, `AchievementInfo`, `Criteria`, and `CompletedTimestamp`
- `AchievementInfo` class with `Key` (LinkObject), `Name`, `Id`, `Description`, and `Points`
- `AchievementCriteria` class for criteria information
- Added `JsonExtensionData` to capture any additional properties

#### Problem 3: Compilation Errors
**Issue**: Code was trying to access `Path`, `LineNumber`, and `LinePosition` properties on `Newtonsoft.Json.JsonException`, which don't exist (those are properties of `System.Text.Json.JsonException`).

**Solution**: Removed references to non-existent properties and simplified error handling to use the exception message and JSON snippets.

## Files Modified

### Services
- **BlizzardService.cs**: 
  - Fixed Authorization header handling
  - Added comprehensive logging
  - Added lowercase conversion for character names and realm slugs
  - Enhanced error handling with detailed messages

### Models
- **BlizzardApiModels.cs**:
  - Updated `Character` model with proper nested structures (`RealmInfo`, `PlayableInfo`, `TypeNamePair`)
  - Updated `Achievement` model to match API structure
  - Added `LinkObject` class for href links
  - Added proper JSON property attributes

### Controllers
- **HomeController.cs**:
  - Added error handling and logging
  - Added debug information for troubleshooting
  - Enhanced exception handling with detailed error messages

### Views
- **Index.cshtml**: 
  - Updated to use `Realm.Slug` instead of `Realm.Key`
  - Added error message display
  - Added debug information display

- **Achievements.cshtml**:
  - Updated to display achievement information from the new model structure
  - Added error message display
  - Fixed date formatting for completed achievements

## Current Application State

### Working Features
✅ **Character Fetching**: Successfully fetches and displays all characters from the user's WoW accounts
✅ **Achievements Loading**: Successfully loads achievements for characters (tested with 4,128 achievements, 25,415 points)
✅ **Error Handling**: Comprehensive error logging and user-friendly error messages
✅ **Authentication**: Battle.net OAuth authentication working correctly

### Displayed Information
- Character name, realm, level, class, race, and faction
- Total achievement points and count
- Achievement completion status and dates
- Achievement names (descriptions and points may need additional API calls)

## Key Learnings

1. **Blizzard API Requirements**:
   - Character names and realm slugs must be lowercase in URLs
   - API uses snake_case for property names (e.g., `wow_accounts`, `playable_race`)
   - Nested objects are common (e.g., `realm.key.href`)

2. **HttpClient Best Practices**:
   - Don't set `DefaultRequestHeaders` on shared HttpClient instances
   - Use `HttpRequestMessage` for per-request headers
   - Always use `AddHttpClient` for proper dependency injection

3. **Error Handling**:
   - Log raw API responses for debugging
   - Provide detailed error messages to help diagnose issues
   - Handle both API errors and deserialization errors

## API Endpoints Used

1. **Profile Summary**: `GET /profile/user/wow?namespace=profile-{region}&locale=en_US`
   - Returns user's WoW accounts and characters

2. **Character Achievements**: `GET /profile/wow/character/{realmSlug}/{characterName}/achievements?namespace=profile-{region}&locale=en_US`
   - Returns character's achievements
   - Requires lowercase character name and realm slug

## Configuration

The application uses the following configuration (from `appsettings.json`):
- `Blizzard:ClientId` - Battle.net OAuth client ID
- `Blizzard:ClientSecret` - Battle.net OAuth client secret
- `BlizzardApi:BaseUrl` - API base URL template: `https://{0}.api.blizzard.com`

## Next Steps (Optional Enhancements)

1. **Achievement Details**: The achievement descriptions and points might require additional API calls to fetch full achievement data
2. **Caching**: Consider implementing caching for character and achievement data to reduce API calls
3. **Pagination**: For characters with many achievements, consider pagination
4. **Filtering/Sorting**: Add UI controls to filter and sort achievements
5. **Achievement Categories**: Display achievements grouped by category

## Testing

The application was tested with:
- Multiple characters across different realms
- Characters with varying achievement counts (from 0 to 4,128+ achievements)
- Different regions (tested with US region)
- Various character levels and classes

All core functionality is working as expected.

