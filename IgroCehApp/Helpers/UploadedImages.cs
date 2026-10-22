namespace API.Helpers
{
    // uploaded images are stored under generated names, the extension comes from the checked content type
    public static class UploadedImages
    {
        public const long MaxSize = 10 * 1024 * 1024;

        public static readonly IReadOnlyDictionary<string, string> Extensions = new Dictionary<string, string>
        {
            ["image/png"] = ".png",
            ["image/jpeg"] = ".jpg",
            ["image/webp"] = ".webp",
            ["image/gif"] = ".gif",
        };
    }
}
