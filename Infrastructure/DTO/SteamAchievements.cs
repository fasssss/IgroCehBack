using System.Text.Json.Serialization;

namespace Infrastructure.DTO
{
    // GetSchemaForGame: a game without achievements comes without availableGameStats
    public class SteamSchemaResponse
    {
        public SteamSchemaGame Game { get; set; }
    }

    public class SteamSchemaGame
    {
        public string GameName { get; set; }
        public SteamGameStats AvailableGameStats { get; set; }
    }

    public class SteamGameStats
    {
        public List<SteamSchemaAchievement> Achievements { get; set; }
    }

    public class SteamSchemaAchievement
    {
        // the API name, the same in every language
        public string Name { get; set; }
        public string DisplayName { get; set; }
        // hidden achievements may come without it
        public string Description { get; set; }
        public string Icon { get; set; }
        public int Hidden { get; set; }
    }

    // GetGlobalAchievementPercentagesForApp, the percent sometimes comes as a string
    public class SteamPercentagesResponse
    {
        public SteamPercentages AchievementPercentages { get; set; }
    }

    public class SteamPercentages
    {
        public List<SteamAchievementPercent> Achievements { get; set; }
    }

    public class SteamAchievementPercent
    {
        public string Name { get; set; }
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public double Percent { get; set; }
    }
}
