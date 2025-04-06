using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using SharingIsCaring.Config;

namespace SharingIsCaring.Patches
{
    /// <summary>
    /// Renames shop health pack items to reflect adjusted heal amount based on config.
    /// Avoids renaming duplicates using a per-round cache.
    /// </summary>
    [HarmonyPatch(typeof(PunManager), "ShopPopulateItemVolumes")]
    internal static class Patch_ShopHealthItemRename
    {
        // Regex to find numeric values in item names (e.g., "Heal 100")
        private static readonly Regex HealValueRegex = new Regex(@"\d+", RegexOptions.Compiled);

        // Track renamed items to avoid redundant renaming within the same round
        private static readonly HashSet<string> renamedItems = new HashSet<string>();

        /// <summary>
        /// Postfix executed after shop items are populated.
        /// Adjusts item names based on healing multiplier, only if enabled in config.
        /// </summary>
        static void Postfix(PunManager __instance)
        {
            if (!ModConfig.EnableHealthSync.Value || !ModConfig.RenameHealthItems.Value)
                return;

            var shopManagerField = Traverse.Create(__instance).Field("shopManager");
            var shopManager = shopManagerField.GetValue<ShopManager>();

            if (shopManager == null || shopManager.potentialItemHealthPacks == null)
                return;

            List<Item> healthItems = new HashSet<Item>(shopManager.potentialItemHealthPacks)
                .Distinct()
                .ToList();

            foreach (Item item in healthItems)
            {
                if (renamedItems.Contains(item.itemName))
                    continue;

                string originalName = item.itemName;

                string updatedName = HealValueRegex.Replace(originalName, match =>
                {
                    if (int.TryParse(match.Value, out int originalHeal))
                    {
                        int adjusted = (int)Math.Ceiling(originalHeal * ModConfig.HealMultiplier.Value);
                        return adjusted.ToString();
                    }

                    return match.Value;
                });

                if (updatedName != originalName)
                {
                    item.itemName = updatedName;
                    renamedItems.Add(updatedName);

                    if (ModConfig.DebugLogging.Value)
                    {
                        SharingIsCaringPlugin.Log.LogDebug($"Renamed item '{originalName}' → '{updatedName}'");
                    }
                }
            }

            if (ModConfig.DebugLogging.Value && healthItems.Count > 0)
            {
                SharingIsCaringPlugin.Log.LogDebug($"Renamed {renamedItems.Count} health items in shop this round.");
            }
        }

        /// <summary>
        /// Clears the renamed item cache. Should be called at the start of each new round.
        /// </summary>
        public static void ClearRenameCache()
        {
            renamedItems.Clear();

            if (ModConfig.DebugLogging.Value)
            {
                SharingIsCaringPlugin.Log.LogDebug("Cleared health item rename cache for new level.");
            }
        }
    }

    /// <summary>
    /// Clears renamed item cache when the game level changes to prevent stale renaming.
    /// </summary>
    [HarmonyPatch(typeof(RunManager), "ChangeLevel")]
    internal static class Patch_ClearItemRenameCache
    {
        static void Prefix()
        {
            Patch_ShopHealthItemRename.ClearRenameCache();
        }
    }
}
