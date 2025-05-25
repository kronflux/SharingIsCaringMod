using HarmonyLib;
using SharingIsCaring.Config;
using SharingIsCaring.Logic;

namespace SharingIsCaring.Patches
{
    /// <summary>
    /// Triggers a forced upgrade sync when the level changes.
    /// This ensures late-joining players receive all upgrades.
    /// </summary>
    [HarmonyPatch(typeof(RunManager), "ChangeLevel")]
    internal static class Patch_RunManager_ChangeLevel
    {
        static void Prefix()
        {
            // Only sync if not in shop, not in arena, not in menu
            if (SemiFunc.MenuLevel() || SemiFunc.RunIsLobbyMenu() || SemiFunc.RunIsShop() || SemiFunc.RunIsArena())
            {
                if (ModConfig.DebugLogging.Value)
                    SharingIsCaringPlugin.Log.LogDebug("Skipped upgrade sync on level change (arena/shop/menu).");
                return;
            }
            UpgradeSync.ForceSync();
        }
    }

    /// <summary>
    /// Resets upgrade tracking after all player stats are reset.
    /// This ensures clean state for the next session.
    /// </summary>
    [HarmonyPatch(typeof(StatsManager), "ResetAllStats")]
    internal static class Patch_StatsManager_ResetAllStats
    {
        static void Postfix()
        {
            UpgradeSync.ResetTracking();
        }
    }
}
