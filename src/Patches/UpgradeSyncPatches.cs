using HarmonyLib;
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
