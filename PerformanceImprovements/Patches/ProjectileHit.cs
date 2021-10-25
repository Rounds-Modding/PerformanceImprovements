using System;
using HarmonyLib;
using UnboundLib;
using UnityEngine;
using PerformanceImprovements.Extensions;

namespace PerformanceImprovements.Patches
{
    [Serializable]
    [HarmonyPatch(typeof(ProjectileHit), "Update")]
    class ProjectileHitPatchUpdate
    {
        private const float delay = 0.5f;

        private const float xmin = -0.25f;
        private const float xmax = 1.25f;
        private const float ymin = -1f;
        private const float ymax = float.MaxValue;

        private static bool IsOutOfBounds(Transform transform)
        {

            Vector3 vector = MainCam.instance.transform.GetComponent<Camera>().FixedWorldToScreenPoint(new Vector3(transform.position.x, transform.position.y, 0f));

            vector.x /= (float)FixedScreen.fixedWidth;
            vector.y /= (float)Screen.height;

            if (vector.x <= xmin || vector.x >= xmax || vector.y <= ymin)
            {
                return true;
            }

            return false;

        }

        private static void TryDestroy(ProjectileHit __instance)
        {
            if (__instance != null && __instance.gameObject != null) { UnityEngine.GameObject.Destroy(__instance.gameObject); }
        }

        private static void Prefix(ProjectileHit __instance)
        {
            if (PerformanceImprovements.RemoveOutOfBoundsBullets.Value)
            {
                if (Time.time >= __instance.GetAdditionalData().lastCheckedTime + delay)
                {
                    __instance.GetAdditionalData().lastCheckedTime = Time.time;
                    if (IsOutOfBounds(__instance.transform))
                    {
                        TryDestroy(__instance);
                    }
                }
            }
        }
    }
    [Serializable]
    [HarmonyPatch(typeof(ProjectileHit),"Start")]
    class ProjectileHitPatchStart
    {
        private static void Postfix(ProjectileHit __instance)
        {
            __instance.GetAdditionalData().lastCheckedTime = -1f;
        }
    }

    // extension methods for dealing with ultrawide displays
    internal static class CameraExtension
    {
        private static float correction => (Screen.width - FixedScreen.fixedWidth) / 2f;
        internal static Vector3 FixedWorldToScreenPoint(this Camera camera, Vector3 worldPoint)
        {
            Vector3 fixedScreenPoint = camera.WorldToScreenPoint(worldPoint);
            if (!FixedScreen.isUltraWide) { return fixedScreenPoint; }

            return new Vector3(fixedScreenPoint.x - correction, fixedScreenPoint.y, fixedScreenPoint.z);
        }
        internal static Vector3 FixedScreenToWorldPoint(this Camera camera, Vector3 fixedScreenPoint)
        {
            Vector3 worldPoint = camera.ScreenToWorldPoint(fixedScreenPoint);
            if (!FixedScreen.isUltraWide) { return worldPoint; }

            return new Vector3(fixedScreenPoint.x + correction, fixedScreenPoint.y, fixedScreenPoint.z);
        }
    }

    // extension for dealing with ultrawide displays
    internal static class FixedScreen
    {
        internal static bool isUltraWide => ((float)Screen.width / (float)Screen.height - ratio >= eps);
        private const float ratio = 16f / 9f;
        private const float eps = 1E-4f;
        internal static int fixedWidth
        {
            get
            {
                if (isUltraWide)
                {
                    // widescreen (or at least nonstandard screen)
                    // we assume the height is correct (since the game seems to scale to force the height to match)
                    return (int)UnityEngine.Mathf.RoundToInt(Screen.height * ratio);
                }
                else
                {
                    return Screen.width;
                }
            }
            private set { }
        }
    }
}
