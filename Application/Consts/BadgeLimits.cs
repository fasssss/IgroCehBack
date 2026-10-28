namespace Application.Consts
{
    // the database columns and the validation of a badge share these limits
    public static class BadgeLimits
    {
        public const int IdMaxLength = 100;
        public const int NameMaxLength = 50;
        public const int ContentTypeMaxLength = 50;
        // a badge is a small picture, it goes with every profile that shows it
        public const int ImageMaxSize = 256 * 1024;
        public const string Svg = "image/svg+xml";
        public const string Png = "image/png";
        public const string Webp = "image/webp";
        public static readonly IReadOnlyList<string> ContentTypes = new[] { Svg, Png, Webp };
    }
}
