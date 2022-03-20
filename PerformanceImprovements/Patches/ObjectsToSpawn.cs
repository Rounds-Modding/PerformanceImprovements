using System;
using HarmonyLib;
using UnboundLib;
using UnityEngine;

namespace PerformanceImprovements.Patches
{
    [HarmonyPatch(typeof(ObjectsToSpawn),"SpawnObject")]
    [HarmonyPatch(new Type[] { typeof(Transform), typeof(HitInfo), typeof(ObjectsToSpawn), typeof(HealthHandler), typeof(PlayerSkin), typeof(float), typeof(SpawnedAttack), typeof(bool) })]

    class ObjectsToSpawnPatchSpawnObject
    {
        private static void Postfix(ref GameObject[] __result)
        {
            if (PerformanceImprovements.FixProjectileObjectsToSpawn)
            {
                foreach (GameObject obj in __result)
                {
                    if (obj != null) { obj.AddComponent<RemoveAfterPoint>(); }
                }
            }
        }
    }
    [HarmonyPatch(typeof(ObjectsToSpawn), "SpawnObject")]
    [HarmonyPatch(new Type[] { typeof(ObjectsToSpawn), typeof(Vector3), typeof(Quaternion) })]

    class ObjectsToSpawnPatchSpawnObject2
    {
        private static bool Prefix(ObjectsToSpawn objectToSpawn, Vector3 position,  Quaternion rotation)
        {
            if (PerformanceImprovements.FixProjectileObjectsToSpawn)
            {
                for (int i = 0; i < objectToSpawn.numberOfSpawns; i++)
                {
                    UnityEngine.GameObject obj = UnityEngine.GameObject.Instantiate<GameObject>(objectToSpawn.effect, position, rotation);
                    if (obj != null) { obj.AddComponent<RemoveAfterPoint>(); }
                }

                return false;

            }
            else
            {
                return true;
            }
        }
    }

    internal class RemoveAfterPoint : MonoBehaviour
    {
    }
}
