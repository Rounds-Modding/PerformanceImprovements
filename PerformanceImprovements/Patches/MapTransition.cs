using HarmonyLib;
using UnboundLib;
using PerformanceImprovements;
using System.Reflection.Emit;
using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


namespace PerformanceImprovements.Patches
{
    [HarmonyPatch]
    class MapTransition_Patch_Move
    {
        static Type GetNestedMoveType()
        {
            var nestedTypes = typeof(MapTransition).GetNestedTypes(BindingFlags.Instance | BindingFlags.NonPublic);
            Type nestedType = null;

            foreach (var type in nestedTypes)
            {
                if (type.Name.Contains("Move") && !type.Name.Contains("Object"))
                {
                    nestedType = type;
                    break;
                }
            }

            return nestedType;
        }

        static MethodBase TargetMethod()
        {
            return AccessTools.Method(GetNestedMoveType(), "MoveNext");
        }

        static void Start(AnimationCurve curve, float random_t, Map map)
        {
            if (PerformanceImprovements.FixMapLoadLag && map != null)
            {
                float t = curve.keys[curve.keys.Length - 1].time + random_t;

                if (PerformanceImprovements.mapTransitionPatchInProgress) { return; }

                PerformanceImprovements.instance.StartCoroutine(PerformanceImprovements.instance.MapTransitionScalePostFX(t));
            }
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            MethodInfo m_Start = ExtensionMethods.GetMethodInfo(typeof(MapTransition_Patch_Move), nameof(MapTransition_Patch_Move.Start));
            FieldInfo f_randomDelay = ExtensionMethods.GetFieldInfo(GetNestedMoveType(), "<randomDelay>5__3");
            FieldInfo f_curve = ExtensionMethods.GetFieldInfo(typeof(MapTransition), nameof(MapTransition.curve));
            FieldInfo f_targetMap = ExtensionMethods.GetFieldInfo(GetNestedMoveType(), "targetMap");

            int idx = -1;

            for (int i = 0; i < codes.Count(); i++)
            {
                if (codes[i].StoresField(f_randomDelay))
                {
                    idx = i;
                    break;
                }
            }

            if (idx == -1)
            {
                throw new Exception("[MAPTRANSITION PATCH] INSTRUCTION NOT FOUND.");
            }
            else
            {
                codes.Insert(idx + 1, new CodeInstruction(OpCodes.Ldloc_1)); // Load the MapTransition (this) instance onto the stack [MapTransition (this), ...] (this field was stored in index 1 by the vanilla method)
                codes.Insert(idx + 2, new CodeInstruction(OpCodes.Ldfld, f_curve)); // Load the MapTransition.curve field onto the stack, pops the MapTransition (this) instance off the stack [curve, ...]
                codes.Insert(idx + 3, new CodeInstruction(OpCodes.Ldarg_0)); // Load the MapTransition.<Move>d__40 instance onto the stack [MapTransition.<Move>d__40, curve, ...]
                codes.Insert(idx + 4, new CodeInstruction(OpCodes.Ldfld, f_randomDelay)); // Load the MapTransition.<Move>d__40:<randomDelay>5__3 field onto the stack, pops the MapTransition.<Move>d__40 instance off the stack [randomDelay, curve, ...]
                codes.Insert(idx + 5, new CodeInstruction(OpCodes.Ldarg_0)); // Load the MapTransition.<Move>d__40 instance onto the stack [MapTransition.<Move>d__40, randomDelay, curve, ...]
                codes.Insert(idx + 6, new CodeInstruction(OpCodes.Ldfld, f_targetMap)); // Load the MapTransition.<Move>d__40:targetMap field onto the stack, pops the MapTransition.<Move>d__40 instance off the stack [targetMap, randomDelay, curve, ...]
                codes.Insert(idx + 7, new CodeInstruction(OpCodes.Call, m_Start)); // Calls MapTransition_Patch_Move.Start, taking three arguments off the top of the stack, leaving it how we found it [ ... ]
            }

            return codes.AsEnumerable();
        }
    }
}
