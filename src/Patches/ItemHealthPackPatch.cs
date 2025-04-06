using HarmonyLib;
using Photon.Pun;
using SharingIsCaring.Logic;

namespace SharingIsCaring.Patches
{
    /// <summary>
    /// Harmony patch for ItemHealthPack.UsedRPC.
    /// This is triggered when a player uses a health pack.
    /// </summary>
    [HarmonyPatch(typeof(ItemHealthPack), "UsedRPC")]
    internal static class Patch_ItemHealthPack_UsedRPC
    {
        /// <summary>
        /// Postfix: Triggers team healing logic after a health pack is used.
        /// </summary>
        /// <param name="__instance">The health pack instance that was used.</param>
        static void Postfix(ItemHealthPack __instance)
        {
            HealthSync.OnHealthPackUsed(__instance);
        }
    }
}
