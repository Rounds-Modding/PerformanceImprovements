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

        internal static int hitEffectsSpawnedThisFrame = 0;

        internal static Color staticGunColor = new Color(0.25f, 0.25f, 0.25f, 1f);

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
        public static ConfigEntry<bool> DisableBackgroundParticleAnimations;
        public static ConfigEntry<bool> DisableForegroundParticleAnimations;
        public static ConfigEntry<float> MaxNumberOfParticles;
        public static ConfigEntry<bool> FixProjectileObjectsToSpawn;
        public static ConfigEntry<bool> FixBulletHitParticleEffects;
        public static ConfigEntry<bool> DisableBulletHitSurfaceParticleEffects;
        public static ConfigEntry<bool> DisableBulletHitBulletParticleEffects;
        public static ConfigEntry<int> MaximumBulletHitParticlesPerFrame;
        public static ConfigEntry<bool> RemoveOutOfBoundsBullets;

        public static PerformanceImprovements instance;
        private PerformanceImprovements()
        {
            instance = this;
        }

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
            DisableBackgroundParticleAnimations = Config.Bind(CompatibilityModName, "Disable background particle animations", true);
            DisableForegroundParticleAnimations = Config.Bind(CompatibilityModName, "Disable foreground particle animations", true);
            MaxNumberOfParticles = Config.Bind(CompatibilityModName, "Maximum number of particles per renderer", 1000f);
            FixProjectileObjectsToSpawn = Config.Bind(CompatibilityModName, "Fix projectile ObjectsToSpawn", true);
            FixBulletHitParticleEffects = Config.Bind(CompatibilityModName, "Fix BulletHit particle effects", true);
            DisableBulletHitSurfaceParticleEffects = Config.Bind(CompatibilityModName, "Disable BulletHitSurface particle effects", true);
            DisableBulletHitBulletParticleEffects = Config.Bind(CompatibilityModName, "Disable BulletHitBullet particle effects", true);
            MaximumBulletHitParticlesPerFrame = Config.Bind(CompatibilityModName, "Maximum BulletHit particles per frame", 100);
            RemoveOutOfBoundsBullets = Config.Bind(CompatibilityModName, "Remove out of bounds bullets", true);
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
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, RemoveAfterPoint);

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

        void LateUpdate()
        {
            hitEffectsSpawnedThisFrame = 0;
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
            void BackPartAnimChanged(bool val)
            {
                DisableBackgroundParticleAnimations.Value = val;
                CycleArt();
            }
            MenuHandler.CreateToggle(DisableBackgroundParticleAnimations.Value, "Disable Background Particle Animations", menu, BackPartAnimChanged, 30);
            void ForePartAnimChanged(bool val)
            {
                DisableForegroundParticleAnimations.Value = val;
                CycleArt();
            }
            MenuHandler.CreateToggle(DisableForegroundParticleAnimations.Value, "Disable Foreground Particle Animations", menu, ForePartAnimChanged, 30);
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
            void FixObjToSpawnChanged(bool val)
            {
                FixProjectileObjectsToSpawn.Value = val;
            }
            MenuHandler.CreateToggle(FixProjectileObjectsToSpawn.Value, "Fix persistance issues with Projectile.ObjectsToSpawn", menu, FixObjToSpawnChanged, 30);
            void FixBulletHitChanged(bool val)
            {
                FixBulletHitParticleEffects.Value = val;
            }
            MenuHandler.CreateToggle(FixBulletHitParticleEffects.Value, "Fix persistance issues with BulletHit particle effects", menu, FixBulletHitChanged, 30);
            void DisableBulletHitChanged(bool val)
            {
                DisableBulletHitSurfaceParticleEffects.Value = val;
            }
            MenuHandler.CreateToggle(DisableBulletHitSurfaceParticleEffects.Value, "Disable BulletHitSurface particle effects", menu, DisableBulletHitChanged, 30);
            void DisableBulletHitBulletChanged(bool val)
            {
                DisableBulletHitBulletParticleEffects.Value = val;
            }
            MenuHandler.CreateToggle(DisableBulletHitBulletParticleEffects.Value, "Disable BulletHitBullet particle effects", menu, DisableBulletHitBulletChanged, 30);
            void MaxBulletHitChanged(float val)
            {
                MaximumBulletHitParticlesPerFrame.Value = (int)val;
            }
            MenuHandler.CreateSlider("Global maximum number of bullethit particles per frame", menu, 30, 0f, 100f, (float)MaximumBulletHitParticlesPerFrame.Value, MaxBulletHitChanged, out Slider _, true);
            void RemoveOOBChanged(bool val)
            {
                RemoveOutOfBoundsBullets.Value = val;
            }
            MenuHandler.CreateToggle(RemoveOutOfBoundsBullets.Value, "Remove out of bounds bullets", menu, RemoveOOBChanged, 30);
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

        private static IEnumerator RemoveAfterPoint(IGameModeHandler gm)
        {
            foreach (UnityEngine.Object obj in Resources.FindObjectsOfTypeAll(typeof(RemoveAfterPoint)))
            {
                if (obj != null) { UnityEngine.GameObject.Destroy(obj); }
            }

            yield break;
        }
    }
}
