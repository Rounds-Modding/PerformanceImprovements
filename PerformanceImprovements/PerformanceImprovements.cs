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

        private static bool _GameInProgress = false;
        internal static bool GameInProgress
        {
            get
            {
                if (GM_Test.instance != null && GM_Test.instance.gameObject != null)
                {
                    return (_GameInProgress || GM_Test.instance.gameObject.activeInHierarchy);
                }
                else
                {
                    return _GameInProgress;
                }

            }
            set
            {
                _GameInProgress = value;
            }
        }

        internal static AssetBundle Assets;

        public static ConfigEntry<bool> DisableCardParticleAnimations;
        public static ConfigEntry<int> NumberOfGeneralParticles;
        public static ConfigEntry<bool> DisablePlayerParticles;
        public static ConfigEntry<bool> DisableMapParticles;
        public static ConfigEntry<bool> DisableBackgroundParticles;
        public static ConfigEntry<bool> DisableOverheadLightAndShadows;
        public static ConfigEntry<float> ScreenShakeStrength;
        public static ConfigEntry<float> ChromaticAberrationStrength;
        public static ConfigEntry<bool> DisableOverheadLightShake;
        public static ConfigEntry<bool> DisableAllParticleAnimations;
        public static ConfigEntry<float> MaxNumberOfParticles;


        private void Awake()
        {
            // bind configs with BepInEx
            DisableCardParticleAnimations = Config.Bind(CompatibilityModName, "Disable Card Particle System Animations", true);
            NumberOfGeneralParticles = Config.Bind(CompatibilityModName, "Number of General Particles", 100);
            DisablePlayerParticles = Config.Bind(CompatibilityModName, "Disable player particles", true);
            DisableMapParticles = Config.Bind(CompatibilityModName, "Disable map particles", true);
            DisableBackgroundParticles = Config.Bind(CompatibilityModName, "Disable background particles", true);
            DisableOverheadLightAndShadows = Config.Bind(CompatibilityModName, "Disable overhead light and shadows", true);
            ScreenShakeStrength = Config.Bind(CompatibilityModName, "Screen shake strength from 0 to 1", 0f);
            ChromaticAberrationStrength = Config.Bind(CompatibilityModName, "Chromatic Aberration Strength from 0 to 1", 0f);
            DisableOverheadLightShake = Config.Bind(CompatibilityModName, "Disable overhead light shake", true);
            DisableAllParticleAnimations = Config.Bind(CompatibilityModName, "Disable all particle animations", true);
            MaxNumberOfParticles = Config.Bind(CompatibilityModName, "Maximum number of particles per renderer", 1000f);
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
            // add modifications to GameFeel
            On.Screenshaker.OnGameFeel += this.Screenshaker_OnGameFeel;
            On.ChomaticAberrationFeeler.OnGameFeel += this.ChomaticAberrationFeeler_OnGameFeel;
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
            void PartAnimChanged(bool val)
            {
                DisableAllParticleAnimations.Value = val;
                CycleArt();
            }
            MenuHandler.CreateToggle(DisableAllParticleAnimations.Value, "Disable All Particle Animations", menu, PartAnimChanged, 30);
            void NumPartsChanged(float val)
            {
                NumberOfGeneralParticles.Value = (int)val;
            }
            MenuHandler.CreateSlider("Max number of particles for GeneralParticleSystems", menu, 30, 10f, 100f, NumberOfGeneralParticles.Value, NumPartsChanged, out Slider slider, true);
            void SimpleSkinChanged(bool val)
            {
                DisablePlayerParticles.Value = val;
                CycleArt();
            }
            MenuHandler.CreateToggle(DisablePlayerParticles.Value, "Use simple player skins", menu, SimpleSkinChanged, 30);
            void MapParticlesChanged(bool val)
            {
                DisableMapParticles.Value = val;
                CycleArt();
            }
            MenuHandler.CreateToggle(DisableMapParticles.Value, "Disable Map Particle Effects", menu, MapParticlesChanged, 30);
            void BackParticlesChanged(bool val)
            {
                DisableBackgroundParticles.Value = val;
                CycleArt();
            }
            MenuHandler.CreateToggle(DisableBackgroundParticles.Value, "Disable Background Particle Effects", menu, BackParticlesChanged, 30);
            void LightShakeChanged(bool val)
            {
                DisableOverheadLightShake.Value = val;
                CycleArt();
            }
            MenuHandler.CreateToggle(DisableOverheadLightShake.Value, "Disable Overhead light shake", menu, LightShakeChanged, 30);
            void LightChanged(bool val)
            {
                DisableOverheadLightAndShadows.Value = val;
                CycleArt();
            }
            MenuHandler.CreateToggle(DisableOverheadLightAndShadows.Value, "Disable Overhead light and shadows", menu, LightChanged, 30);
            void ShakeChanged(float val)
            {
                ScreenShakeStrength.Value = val / 100f;
            }
            void AberrationChanged(float val)
            {
                ChromaticAberrationStrength.Value = val / 100f;
            }
            void MaxParticlesChanged(float val)
            {
                MaxNumberOfParticles.Value = (int)val;
                CycleArt();
            }
            MenuHandler.CreateSlider("Screen Shake Strength", menu, 30, 0f, 100f, (int)(100f*ScreenShakeStrength.Value), ShakeChanged, out Slider _, true);
            MenuHandler.CreateSlider("Chromatic Aberration Strength", menu, 30, 0f, 100f, (int)(100f * ChromaticAberrationStrength.Value), AberrationChanged, out Slider _, true);
            MenuHandler.CreateSlider("Maximum number of particles", menu, 30, 20f, 1000f, MaxNumberOfParticles.Value, MaxParticlesChanged, out Slider _, true);
        }

        private static void CycleArt()
        {
            ArtHandler.instance.NextArt();
            ArtHandler.instance.NextArt();
        }

        private void Screenshaker_OnGameFeel(On.Screenshaker.orig_OnGameFeel orig, global::Screenshaker self, Vector2 feelDirection)
        {
            orig(self, feelDirection * (float)(ScreenShakeStrength.Value));
        }

        private void ChomaticAberrationFeeler_OnGameFeel(On.ChomaticAberrationFeeler.orig_OnGameFeel orig, global::ChomaticAberrationFeeler self, Vector2 feelDirection)
        {
            orig(self, feelDirection * (float)(ChromaticAberrationStrength.Value));
        }
    }
}
