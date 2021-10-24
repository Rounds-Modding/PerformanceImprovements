using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnboundLib;

namespace PerformanceImprovements.Patches
{
	[HarmonyPatch(typeof(ArtHandler), "NextArt")]
	internal class ArtHandler_Patch
	{
		private static GameObject Rendering => GameObject.Find("/Game/Visual/Rendering ");
		private static GameObject FrontParticles => Rendering?.transform?.GetChild(1)?.gameObject;
		private static GameObject BackParticles => Rendering?.transform?.GetChild(0)?.gameObject;
		private static GameObject Light => Rendering?.transform?.GetChild(3)?.GetChild(0)?.gameObject;

		private static Color staticGunColor = new Color(0.25f, 0.25f, 0.25f, 1f);

		private static void Postfix()
		{
			foreach (ParticleSystem particleSystem in UnityEngine.Object.FindObjectsOfType<ParticleSystem>())
			{
				if (particleSystem.gameObject.name.Contains("Skin_Player"))
				{
					particleSystem.SetPropertyValue("enableEmission", !PerformanceImprovements.DisablePlayerParticles.Value);

					Player player = particleSystem.gameObject.GetComponentInParent<Player>();
					Gun gun = player.GetComponent<Holding>().holdable.GetComponent<Gun>();
					GameObject spring = gun.gameObject.transform.GetChild(1).gameObject;
					GameObject handle = spring.transform.GetChild(2).gameObject;
					GameObject barrel = spring.transform.GetChild(3).gameObject;

					handle.GetComponent<SpriteMask>().enabled = !PerformanceImprovements.DisablePlayerParticles.Value;
					handle.GetComponent<SpriteRenderer>().enabled = PerformanceImprovements.DisablePlayerParticles.Value;
					handle.GetComponent<SpriteRenderer>().color = staticGunColor;
					barrel.GetComponent<SpriteMask>().enabled = !PerformanceImprovements.DisablePlayerParticles.Value;
					barrel.GetComponent<SpriteRenderer>().enabled = PerformanceImprovements.DisablePlayerParticles.Value;
					barrel.GetComponent<SpriteRenderer>().color = staticGunColor;
				}
			}
			BackParticles?.SetActive(!PerformanceImprovements.DisableBackgroundParticles.Value);
			FrontParticles?.SetActive(!PerformanceImprovements.DisableMapParticles.Value);
			Light?.SetActive(!PerformanceImprovements.DisableOverheadLightAndShadows.Value);
			/*
			GameObject backgroundpart = GameObject.Find("BackgroudParticles");
			if (backgroundpart != null)
			{
				backgroundpart.SetActive(!PerformanceImprovements.DisableMapParticles.Value);
			}
			GameObject foregroundpart = GameObject.Find("FrontParticles");
			if (foregroundpart != null)
			{
				foregroundpart.SetActive(!PerformanceImprovements.DisableMapParticles.Value);
			}
			GameObject light = GameObject.Find("Light");
			if (light != null)
			{
				light.SetActive(!PerformanceImprovements.DisableOverheadLightAndShadows.Value);
			}*/
		}
	}
}
