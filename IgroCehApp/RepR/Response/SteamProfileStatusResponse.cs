namespace API.RepR.Response
{
    public class SteamProfileStatusResponse
    {
        // Steam gives the achievements of the player only with public game details; null when Steam didn't answer
        public bool? IsGameDataPublic { get; set; }
    }
}
