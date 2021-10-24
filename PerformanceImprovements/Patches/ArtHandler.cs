using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnboundLib;
using System.Collections;

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

		private static IEnumerator PlayForSeconds(ParticleSystem part, float duration, float delay)
        {
			yield return new WaitForSecondsRealtime(delay);
			part?.Play();
			yield return new WaitForSecondsRealtime(duration);
			part?.Pause();
			yield break;
        }

		private static void Postfix()
		{
			foreach (ParticleSystem particleSystem in UnityEngine.Object.FindObjectsOfType<ParticleSystem>())
			{
				ParticleSystem.MainModule main = particleSystem.main;
				main.maxParticles = (int)PerformanceImprovements.MaxNumberOfParticles.Value;
				if (particleSystem.gameObject.name.Contains("Skin_Player"))
				{
					if (!PerformanceImprovements.DisablePlayerParticles.Value)
					{
						particleSystem?.Play();
					}
					particleSystem.SetPropertyValue("enableEmission", !PerformanceImprovements.DisablePlayerParticles.Value);
					if (!PerformanceImprovements.DisableAllParticleAnimations.Value || PerformanceImprovements.DisablePlayerParticles.Value)
                    {
						particleSystem?.Clear();
                    }
					else if (PerformanceImprovements.DisableAllParticleAnimations.Value && !PerformanceImprovements.DisablePlayerParticles.Value)
                    {
						Unbound.Instance.StartCoroutine(PlayForSeconds(particleSystem, 0.1f, 0.1f));
                    }

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

				if (PerformanceImprovements.DisableAllParticleAnimations.Value)
				{
					particleSystem?.Pause();
				}
			}
			BackParticles?.SetActive(!PerformanceImprovements.DisableBackgroundParticles.Value);
			FrontParticles?.SetActive(!PerformanceImprovements.DisableMapParticles.Value);
			Light?.SetActive(!PerformanceImprovements.DisableOverheadLightAndShadows.Value);
			if (Light && Light.GetComponent<Screenshaker>())
			{
				Light.GetComponentInChildren<Screenshaker>().enabled = !PerformanceImprovements.DisableOverheadLightShake.Value;
			}
			if ((bool)BackParticles?.activeSelf && PerformanceImprovements.DisableAllParticleAnimations.Value)
			{
				BackParticles?.GetComponent<ParticleSystem>()?.Play();
				Unbound.Instance.ExecuteAfterFrames(2, () => { BackParticles?.GetComponent<ParticleSystem>()?.Pause(); });
			}
			if ((bool)FrontParticles?.activeSelf && PerformanceImprovements.DisableAllParticleAnimations.Value)
			{
				FrontParticles?.GetComponent<ParticleSystem>()?.Play();
				Unbound.Instance.ExecuteAfterFrames(2, () => { FrontParticles?.GetComponent<ParticleSystem>()?.Pause(); });
			}
		}
	}
}
