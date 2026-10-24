namespace Domain.Entities
{
    // a tag in the "prefer" or the "avoid" list of a player, a tag is in one of the lists at most
    public class UserPreference
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public int PreferenceTagId { get; set; }
        public PreferenceTag PreferenceTag { get; set; }
        public bool IsAvoided { get; set; }
        // the tags keep the order the player added them in
        public int Order { get; set; }
    }
}
