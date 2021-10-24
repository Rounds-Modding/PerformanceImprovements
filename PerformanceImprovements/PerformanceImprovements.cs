using System;
using BepInEx;
using BepInEx.Configuration;
using UnboundLib;
using HarmonyLib;
using UnityEngine;
using Jotunn.Utils;
using System.Runtime.CompilerServices;
using System.Reflection;
using UnboundLib.Utils.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using On;
using UnboundLib.GameModes;
using System.Collections;
using PerformanceImprovements.Patches;

namespace PerformanceImprovements
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, "0.0.0")]
    [BepInProcess("Rounds.exe")]
    public class PerformanceImprovements : BaseUnityPlugin
    {
        private const string ModId = "pykess-and-ascyst.rounds.plugins.performanceimprovements";
        private const string ModName = "Performance Improvements";
        private const string CompatibilityModName = "PerformanceImprovements";

        internal static bool GameInProgress = false;

        internal static AssetBundle Assets;

        public static ConfigEntry<bool> DisableCardParticleAnimations;
        public static ConfigEntry<int> NumberOfGeneralParticles;
        public static ConfigEntry<bool> ToggleSimpleSkins;


        private void Awake()
        {
            // bind configs with BepInEx
            DisableCardParticleAnimations = Config.Bind(CompatibilityModName, "Disable Card Particle System Animations", true);
            NumberOfGeneralParticles = Config.Bind(CompatibilityModName, "Number of General Particles", 100);
            ToggleSimpleSkins = Config.Bind(CompatibilityModName, "Use Simple Player Skins", true);
            // apply patches
            new Harmony(ModId).PatchAll();
        }
        private void Start()
        {
            // load assets
            //PerformanceImprovements.Assets = AssetUtils.LoadAssetBundleFromResources("performance", typeof(PerformanceImprovements).Assembly);

            // add credits
            Unbound.RegisterCredits(ModName, new string[] { "Pykess", "Ascyst" }, new string[] { "github", "Buy Pykess a coffee", "Buy Ascyst a coffee" }, new string[] { "https://github.com/Rounds-Modding/PerformanceImprovements", "https://www.buymeacoffee.com/Pykess", "https://www.buymeacoffee.com/Ascyst" });

            // add GUI to modoptions menu
            Unbound.RegisterMenu(ModName, () => { }, this.NewGUI, null, true);

            // register as client-side
            Unbound.RegisterClientSideMod(ModId);

            GameModeManager.AddHook(GameModeHooks.HookGameStart, (gm) => SetGameInProgress(true));

            // reset GameInProgress
            On.MainMenuHandler.Awake += (orig, self) =>
            {
                PerformanceImprovements.GameInProgress = false;

                orig(self);
            };
        }
        private static IEnumerator SetGameInProgress(bool inProgress)
        {
            PerformanceImprovements.GameInProgress = inProgress;
            yield break;
        }
        private void NewGUI(GameObject menu)
        {
            MenuHandler.CreateText(ModName + " Options", menu, out TextMeshProUGUI _, 60);
            void CardPartAnimChanged(bool val)
            {
                DisableCardParticleAnimations.Value = val;
            }
            MenuHandler.CreateToggle(DisableCardParticleAnimations.Value, "Disable Card Particle Animations", menu, CardPartAnimChanged, 30);
            void NumPartsChanged(float val)
            {
                NumberOfGeneralParticles.Value = (int)val;
            }
            MenuHandler.CreateSlider("Max number of particles for GeneralParticleSystems", menu, 30, 10f, 100f, NumberOfGeneralParticles.Value, NumPartsChanged, out Slider slider, true);
            void SimpleSkinChanged(bool val)
            {
                ToggleSimpleSkins.Value = val;
                if (PerformanceImprovements.GameInProgress)
                {
                    PlayerSkinHandlerPatchStart.ForceSwitch();
                }
            }
            MenuHandler.CreateToggle(ToggleSimpleSkins.Value, "Use simple player skins", menu, SimpleSkinChanged, 30);
        }
    }
}
