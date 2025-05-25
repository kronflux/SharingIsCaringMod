using BepInEx.Configuration;

namespace SharingIsCaring.Config
{
    /// <summary>
    /// Holds all configurable mod settings for Sharing is Caring.
    /// </summary>
    public static class ModConfig
    {
        // ────────────────────────────────
        // General Settings
        // ────────────────────────────────

        public static ConfigEntry<bool> EnableUpgradeSync;
        public static ConfigEntry<bool> EnableHealthSync;
        public static ConfigEntry<bool> DebugLogging;

        // ────────────────────────────────
        // Upgrade Sync Filters
        // ────────────────────────────────

        public static ConfigEntry<bool> SyncHealthUpgrades;
        public static ConfigEntry<bool> SyncMobilityUpgrades;
        public static ConfigEntry<bool> SyncGrabUpgrades;
        public static ConfigEntry<bool> SyncModdedUpgrades;

        // ────────────────────────────────
        // Upgrade Sync Timing
        // ────────────────────────────────

        public static ConfigEntry<float> UpgradeSyncInterval;

        private const float MinSyncInterval = 1f;
        private const float MaxSyncInterval = 60f;

        /// <summary>
        /// Initializes all configuration entries and assigns default values.
        /// Should be called once from the plugin's Awake().
        /// </summary>
        public static void Init(ConfigFile config)
        {
            // General
            EnableUpgradeSync = config.Bind("General", "EnableUpgradeSync", true, "Enable syncing of upgrades across all players.");
            EnableHealthSync = config.Bind("General", "EnableHealthSync", true, "Enable team healing when using health packs.");
            DebugLogging = config.Bind("General", "DebugLogging", false, "If true, enables detailed logging for troubleshooting.");

            // Upgrade Sync Categories
            SyncHealthUpgrades = config.Bind("Upgrade Sync", "SyncHealth", true, "Sync health-related upgrades.");
            SyncMobilityUpgrades = config.Bind("Upgrade Sync", "SyncMobility", true, "Sync movement-related upgrades.");
            SyncGrabUpgrades = config.Bind("Upgrade Sync", "SyncGrab", true, "Sync grabbing-related upgrades.");
            SyncModdedUpgrades = config.Bind("Upgrade Sync", "SyncModded", true, "Sync unrecognized or modded upgrades.");

            // Upgrade Sync Timing
            UpgradeSyncInterval = config.Bind("Upgrade Sync", "SyncInterval", 5f, $"Time in seconds between automatic upgrade sync checks. Allowed range: {MinSyncInterval}-{MaxSyncInterval} seconds.");

            UpgradeSyncInterval.SettingChanged += (_, __) =>
            {
                ClampSyncInterval("runtime");
            };

            ClampSyncInterval("startup");
        }

        /// <summary>
        /// Ensures UpgradeSyncInterval stays within allowed bounds.
        /// </summary>
        private static void ClampSyncInterval(string context)
        {
            if (UpgradeSyncInterval.Value < MinSyncInterval)
            {
                SharingIsCaringPlugin.Log.LogWarning($"SyncInterval was below {MinSyncInterval}s ({UpgradeSyncInterval.Value}) during {context} — clamping to {MinSyncInterval}s.");
                UpgradeSyncInterval.Value = MinSyncInterval;
            }
            else if (UpgradeSyncInterval.Value > MaxSyncInterval)
            {
                SharingIsCaringPlugin.Log.LogWarning($"SyncInterval exceeded {MaxSyncInterval}s ({UpgradeSyncInterval.Value}) during {context} — clamping to {MaxSyncInterval}s.");
                UpgradeSyncInterval.Value = MaxSyncInterval;
            }
        }
    }
}
