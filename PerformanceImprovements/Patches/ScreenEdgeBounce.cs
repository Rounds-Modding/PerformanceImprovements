using System;
using HarmonyLib;
using UnboundLib;
using UnityEngine;
using Photon.Pun;

namespace PerformanceImprovements.Patches
{
    [Serializable]
    [HarmonyPatch(typeof(ScreenEdgeBounce), "DoHit")]
    class ScreenEdgeBouncePatchDoHit
    {
        private static void TryDestroy(ScreenEdgeBounce __instance)
        {
            if (__instance != null && __instance.gameObject != null) { UnityEngine.GameObject.Destroy(__instance.gameObject); }
        }
        static Exception Finalizer(ScreenEdgeBounce __instance, Exception __exception)
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
    [Serializable]
    [HarmonyPatch(typeof(ScreenEdgeBounce), "Update")]
    class ScreenEdgeBouncePatchUpdate
    {
        private static void TryDestroy(ScreenEdgeBounce __instance)
        {
            if (__instance != null && __instance.gameObject != null) { UnityEngine.GameObject.Destroy(__instance.gameObject); }
        }
        static Exception Finalizer(ScreenEdgeBounce __instance, Exception __exception)
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
