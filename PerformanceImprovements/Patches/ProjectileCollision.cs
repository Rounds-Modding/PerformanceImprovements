using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace PerformanceImprovements.Patches
{
    [Serializable]
    [HarmonyPatch(typeof(ProjectileCollision), "Die")]
    class ProjectileCollisionPatchDie
    {
        private static HitInfo GetHitInfo(RaycastHit2D raycastHit2D)
        {
            return new HitInfo
            {
                point = raycastHit2D.point,
                normal = raycastHit2D.normal,
                transform = raycastHit2D.transform,
                collider = raycastHit2D.collider,
                rigidbody = raycastHit2D.rigidbody
            };
        }
        private static bool Prefix(ProjectileCollision __instance, ref bool ___hasCollided, float ___startDMG)
        {
            if (___hasCollided)
            {
                return false;
            }
            ___hasCollided = true;
            RaycastHit2D raycastHit2D = default(RaycastHit2D);
            raycastHit2D.normal = -__instance.transform.root.forward;
            raycastHit2D.point = __instance.transform.position;
            if (!PerformanceImprovements.DisableBulletHitBulletParticleEffects.Value && !(PerformanceImprovements.hitEffectsSpawnedThisFrame >= PerformanceImprovements.MaximumBulletHitParticlesPerFrame.Value))
            {
                PerformanceImprovements.hitEffectsSpawnedThisFrame++;
                GameObject spark = GameObject.Instantiate<GameObject>(__instance.sparkObject, __instance.transform.position, __instance.transform.rotation);
                spark.transform.localScale = Vector3.one * ((___startDMG / 55f + 1f) * 0.5f);
                if (PerformanceImprovements.FixBulletHitParticleEffects.Value)
                {
                    spark.AddComponent<RemoveAfterPoint>();
                    RemoveAfterSeconds rem = spark.GetComponent<RemoveAfterSeconds>();
                    if (rem == null)
                    {
                        rem = spark.AddComponent<RemoveAfterSeconds>();
                        rem.seconds = 2f;
                    }
                }
            }
            __instance.GetComponentInParent<ProjectileHit>().Hit(GetHitInfo(raycastHit2D), true);

            return false;
        }
    }
}
