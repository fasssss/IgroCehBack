namespace Application.DTO
{
    // the front shows the name in the language of the site and searches both
    public class PreferenceTagObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameRu { get; set; }
    }

    // the tags in the order the player added them
    public class PlayerPreferencesObject
    {
        public List<PreferenceTagObject> Preferred { get; set; }
        public List<PreferenceTagObject> Avoided { get; set; }
        // "Что нравится в играх", in the order the player added them
        public List<GameAspectObject> LikedAspects { get; set; }
    }

    // an aspect of games of the catalog, the name is shown in the language of the site
    public class GameAspectObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameRu { get; set; }
    }
}
