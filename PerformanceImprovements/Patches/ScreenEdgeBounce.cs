using System;
using HarmonyLib;
using UnboundLib;

namespace PerformanceImprovements.Patches
{
    [Serializable]
    [HarmonyPatch(typeof(ScreenEdgeBounce), "DoHit")]
    class ScreenEdgeBouncePatchDoHit
    {
        static void Finalizer(RayHitBulletSound __instance, Exception __exception)
        {
            if (__exception is NullReferenceException)
            {
                UnityEngine.GameObject.Destroy(__instance.gameObject);
            }
        }
    }
}
