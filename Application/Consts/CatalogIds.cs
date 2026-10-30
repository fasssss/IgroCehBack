namespace Application.Consts
{
    // VideoGamesCritic gives a game in Steam its Steam app id and a game outside Steam an id of its own from FirstOutsideSteam on,
    // e.g. 367520 for Hollow Knight and 1000012522 for Ninja Gaiden 3, which is only on consoles
    public static class CatalogIds
    {
        public const int FirstOutsideSteam = 1_000_000_000;

        public static bool IsSteamAppId(int vgcId) => vgcId > 0 && vgcId < FirstOutsideSteam;
    }
}
