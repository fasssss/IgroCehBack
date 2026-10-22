namespace Domain.Enums
{
    // who sees a review besides its author, every next value shows it to more players
    public enum ReviewVisibility
    {
        OnlyAuthor = 0,
        AuthorAndGuesser = 1,
        Guild = 2,
        Everyone = 3,
    }
}
