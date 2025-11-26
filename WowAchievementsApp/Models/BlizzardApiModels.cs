using Newtonsoft.Json;

namespace WowAchievementsApp.Models
{
    public class SelfProfile
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("accountId")]
        public int AccountId { get; set; }
        
        // Blizzard API uses "wow_accounts" (snake_case)
        [JsonProperty("wow_accounts")]
        public List<WowAccount> WowAccounts { get; set; } = new List<WowAccount>();
    }

    public class WowAccount
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("characters")]
        public List<Character> Characters { get; set; } = new List<Character>();
    }

    public class Character
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("realm")]
        public RealmInfo Realm { get; set; } = new RealmInfo();
        
        [JsonProperty("playable_race")]
        public PlayableInfo PlayableRace { get; set; } = new PlayableInfo();
        
        [JsonProperty("playable_class")]
        public PlayableInfo PlayableClass { get; set; } = new PlayableInfo();
        
        [JsonProperty("gender")]
        public TypeNamePair Gender { get; set; } = new TypeNamePair();
        
        [JsonProperty("faction")]
        public TypeNamePair Faction { get; set; } = new TypeNamePair();
        
        [JsonProperty("level")]
        public int Level { get; set; }
    }

    public class RealmInfo
    {
        [JsonProperty("key")]
        public LinkObject Key { get; set; } = new LinkObject();
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("slug")]
        public string Slug { get; set; } = string.Empty;
    }

    public class PlayableInfo
    {
        [JsonProperty("key")]
        public LinkObject Key { get; set; } = new LinkObject();
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("id")]
        public int Id { get; set; }
    }

    public class TypeNamePair
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class LinkObject
    {
        [JsonProperty("href")]
        public string Href { get; set; } = string.Empty;
    }

    public class KeyValuePair
    {
        [JsonProperty("key")]
        public string Key { get; set; } = string.Empty;
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("id")]
        public int Id { get; set; }
    }

    public class CharacterAchievements
    {
        [JsonProperty("_links")]
        public Dictionary<string, LinkObject>? Links { get; set; }
        
        [JsonProperty("character")]
        public LinkObject? Character { get; set; }
        
        [JsonProperty("total_quantity")]
        public int? TotalQuantity { get; set; }
        
        [JsonProperty("total_points")]
        public int TotalPoints { get; set; }
        
        [JsonProperty("achievements")]
        public List<Achievement> Achievements { get; set; } = new List<Achievement>();
    }

    public class Achievement
    {
        [JsonProperty("id")]
        public int? Id { get; set; }
        
        [JsonProperty("achievement")]
        public AchievementInfo? AchievementInfo { get; set; }
        
        [JsonProperty("criteria")]
        public AchievementCriteria? Criteria { get; set; }
        
        [JsonProperty("completed_timestamp")]
        public long? CompletedTimestamp { get; set; }
        
        // Allow additional properties to be stored
        [JsonExtensionData]
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    public class AchievementInfo
    {
        [JsonProperty("key")]
        public LinkObject? Key { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("description")]
        public string? Description { get; set; }
        
        [JsonProperty("points")]
        public int? Points { get; set; }
    }

    public class AchievementCriteria
    {
        [JsonProperty("id")]
        public int? Id { get; set; }
        
        [JsonProperty("is_completed")]
        public bool? IsCompleted { get; set; }
        
        [JsonProperty("child_criteria")]
        public List<AchievementCriteria>? ChildCriteria { get; set; }
    }
}
