using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SharingIsCaring.Config;
using SharingIsCaring.Logic;

namespace SharingIsCaring
{
    /// <summary>
    /// Main plugin entry point for the Sharing is Caring mod.
    /// Initializes configuration, Harmony patches, and sync logic.
    /// </summary>
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class SharingIsCaringPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "FluxTeam.SharingIsCaring";
        public const string PluginName = "Sharing is Caring";
        public const string PluginVersion = "1.1.0";

        /// <summary>Global log instance for all mod systems to use.</summary>
        internal static ManualLogSource Log;

        /// <summary>
        /// Plugin initialization. Runs once on load.
        /// </summary>
        private void Awake()
        {
            Log = Logger;
            Logger.LogInfo($"{PluginName} v{PluginVersion} loading...");

            // Load and bind configuration values
            ModConfig.Init(Config);

            // Show configured sync interval
            Logger.LogInfo($"Upgrade sync interval: {ModConfig.UpgradeSyncInterval.Value} second(s)");

            // React to configuration file changes during runtime
            Config.ConfigReloaded += (_, __) =>
            {
                Log.LogInfo("Configuration reloaded.");
                UpgradeSync.ResetTracking(); // Force resync on next check
            };

            // Start upgrade synchronization runner (host only)
            UpgradeSync.Initialize();

            // Patch all Harmony-decorated methods in the mod
            var harmony = new Harmony(PluginGUID);
            harmony.PatchAll();

            Logger.LogInfo($"{PluginName} loaded successfully.");
        }
    }
}
