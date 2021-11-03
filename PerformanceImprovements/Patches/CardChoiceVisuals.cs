using System;
using HarmonyLib;
using UnboundLib;
using UnityEngine;

namespace PerformanceImprovements.Patches
{
    [HarmonyPatch(typeof(CardChoiceVisuals), "Show")]

    class CardChoiceVisualsPatchShow
    {
        private static void Postfix(CardChoiceVisuals __instance, GameObject ___currentSkin)
        {
            if (___currentSkin?.GetComponentInChildren<ParticleSystem>() != null && (PerformanceImprovements.DisablePlayerParticles.Value || PerformanceImprovements.DisableForegroundParticleAnimations.Value))
            {
                ___currentSkin.GetComponentInChildren<ParticleSystem>().Pause();
            }
        }
    }
}
