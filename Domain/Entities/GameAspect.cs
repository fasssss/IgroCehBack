namespace Domain.Entities
{
    // what a player likes in games ("Что нравится в играх" of the profile): the story, the gameplay, the achievements…;
    // the catalog comes with the database in both languages of the site
    public class GameAspect
    {
        public int Id { get; set; }
        // English
        public string Name { get; set; }
        public string NameRu { get; set; }
    }
}
