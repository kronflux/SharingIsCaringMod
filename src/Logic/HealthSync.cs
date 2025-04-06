using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using SharingIsCaring.Config;

namespace SharingIsCaring.Logic
{
    /// <summary>
    /// Central logic for team health sharing.
    /// Called from Harmony patch after a health pack is used.
    /// </summary>
    public static class HealthSync
    {
        /// <summary>
        /// Applies health pack healing to all players.
        /// Only the host performs this logic.
        /// </summary>
        public static void OnHealthPackUsed(ItemHealthPack pack)
        {
            if (!PhotonNetwork.IsMasterClient || !ModConfig.EnableHealthSync.Value)
                return;

            int adjustedHeal = Mathf.CeilToInt(pack.healAmount * ModConfig.HealMultiplier.Value);
            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
            int healedCount = 0;

            foreach (var player in players)
            {
                if (player?.playerHealth != null)
                {
                    player.playerHealth.HealOther(adjustedHeal, true);
                    healedCount++;
                }
            }

            SharingIsCaringPlugin.Log.LogInfo(
                $"Shared health pack used → healed {healedCount} player(s) for {adjustedHeal} HP.");

            if (ModConfig.DebugLogging.Value)
            {
                string names = string.Join(", ",
                    players.ConvertAll(p => p?.photonView?.Owner?.NickName ?? "Unknown"));
                SharingIsCaringPlugin.Log.LogDebug($"Healed players: {names}");
            }
        }
    }
}
