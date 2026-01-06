# WoW Achievements App

A .NET MVC application that integrates with the Battle.net API to fetch and display World of Warcraft character profiles and achievements.

## Features

*   **Battle.net Authentication**: Secure login using OAuth 2.0.
*   **Character Listing**: View all characters from your WoW account across different realms.
*   **Achievement Browser**: Detailed view of achievements for any selected character, including completion status and points.
*   **Modern Interface**: Clean and responsive UI using ASP.NET Core MVC.

## Technology Stack

*   **Framework**: ASP.NET Core MVC (.NET 7+)
*   **Authentication**: AspNet.Security.OAuth.BattleNet
*   **HTTP Client**: Typed HttpClient for Blizzard API interaction
*   **JSON Processing**: Newtonsoft.Json & System.Text.Json

## Getting Started

### Prerequisites

*   [.NET SDK](https://dotnet.microsoft.com/download)
*   A [Battle.net Developer Account](https://develop.battle.net/) to get Client ID and Client Secret.

### Configuration

1.  Clone the repository.
2.  Update `appsettings.json` or use [User Secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets) (Recommended) with your Battle.net credentials:

```json
"Blizzard": {
  "ClientId": "YOUR_CLIENT_ID",
  "ClientSecret": "YOUR_CLIENT_SECRET"
}
```

### Running the Application

1.  Navigate to the project directory:
    ```bash
    cd WowAchievementsApp
    ```
2.  Restore dependencies:
    ```bash
    dotnet restore
    ```
3.  Run the application:
    ```bash
    dotnet run
    ```

## Development Notes

*   **API Rate Limits**: Be aware of Blizzard API rate limits during development.
*   **Region Support**: The app currently defaults to `en_US` locale and associated regions.

## Project Structure

*   `/Controllers`: Handles page navigation and user actions.
*   `/Services`: Contains `BlizzardService` for API communication.
*   `/Models`: Data models matching Blizzard API responses.
*   `/Views`: Razor views for the UI.
