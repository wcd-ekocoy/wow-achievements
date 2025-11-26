# Blizzard API Registration Procedure

To access World of Warcraft achievement data, you must register an application with the Battle.net Developer Portal.

## 1. Create a Developer Account
1. Visit the [Battle.net Developer Portal](https://develop.battle.net/).
2. Click **Sign In** and log in with your existing Battle.net account (or create a new one).

## 2. Create a New Client
1. Navigate to the **My Clients** section (usually in the top header).
2. Click the **+ Create Client** button.
3. Fill in the required details:
   - **Client Name**: A descriptive name for your app (e.g., `WowAchievementsFetcher`).
   - **Redirect URLs**: 
     - For local development/testing with this application, use: `https://localhost:5089/signin-battlenet`
     - **Important**: This application only supports HTTPS. All redirect URLs must use the `https://` protocol.
     - (Note: You need a redirect URL if you plan to use OAuth flows that require user login).
   - **Service URL**: (Optional) Link to your website if you have one.
   - **Intended Use**: Briefly explain you are fetching character achievements.
4. Agree to the developer policy and save.

## 3. Save Your Credentials
Once the client is created, you will see a **Client Settings** page.
1. Locate the **Client ID**.
2. Locate the **Client Secret**.
   - **IMPORTANT**: Your Client Secret is like a password. **Do not** commit this to version control (git). Store it in a `.env` file or environment variables.

## 4. Understanding Tokens
To make API calls, you need an Access Token.

- **Client Credentials Flow**: 
  - Used for "Game Data" (static info like achievement definitions).
  - Requires: `Client ID` + `Client Secret`.
  
- **Authorization Code Flow**:
  - Used for "Profile Data" (user-specific info like *your* completed achievements).
  - Requires: User login via Blizzard to grant permission.

## References
- [Blizzard API Documentation](https://develop.battle.net/documentation/world-of-warcraft)
- [OAuth Guide](https://develop.battle.net/documentation/guides/using-oauth)

