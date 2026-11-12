namespace Application.DTO
{
    // a guild of the player as the query of the profile reads it, before the guilds are chosen for the viewer
    public class PlayerGuildRow
    {
        public PlayerGuildObject Guild { get; set; }
        // a newcomer is only the one who got a game in an event of the guild
        public bool HasPlayed { get; set; }
        // the viewer is a member of the guild too
        public bool IsShared { get; set; }
    }
}
