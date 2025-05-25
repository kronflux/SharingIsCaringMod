using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using SharingIsCaring.Config;
using HarmonyLib;

namespace SharingIsCaring.Logic
{
    /// <summary>
    /// Central logic for team health sharing.
    /// Called from Harmony patch after a health pack is used.
    /// </summary>
    public static class HealthSync
    {
        /// <summary>
        /// Applies health pack healing to all players except the user who triggered the item.
        /// Only the host performs this logic.
        /// </summary>
        /// <param name="pack">The health pack instance that was used.</param>
        public static void OnHealthPackUsed(ItemHealthPack pack)
        {
            if (!PhotonNetwork.IsMasterClient || !ModConfig.EnableHealthSync.Value)
                return;

            // Use reflection to identify which player used the health pack.
            var itemToggleField = AccessTools.Field(typeof(ItemHealthPack), "itemToggle");
            var playerTogglePhotonIDField = AccessTools.Field(typeof(ItemToggle), "playerTogglePhotonID");
            var itemToggle = itemToggleField.GetValue(pack);
            int userViewId = (int)playerTogglePhotonIDField.GetValue(itemToggle);

            int healedCount = 0;

            foreach (var avatar in SemiFunc.PlayerGetAll())
            {
                if (avatar == null)
                    continue;

                var photonView = avatar.photonView;
                if (photonView == null)
                    continue;

                // Heal all players except the one who used the health pack.
                if (photonView.ViewID != userViewId)
                {
                    avatar.playerHealth.HealOther(pack.healAmount, true);
                    healedCount++;
                }
            }

            SharingIsCaringPlugin.Log.LogInfo(
                $"Shared health pack used (host-side) → healed {healedCount} player(s) for {pack.healAmount} HP.");

            if (ModConfig.DebugLogging.Value)
            {
                string names = string.Join(", ",
                    SemiFunc.PlayerGetAll().FindAll(p => p?.photonView?.ViewID != userViewId)
                        .ConvertAll(p => p?.photonView?.Owner?.NickName ?? "Unknown"));
                SharingIsCaringPlugin.Log.LogDebug($"Healed players: {names}");
            }
        }
    }
}
