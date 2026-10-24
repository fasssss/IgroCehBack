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
    }
}
