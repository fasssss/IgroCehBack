namespace Application.DTO
{
    // an account in PSN by its online id; the account id stays when the player changes the online id
    public class PsnProfileObject
    {
        public string AccountId { get; set; }
        public string OnlineId { get; set; }
        // what the player wrote in "About me", empty when nothing
        public string AboutMe { get; set; }
    }
}
