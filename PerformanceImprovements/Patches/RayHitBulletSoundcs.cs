using System;
using HarmonyLib;

namespace PerformanceImprovements.Patches
{
    [Serializable]
    [HarmonyPatch(typeof(RayHitBulletSound), "DoHitEffect")]
    class RayHitBulletSoundPatchDoHitEffect
    {
        private static void TryDestroy(RayHitBulletSound __instance)
        {
            if (__instance != null && __instance.gameObject != null) { UnityEngine.GameObject.Destroy(__instance.gameObject); }
        }
        static Exception Finalizer(RayHitBulletSound __instance, Exception __exception)
        {
            if (__exception is NullReferenceException)
            {
                TryDestroy(__instance);
                return null;
            }
            else
            {
                return __exception;
            }
        }
    }
}
