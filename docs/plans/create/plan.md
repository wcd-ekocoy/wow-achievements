# WoW Achievements C# MVC App

## Overview
Build an ASP.NET Core MVC application to authenticate via Battle.net, select a region, and display user's WoW achievements.

## Architecture
- **Framework**: ASP.NET Core MVC
- **Authentication**: OAuth 2.0 (Battle.net) using `AspNet.Security.OAuth.BattleNet` or generic OAuth.
- **Client**: Raw `HttpClient` (using `System.Net.Http`).
- **Data Flow`:
    1. User lands on home page.
    2. User selects Region (US, EU, etc.) -> stores in Session/Cookie.
    3. User clicks "Login with Battle.net".
    4. App redirects to Blizzard OAuth (using selected region for endpoints if needed, though OAuth is global, APIs are regional).
    5. Callback receives `access_token`.
    6. App fetches User Profile (Account Profile API) to list Characters.
    7. User selects a Character.
    8. App fetches Character Achievements (Character Achievements API).
    9. App displays achievements.

## Steps

### 1. Project Setup
- [x] Create new ASP.NET Core MVC project (`dotnet new mvc`).
- [x] Add `.gitignore`.
- [x] Install NuGet packages:
    - [x] `AspNet.Security.OAuth.BattleNet`
    - [x] `Microsoft.AspNetCore.Authentication.Cookies`
- [x] Setup `appsettings.json` for Client ID/Secret.
- [x] **Stage Check**: Mark all items in "Project Setup" as checked and rewrite `plan.md`.

### 2. Authentication & Region Selection
- [x] Create a Region Selection UI on the Login page.
- [x] Configure Authentication in `Program.cs`.
- [x] Implement `AccountController` for Login/Logout.
- [x] Handle OAuth Callback.
- [x] **Stage Check**: Mark all items in "Authentication & Region Selection" as checked and rewrite `plan.md`.

### 3. Testing Setup
- [x] Create a new xUnit test project (`dotnet new xunit`).
- [x] Install NuGet packages:
    - [x] `xunit`
    - [x] `xunit.runner.visualstudio`
    - [x] `Moq`
    - [x] `Microsoft.NET.Test.Sdk`
    - [x] `Microsoft.AspNetCore.Mvc.Testing`
- [x] Reference the main application project in the test project.
- [x] **Stage Check**: Mark all items in "Testing Setup" as checked and rewrite `plan.md`.

### 4. Blizzard API Integration
- [x] Create a `BlizzardService` to handle API calls.
- [x] Implement `GetProfileSummary` (to list characters).
- [x] Implement `GetCharacterAchievements`.
- [x] Create unit tests for `BlizzardService` using xUnit and Moq.
- [x] Ensure all `BlizzardService` tests pass.
- [x] **Stage Check**: Mark all items in "Blizzard API Integration" as checked and rewrite `plan.md`.

### 5. AccountController Unit Tests
- [x] Create unit tests for `AccountController` using xUnit and Moq, focusing on login/logout flows and region handling.
- [x] Ensure all `AccountController` tests pass.
- [x] **Stage Check**: Mark all items in "AccountController Unit Tests" as checked and rewrite `plan.md`.

### 6. UI Implementation
- [x] **Home/Login**: Region dropdown + "Login" button.
- [x] **Character List**: Display list of WoW characters.
- [x] **Achievements**: Display achievements for selected character.
- [x] **Stage Check**: Mark all items in "UI Implementation" as checked and rewrite `plan.md`.

### 7. Refinement
- [ ] Error handling (token expiration, invalid region).
- [ ] Basic styling (Bootstrap is default in MVC).
- [ ] **Stage Check**: Mark all items in "Refinement" as checked and rewrite `plan.md`.

