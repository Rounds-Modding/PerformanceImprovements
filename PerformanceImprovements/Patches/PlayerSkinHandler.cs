using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnboundLib;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PerformanceImprovements.Patches
{
    [HarmonyPatch(typeof(PlayerSkinHandler),"Start")]
    class PlayerSkinHandlerPatchStart
    {
        private static void Prefix(PlayerSkinHandler __instance)
        {
            if (PerformanceImprovements.ToggleSimpleSkins.Value)
            {
                __instance.simpleSkin = true;
            }
        }

        internal static void ForceSwitch()
        {
            if (PerformanceImprovements.GameInProgress)
            {
                foreach (Player player in PlayerManager.instance.players)
                {
                    player.gameObject.GetComponentInChildren<PlayerSkinHandler>().ToggleSimpleSkin(PerformanceImprovements.ToggleSimpleSkins.Value);
                }    
            }
        }
		
    }
}
