namespace Domain.Entities
{
    // a genre, a release period or a scale of games that a player prefers or avoids;
    // the catalog comes with the database in both languages of the site
    public class PreferenceTag
    {
        public int Id { get; set; }
        // English
        public string Name { get; set; }
        public string NameRu { get; set; }
    }
}
