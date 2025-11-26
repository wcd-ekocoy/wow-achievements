using WowAchievementsApp.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Web;

namespace WowAchievementsApp.Services
{
    public class BlizzardService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BlizzardService> _logger;

        public BlizzardService(HttpClient httpClient, IConfiguration configuration, ILogger<BlizzardService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<SelfProfile?> GetProfileSummary(string accessToken, string region)
        {
            var rawBaseUrl = _configuration["BlizzardApi:BaseUrl"];
            if (string.IsNullOrEmpty(rawBaseUrl))
            {
                throw new InvalidOperationException("BlizzardApi:BaseUrl is not configured.");
            }
            var baseUrl = string.Format(rawBaseUrl, region);
            var url = $"{baseUrl}/profile/user/wow?namespace=profile-{region}&locale=en_US";
            
            // Create a new request message to set Authorization per-request
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            _logger.LogInformation("Fetching profile from: {Url}", url);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Profile API response received. Length: {Length}", jsonString.Length);
                _logger.LogDebug("Profile API response: {Response}", jsonString);
                
                try
                {
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };
                    
                    var profile = JsonConvert.DeserializeObject<SelfProfile>(jsonString, settings);
                    
                    if (profile != null)
                    {
                        _logger.LogInformation("Profile deserialized. WowAccounts count: {Count}", profile.WowAccounts?.Count ?? 0);
                        if (profile.WowAccounts != null)
                        {
                            foreach (var account in profile.WowAccounts)
                            {
                                _logger.LogInformation("Account {AccountId} has {CharacterCount} characters", account.Id, account.Characters?.Count ?? 0);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Profile deserialized to null");
                    }
                    
                    return profile;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize profile. JSON: {Json}", jsonString);
                    throw;
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Profile API call failed. Status: {Status}, Response: {Response}", response.StatusCode, errorContent);
            }

            return null;
        }

        public async Task<CharacterAchievements?> GetCharacterAchievements(
            string accessToken, string region, string realmSlug, string characterName)
        {
            var rawBaseUrl = _configuration["BlizzardApi:BaseUrl"];
            if (string.IsNullOrEmpty(rawBaseUrl))
            {
                throw new InvalidOperationException("BlizzardApi:BaseUrl is not configured.");
            }
            var baseUrl = string.Format(rawBaseUrl, region);
            // Blizzard API requires character names to be lowercase
            var lowerCharacterName = characterName.ToLowerInvariant();
            // URL encode the realm slug and character name to handle special characters
            var encodedRealmSlug = Uri.EscapeDataString(realmSlug.ToLowerInvariant());
            var encodedCharacterName = Uri.EscapeDataString(lowerCharacterName);
            var url = $"{baseUrl}/profile/wow/character/{encodedRealmSlug}/{encodedCharacterName}/achievements?namespace=profile-{region}&locale=en_US";
            
            _logger.LogInformation("Achievements URL construction - Original: realm={Realm}, character={Character}", realmSlug, characterName);
            _logger.LogInformation("Achievements URL construction - Encoded: realm={EncodedRealm}, character={EncodedCharacter}", encodedRealmSlug, encodedCharacterName);
            _logger.LogInformation("Fetching achievements from: {Url}", url);
            
            // Create a new request message to set Authorization per-request
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Achievements API response received. Length: {Length}", jsonString.Length);
                _logger.LogDebug("Achievements API response: {Response}", jsonString);
                
                try
                {
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };
                    
                    var achievements = JsonConvert.DeserializeObject<CharacterAchievements>(jsonString, settings);
                    
                    if (achievements == null)
                    {
                        _logger.LogError("Achievements deserialized to null. JSON length: {Length}, First 500 chars: {Snippet}", 
                            jsonString.Length, jsonString.Length > 500 ? jsonString.Substring(0, 500) : jsonString);
                        throw new InvalidOperationException($"Failed to deserialize achievements: JSON deserialized to null. Response length: {jsonString.Length} characters.");
                    }
                    
                    _logger.LogInformation("Achievements deserialized. Total achievements: {Count}, Total points: {Points}", 
                        achievements.Achievements?.Count ?? 0, achievements.TotalPoints);
                    
                    return achievements;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize achievements. Message: {Message}. JSON (first 2000 chars): {Json}", 
                        ex.Message, jsonString.Length > 2000 ? jsonString.Substring(0, 2000) : jsonString);
                    
                    // Try to extract a meaningful snippet from the JSON
                    var jsonSnippet = jsonString.Length > 500 ? jsonString.Substring(0, 500) + "..." : jsonString;
                    
                    throw new InvalidOperationException($"Failed to deserialize achievements: {ex.Message}. JSON snippet: {jsonSnippet}", ex);
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Achievements API call failed. Status: {Status}, Response: {Response}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Blizzard API returned status {response.StatusCode}: {errorContent}");
            }
        }
    }
}
