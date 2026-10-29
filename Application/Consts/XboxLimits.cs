namespace Application.Consts
{
    // the database columns and the validation of a linked Xbox account share these limits
    public static class XboxLimits
    {
        // a XUID is a 64-bit number, e.g. 2535445689199050
        public const int XuidMaxLength = 20;
        // classic gamertags are up to 15 characters, modern ones carry a #suffix
        public const int GamertagMaxLength = 50;
        // the code of Microsoft the sign-in brings back is several hundred characters long
        public const int CodeMaxLength = 4000;
        // an unsigned 32-bit number, e.g. 1993022209
        public const int TitleIdMaxLength = 10;
        public const int TitleNameMaxLength = 255;
        public const int SearchQueryMaxLength = 100;
    }
}
