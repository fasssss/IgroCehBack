namespace Application.Enum
{
    public enum LinkXboxResult
    {
        Linked,
        // OpenXBL doesn't accept the code of the sign-in
        InvalidCode,
        // the Xbox account is linked to another user
        TakenByAnotherUser,
    }
}
