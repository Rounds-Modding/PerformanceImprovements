using System;
using HarmonyLib;
using UnityEngine;
using UnboundLib;
using PerformanceImprovements.Utils;
using System.Collections;
using System.Linq;

namespace PerformanceImprovements.Patches
{
    [HarmonyPatch(typeof(PlayerSkinParticle), "Init")]
    class PlayerSkinParticlePatchInit
    {
        private static void Postfix(ParticleSystem ___part)
        {
            if (PerformanceImprovements.DisableAllParticleAnimations.Value)
            {
                ___part?.Pause();
            }
        }
    }
    [HarmonyPatch(typeof(PlayerSkinParticle), "OnEnable")]
    class PlayerSkinParticlePatchOnEnable
    {
        private static void Postfix(ParticleSystem ___part)
        {
            if (PerformanceImprovements.DisableAllParticleAnimations.Value)
            {
                ___part?.Pause();
            }
        }
    }
    [HarmonyPatch(typeof(PlayerSkinParticle), "BlinkColor")]
    class PlayerSkinParticlePatchBlinkColor
    {
        private static void Prefix(ParticleSystem ___part, ref ParticleSystem.Particle[] ___particles, Color blinkColor)
        {

            if (PerformanceImprovements.DisableAllParticleAnimations.Value)
            {

                ___particles = new ParticleSystem.Particle[___part.main.maxParticles];
                int num = ___part.GetParticles(___particles);

                Unbound.Instance.StartCoroutine(RestorePartsAndPause(___particles.Select(p => p.GetCurrentColor(___part)).ToArray(), ___part, ___particles, 0.1f));

            }

        }

        private static IEnumerator RestorePartsAndPause(Color32[] colors, ParticleSystem part, ParticleSystem.Particle[] particles, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            int num = particles.Length;
            for (int i = 0; i < num; i++)
            {
                particles[i].startColor = colors[i];
            }
            part.SetParticles(particles, num);
            part.Pause();
            yield break;
        }
    }
}
