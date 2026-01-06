using System.Collections.Generic;

namespace WowAchievementsApp.Models
{
    public class AchievementsViewModel
    {
        public CharacterAchievements CharacterAchievements { get; set; }
        public List<Achievement> PaginatedAchievements { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public string RealmSlug { get; set; }
        public string CharacterName { get; set; }
        public string Region { get; set; }
    }
}
