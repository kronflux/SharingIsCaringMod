using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using SharingIsCaring.Config;

namespace SharingIsCaring.Logic
{
    /// <summary>
    /// Synchronizes player upgrades across the team.
    /// Host checks upgrades periodically and applies the highest value to all players.
    /// </summary>
    public static class UpgradeSync
    {
        private static Dictionary<string, int> previousMaxValues = new Dictionary<string, int>();
        private static float lastSyncTime = 0f;

        /// <summary>
        /// Initializes the upgrade sync logic by creating an update runner.
        /// </summary>
        public static void Initialize()
        {
            previousMaxValues.Clear();
            lastSyncTime = 0f;

            new GameObject("UpgradeSyncRunner").AddComponent<UpgradeSyncRunner>();
        }

        /// <summary>
        /// Forces an upgrade sync regardless of whether values have changed.
        /// </summary>
        public static void ForceSync()
        {
            SyncUpgrades(forced: true);
        }

        /// <summary>
        /// Resets cached values so next sync will re-evaluate all upgrades.
        /// Useful when stats are reset or config is reloaded.
        /// </summary>
        public static void ResetTracking()
        {
            previousMaxValues.Clear();
        }

        /// <summary>
        /// Main logic to evaluate and synchronize upgrades.
        /// Only runs on host.
        /// </summary>
        /// <param name="forced">If true, syncs regardless of changes.</param>
        private static void SyncUpgrades(bool forced = false)
        {
            if (!PhotonNetwork.IsMasterClient || !ModConfig.EnableUpgradeSync.Value)
                return;

            if (!LevelGenerator.Instance.Generated || SemiFunc.MenuLevel())
                return;

            // Skip syncing in the Shop or Arena
            if (SemiFunc.RunIsShop() || SemiFunc.RunIsArena())
            {
                if (ModConfig.DebugLogging.Value)
                    SharingIsCaringPlugin.Log.LogDebug("Skipping upgrade sync: currently in Shop or Arena.");
                return;
            }

            var stats = StatsManager.instance;
            var allStats = stats.dictionaryOfDictionaries;

            // Filter upgrade-related keys by config
            List<string> upgradeKeys = allStats.Keys
                .Where(key => key.StartsWith("playerUpgrade") && ShouldSyncKey(key))
                .ToList();

            Dictionary<string, int> maxValues = new Dictionary<string, int>();

            foreach (string key in upgradeKeys)
            {
                if (allStats.TryGetValue(key, out var valueDict) && valueDict.Count > 0)
                {
                    maxValues[key] = valueDict.Values.Max();
                }
            }

            bool hasChanged = forced || maxValues.Any(kv =>
                !previousMaxValues.TryGetValue(kv.Key, out int prev) || prev != kv.Value);

            if (!hasChanged)
                return;

            if (ModConfig.DebugLogging.Value)
                SharingIsCaringPlugin.Log.LogDebug("Upgrades changed or forced — syncing...");

            foreach (var kv in maxValues)
            {
                stats.DictionaryFill(kv.Key, kv.Value);

                if (ModConfig.DebugLogging.Value)
                    SharingIsCaringPlugin.Log.LogDebug($"→ Synced {kv.Key} = {kv.Value}");
            }

            previousMaxValues = new Dictionary<string, int>(maxValues);

            SemiFunc.StatSyncAll();
            SharingIsCaringPlugin.Log.LogInfo("Upgrades synced for all players.");
        }

        /// <summary>
        /// Determines whether a given upgrade key should be synced, based on user config.
        /// </summary>
        private static bool ShouldSyncKey(string key)
        {
            string cleanKey = key.Replace("playerUpgrade", "").ToLowerInvariant();

            if (cleanKey.Contains("health") || cleanKey.Contains("stamina"))
                return ModConfig.SyncHealthUpgrades.Value;

            if (cleanKey.Contains("jump") || cleanKey.Contains("launch") || cleanKey.Contains("speed"))
                return ModConfig.SyncMobilityUpgrades.Value;

            if (cleanKey.Contains("strength") || cleanKey.Contains("range") || cleanKey.Contains("throw"))
                return ModConfig.SyncGrabUpgrades.Value;

            return ModConfig.SyncModdedUpgrades.Value;
        }

        /// <summary>
        /// Runner MonoBehaviour to handle sync checks each frame.
        /// Only active on the host.
        /// </summary>
        private class UpgradeSyncRunner : MonoBehaviour
        {
            void Update()
            {
                if (!PhotonNetwork.IsMasterClient || !ModConfig.EnableUpgradeSync.Value)
                    return;

                float interval = Mathf.Clamp(ModConfig.UpgradeSyncInterval.Value, 1f, 60f);

                if (Time.time - lastSyncTime > interval)
                {
                    SyncUpgrades();
                    lastSyncTime = Time.time;
                }
            }
        }
    }
}
