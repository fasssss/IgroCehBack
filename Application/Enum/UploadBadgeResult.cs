namespace Application.Enum
{
    public enum UploadBadgeResult
    {
        Uploaded,
        // the file isn't the image its type says, or an SVG has scripts or links outside of itself
        InvalidImage,
    }
}
