using System;
using HarmonyLib;
using UnityEngine;

namespace PerformanceImprovements.Patches
{
    [HarmonyPatch(typeof(ChangeColor),"Start")]
    class ChangeColorPatchStart
    {
        private static void Postfix(ChangeColor __instance)
        {
            if (PerformanceImprovements.DisableBulletHitSurfaceParticleEffects)
            {
                if (__instance != null && __instance.gameObject != null) { UnityEngine.GameObject.Destroy(__instance.gameObject); }
            }
            else if (PerformanceImprovements.FixBulletHitParticleEffects)
            {
                if (__instance!= null && __instance.gameObject!=null)
                {
                    __instance.gameObject.AddComponent<RemoveAfterPoint>();
                    RemoveAfterSeconds rem = __instance.gameObject.GetComponent<RemoveAfterSeconds>();
                    if (rem == null)
                    {
                        rem = __instance.gameObject.AddComponent<RemoveAfterSeconds>();
                        rem.seconds = 2f;
                    }
                }
            }
        }
    }
}
