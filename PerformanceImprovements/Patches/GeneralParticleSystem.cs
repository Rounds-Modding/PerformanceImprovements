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
    [HarmonyPatch(typeof(GeneralParticleSystem),"Play")]
    class GeneralParticleSystemPatchPlay
    {

		private const float staticRandomRotation = 45f;
		private const float staticRandomXPos = 100f;
		private const float staticRandomYPos = 200f;

        private static bool Prefix(GeneralParticleSystem __instance)
        {
            if (PerformanceImprovements.DisableCardParticleAnimations.Value && PerformanceImprovements.GameInProgress)
            {
                __instance.InvokeMethod("Init", new object[] { });
				int num = (int)UnityEngine.Mathf.Clamp(6, 0, PerformanceImprovements.NumberOfGeneralParticles.Value);
				for (int i = 0; i < num; i++)
                {
					CreateParticleStatic(__instance, i/__instance.duration);
                }
                return false;
            }
            return true;
        }
		private static void CreateParticleStatic(GeneralParticleSystem instance, float currentAnimationTime)
		{
			GameObject spawned = ((ObjectPool)instance.GetFieldValue("particlePool")).GetObject();
			float counter = UnityEngine.Random.Range(0f, instance.particleSettings.lifetime);
			float t = instance.particleSettings.lifetime;
			Vector3 startSize = spawned.transform.localScale;
			Vector3 modifiedStartSize = spawned.transform.localScale * instance.particleSettings.size * instance.sizeMultiplierOverTime.Evaluate(currentAnimationTime * (float)instance.GetFieldValue("sizeMultiplierOverTimeAnimationCurveLength"));
			Image img = spawned.GetComponent<Image>();
			Color startColor = Color.magenta;
			if (img)
			{
				startColor = img.color;
			}
			if (img)
			{
				float value = UnityEngine.Random.value;
				if (instance.particleSettings.color != Color.magenta)
				{
					img.color = instance.particleSettings.color;
				}
				if (instance.particleSettings.randomColor != Color.magenta)
				{
					img.color = Color.Lerp(img.color, instance.particleSettings.randomColor, value);
				}
				if (!instance.particleSettings.singleRandomValueColor)
				{
					value = UnityEngine.Random.value;
				}
				if (instance.particleSettings.randomAddedColor != Color.black)
				{
					img.color += Color.Lerp(Color.black, instance.particleSettings.randomAddedColor, value);
				}
				if (!instance.particleSettings.singleRandomValueColor)
				{
					value = UnityEngine.Random.value;
				}
				if (instance.particleSettings.randomAddedSaturation != 0f || instance.saturationMultiplier != 1f)
				{
					float h;
					float num;
					float v;
					Color.RGBToHSV(img.color, out h, out num, out v);
					num += value * instance.particleSettings.randomAddedSaturation;
					num *= instance.saturationMultiplier;
					img.color = Color.HSVToRGB(h, num, v);
				}
			}
			spawned.transform.Rotate(instance.transform.forward * instance.particleSettings.rotation);
			spawned.transform.Rotate(instance.transform.forward * UnityEngine.Random.Range(-staticRandomRotation, staticRandomRotation));
			spawned.transform.localPosition = Vector3.zero;
			spawned.transform.position += instance.transform.up * UnityEngine.Random.Range(-staticRandomYPos, staticRandomYPos);
			spawned.transform.position += instance.transform.right * UnityEngine.Random.Range(-staticRandomXPos, staticRandomXPos);
			spawned.transform.position += instance.transform.forward * UnityEngine.Random.Range(-0.1f, 0.1f);
			spawned.transform.localPosition += instance.transform.up * UnityEngine.Random.Range(-staticRandomYPos, staticRandomYPos);
			spawned.transform.localPosition += instance.transform.right * UnityEngine.Random.Range(-staticRandomXPos, staticRandomXPos);
			spawned.transform.localPosition += instance.transform.forward * UnityEngine.Random.Range(-0.1f, 0.1f);
			if (instance.particleSettings.sizeOverTime.keys.Length > 1)
			{
				spawned.transform.localScale = modifiedStartSize * instance.particleSettings.sizeOverTime.Evaluate(counter / t * (float)instance.GetFieldValue("sizeOverTimeAnimationCurveLength"));
			}
			float num2 = instance.particleSettings.alphaOverTime.Evaluate(counter / t * (float)instance.GetFieldValue("alphaOverTimeAnimationCurveLength"));
			if (img && img.color.a != num2)
			{
				img.color = new Color(img.color.r, img.color.g, img.color.b, num2);
			}
			return;
			/*
			while (counter < t)
			{

				if (instance.particleSettings.sizeOverTime.keys.Length > 1)
				{
					spawned.transform.localScale = modifiedStartSize * instance.particleSettings.sizeOverTime.Evaluate(counter / t * (float)instance.GetFieldValue("sizeOverTimeAnimationCurveLength"));
				}
				float num2 = instance.particleSettings.alphaOverTime.Evaluate(counter / t * (float)instance.GetFieldValue("alphaOverTimeAnimationCurveLength"));
				if (img && img.color.a != num2)
				{
					img.color = new Color(img.color.r, img.color.g, img.color.b, num2);
				}
				counter += (instance.useTimeScale ? TimeHandler.deltaTime : Time.unscaledDeltaTime) * (instance.simulationSpeed * instance.simulationSpeedMultiplier);
				yield return null;
			}

			if (img)
			{
				img.color = startColor;
			}
			spawned.transform.localScale = startSize;
				
			
			//((ObjectPool)instance.GetFieldValue("particlePool")).ReleaseObject(spawned);
			yield break;*/
		}
	}
    [HarmonyPatch(typeof(ObjectPool),MethodType.Constructor)]
	[HarmonyPatch(new Type[] {typeof(GameObject), typeof(int), typeof(Transform)})]
    class ObjectPoolPatchConstructor
    {
        private static void Prefix(GameObject prefab, ref int initSpawn, Transform parent)
        {
			if (PerformanceImprovements.GameInProgress)
            {
				initSpawn = (int)UnityEngine.Mathf.Clamp(initSpawn, 0f, PerformanceImprovements.NumberOfGeneralParticles.Value);
			}
		}
    }
}
