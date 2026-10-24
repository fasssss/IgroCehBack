namespace Domain.Entities
{
    // the database columns and the validation of the preferences share these limits
    public static class PreferenceLimits
    {
        public const int NameMaxLength = 50;
        public const int MaxTagsPerList = 20;
    }
}
