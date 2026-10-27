namespace Application.Enum
{
    public enum LinkSteamResult
    {
        Linked,
        // Steam doesn't vouch for the answer or it was meant for another sign-in
        InvalidAnswer,
        // the Steam account is linked to another user
        TakenByAnotherUser,
    }
}
