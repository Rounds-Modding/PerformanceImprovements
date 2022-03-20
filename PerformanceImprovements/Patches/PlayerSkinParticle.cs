using System;
using HarmonyLib;
using UnityEngine;
using UnboundLib;
using System.Collections;
using System.Linq;
using PerformanceImprovements.Utils;

namespace PerformanceImprovements.Patches
{
    [HarmonyPatch(typeof(PlayerSkinParticle), "Init")]
    class PlayerSkinParticlePatchInit
    {
        private static void Postfix(PlayerSkinParticle __instance, ParticleSystem ___part)
        {
            if (___part != null)
            {
                ___part.enableEmission = !PerformanceImprovements.DisablePlayerParticles;
            }
            if (__instance != null)
            {
                Gun gun = __instance.transform.parent.GetComponentInParent<Player>().GetComponent<Holding>().holdable.GetComponent<Gun>();
                GameObject spring = gun.gameObject.transform.GetChild(1).gameObject;
                GameObject handle = spring.transform.GetChild(2).gameObject;
                GameObject barrel = spring.transform.GetChild(3).gameObject;

                handle.GetComponent<SpriteMask>().enabled = !PerformanceImprovements.DisablePlayerParticles;
                handle.GetComponent<SpriteRenderer>().enabled = PerformanceImprovements.DisablePlayerParticles;
                handle.GetComponent<SpriteRenderer>().color = PerformanceImprovements.staticGunColor;
                barrel.GetComponent<SpriteMask>().enabled = !PerformanceImprovements.DisablePlayerParticles;
                barrel.GetComponent<SpriteRenderer>().enabled = PerformanceImprovements.DisablePlayerParticles;
                barrel.GetComponent<SpriteRenderer>().color = PerformanceImprovements.staticGunColor;
            }
        }
    }
    [HarmonyPatch(typeof(PlayerSkinParticle), "OnEnable")]
    class PlayerSkinParticlePatchOnEnable
    {
        private static void Postfix(PlayerSkinParticle __instance, ParticleSystem ___part)
        {
            if (___part != null) 
            { 
                ___part.enableEmission = !PerformanceImprovements.DisablePlayerParticles;
            }
            if (__instance != null)
            {
                Gun gun = __instance?.transform?.parent?.GetComponentInParent<Player>()?.GetComponent<Holding>()?.holdable?.GetComponent<Gun>();
                if (gun == null) { return; }
                GameObject spring = gun.gameObject.transform.GetChild(1).gameObject;
                GameObject handle = spring.transform.GetChild(2).gameObject;
                GameObject barrel = spring.transform.GetChild(3).gameObject;

                handle.GetComponent<SpriteMask>().enabled = !PerformanceImprovements.DisablePlayerParticles;
                handle.GetComponent<SpriteRenderer>().enabled = PerformanceImprovements.DisablePlayerParticles;
                handle.GetComponent<SpriteRenderer>().color = PerformanceImprovements.staticGunColor;
                barrel.GetComponent<SpriteMask>().enabled = !PerformanceImprovements.DisablePlayerParticles;
                barrel.GetComponent<SpriteRenderer>().enabled = PerformanceImprovements.DisablePlayerParticles;
                barrel.GetComponent<SpriteRenderer>().color = PerformanceImprovements.staticGunColor;
            }
        }
    }
    [HarmonyPatch(typeof(PlayerSkinParticle), "BlinkColor")]
    class PlayerSkinParticlePatchBlinkColor
    {

        private const float blinkTime = 0.2f;
        private static bool Prefix(PlayerSkinParticle __instance, Color blinkColor)
        {

            if (PerformanceImprovements.DisablePlayerParticles)
            {
                ColorFlash effect = __instance.transform.parent.GetComponentInParent<Player>().gameObject.AddComponent<ColorFlash>();
                effect.SetColor(blinkColor);
                effect.SetNumberOfFlashes(1);
                effect.SetDuration(blinkTime);
            }

            return !PerformanceImprovements.DisablePlayerParticles;

        }
    }
}
