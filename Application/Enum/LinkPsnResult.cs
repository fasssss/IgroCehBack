namespace Application.Enum
{
    public enum LinkPsnResult
    {
        Linked,
        // PSN has no such online id
        NotFound,
        // nothing is started: the code is too old, was already used or the site restarted
        NotStarted,
        // About me doesn't show the code
        CodeNotFound,
        TooManyAttempts,
    }
}
