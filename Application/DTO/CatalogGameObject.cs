namespace Application.DTO
{
    // game found in external catalog (VideoGamesCritic), not saved to the database
    public class CatalogGameObject
    {
        public int VgcId { get; set; }
        public string Name { get; set; }
        public string ReleaseDate { get; set; }
        public string CoverUrl { get; set; }
        public string PageUrl { get; set; }
        public string SteamUrl { get; set; }
        public List<string> Platforms { get; set; } = new List<string>();
        public int? CriticsScore { get; set; }
        public int? PlayersScore { get; set; }
    }
}
