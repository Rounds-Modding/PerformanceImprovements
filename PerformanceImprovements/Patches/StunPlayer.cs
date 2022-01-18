using System;
using HarmonyLib;
using UnityEngine;

namespace PerformanceImprovements.Patches
{
    [HarmonyPatch(typeof(StunPlayer), "Go")]
    class StunPlayerPatchGo
    {
        // skip StunPlayer.Go if it will throw a null reference exception
        private static bool Prefix(StunPlayer __instance)
        {
            return (!PerformanceImprovements.FixStunPlayer.Value || __instance.GetComponentInParent<Player>() != null);
        }
    }
}
