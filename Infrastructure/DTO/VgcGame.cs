namespace Infrastructure.DTO
{
    // game object of VideoGamesCritic API, shared by search results and game details (details contain more fields)
    public class VgcGame
    {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string ReleaseDate { get; set; }
        public VgcScores Scores { get; set; }
        public List<string> Platforms { get; set; }
        public VgcArt Art { get; set; }
        public VgcLinks Links { get; set; }
    }

    public class VgcScores
    {
        public double? Vgc { get; set; }
        public double? Players { get; set; }
        public double? Critics { get; set; }
    }

    public class VgcArt
    {
        public string Cover { get; set; }
        public string Header { get; set; }
    }

    public class VgcLinks
    {
        public string Page { get; set; }
        public string Steam { get; set; }
    }

    public class VgcSearchResult
    {
        public string Query { get; set; }
        public int Total { get; set; }
        public List<VgcGame> Games { get; set; }
    }
}
