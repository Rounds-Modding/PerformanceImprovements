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
    [BepInPlugin(ModId, ModName, "0.0.1")]
    [BepInProcess("Rounds.exe")]
    public class PerformanceImprovements : BaseUnityPlugin
    {
        private const string ModId = "pykess.rounds.plugins.performanceimprovements";
        internal const string ModName = "Performance Improvements";
        private const string CompatibilityModName = "PerformanceImprovements";

        internal static int hitEffectsSpawnedThisFrame = 0;

        internal static Color staticGunColor = new Color(0.25f, 0.25f, 0.25f, 1f);

        private static Color easyChangeColor = new Color(0.521f, 1f, 0.521f, 1f);
        private static Color hardChangeColor = new Color(1f, 0.521f, 0.521f, 1f);

        private float PostFXDampening = 1f;
        private const float mapTransitionExtraDelay = 0.5f;
        private const float postFXRampDuration = 1f;

        internal Coroutine dampenPostFXCO = null;

        private static bool BattleInProgress = false;

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

        public static ConfigEntry<bool> DisableCardParticleAnimations;
        public static ConfigEntry<int> NumberOfGeneralParticles;
        public static ConfigEntry<bool> DisablePlayerParticles;
        public static ConfigEntry<bool> DisableMapParticles;
        public static ConfigEntry<bool> DisableBackgroundParticles;
        public static ConfigEntry<bool> DisableOverheadLightAndShadows;
        public static ConfigEntry<int> ScreenShakeStrength;
        public static ConfigEntry<int> ChromaticAberrationStrength;
        public static ConfigEntry<bool> DisableOverheadLightShake;
        public static ConfigEntry<bool> DisableBackgroundParticleAnimations;
        public static ConfigEntry<bool> DisableForegroundParticleAnimations;
        public static ConfigEntry<int> MaxNumberOfParticles;
        public static ConfigEntry<bool> FixProjectileObjectsToSpawn;
        public static ConfigEntry<bool> FixBulletHitParticleEffects;
        public static ConfigEntry<bool> FixMapLoadLag;
        public static ConfigEntry<bool> DisableBulletHitSurfaceParticleEffects;
        public static ConfigEntry<bool> DisableBulletHitBulletParticleEffects;
        public static ConfigEntry<int> MaximumBulletHitParticlesPerFrame;
        public static ConfigEntry<bool> RemoveOutOfBoundsBullets;

        internal static Dictionary<ConfigEntry<bool>, List<Toggle>> TogglesToSync = new Dictionary<ConfigEntry<bool>, List<Toggle>>();
        internal static Dictionary<ConfigEntry<int>, List<Slider>> SlidersToSync = new Dictionary<ConfigEntry<int>, List<Slider>>();

        public static PerformanceImprovements instance;
        private PerformanceImprovements()
        {
            instance = this;
        }
        private float ScreenShakeValue
        {
            get
            {
                return PerformanceImprovements.ScreenShakeStrength.Value * this.PostFXDampening;
            }
            set { }
        }
        private float AberrationValue
        {
            get
            {
                return PerformanceImprovements.ChromaticAberrationStrength.Value * this.PostFXDampening;
            }
            set { }
        }

        private void Awake()
        {
            // bind configs with BepInEx
            DisableCardParticleAnimations = Config.Bind(CompatibilityModName, "Disable Card Particle System Animations", false);
            NumberOfGeneralParticles = Config.Bind(CompatibilityModName, "Number of General Particles", 100);
            DisablePlayerParticles = Config.Bind(CompatibilityModName, "Disable player particles", false);
            DisableMapParticles = Config.Bind(CompatibilityModName, "Disable map particles", false);
            DisableBackgroundParticles = Config.Bind(CompatibilityModName, "Disable background particles", false);
            DisableOverheadLightAndShadows = Config.Bind(CompatibilityModName, "Disable overhead light and shadows", false);
            ScreenShakeStrength = Config.Bind(CompatibilityModName, "Screen shake strength from 0 to 1", 100);
            ChromaticAberrationStrength = Config.Bind(CompatibilityModName, "Chromatic Aberration Strength from 0 to 1", 100);
            DisableOverheadLightShake = Config.Bind(CompatibilityModName, "Disable overhead light shake", false);
            DisableBackgroundParticleAnimations = Config.Bind(CompatibilityModName, "Disable background particle animations", false);
            DisableForegroundParticleAnimations = Config.Bind(CompatibilityModName, "Disable foreground particle animations", false);
            MaxNumberOfParticles = Config.Bind(CompatibilityModName, "Maximum number of particles per renderer", 300);
            FixProjectileObjectsToSpawn = Config.Bind(CompatibilityModName, "Fix projectile ObjectsToSpawn", true);
            FixBulletHitParticleEffects = Config.Bind(CompatibilityModName, "Fix BulletHit particle effects", true);
            FixMapLoadLag = Config.Bind(CompatibilityModName, "Fix lag when maps first load", true);
            DisableBulletHitSurfaceParticleEffects = Config.Bind(CompatibilityModName, "Disable BulletHitSurface particle effects", false);
            DisableBulletHitBulletParticleEffects = Config.Bind(CompatibilityModName, "Disable BulletHitBullet particle effects", false);
            MaximumBulletHitParticlesPerFrame = Config.Bind(CompatibilityModName, "Maximum BulletHit particles per frame", 5);
            RemoveOutOfBoundsBullets = Config.Bind(CompatibilityModName, "Remove out of bounds bullets", false);
            // apply patches
            new Harmony(ModId).PatchAll();
        }
        private void Start()
        {
            // load assets
            //PerformanceImprovements.Assets = AssetUtils.LoadAssetBundleFromResources("performance", typeof(PerformanceImprovements).Assembly);

            // add credits
            Unbound.RegisterCredits(ModName, new string[] { "Pykess", "Ascyst (Original RemovePostFX mod)" }, new string[] { "github", "Buy Pykess a coffee", "Buy Ascyst a coffee" }, new string[] { "https://github.com/Rounds-Modding/PerformanceImprovements", "https://www.buymeacoffee.com/Pykess", "https://www.buymeacoffee.com/Ascyst" });

            // add GUI to modoptions menu
            Unbound.RegisterMenu(ModName, () => { }, NewGUI, null, true);

            // register as client-side
            Unbound.RegisterClientSideMod(ModId);

            GameModeManager.AddHook(GameModeHooks.HookGameStart, (gm) => SetGameInProgress(true));
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, RemoveAfterPoint);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, (gm) => SetBattleInProgress(false));
            GameModeManager.AddHook(GameModeHooks.HookBattleStart, (gm) => SetBattleInProgress(true));

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
        private static IEnumerator SetBattleInProgress(bool inProgress)
        {
            PerformanceImprovements.BattleInProgress = inProgress;
            yield break;
        }
        private static IEnumerator SetGameInProgress(bool inProgress)
        {
            if (inProgress) { PerformanceImprovements.instance.PostFXDampening = 1f; }
            PerformanceImprovements.GameInProgress = inProgress;
            yield break;
        }

        private static void VanillaPreset()
        {
            DisableCardParticleAnimations.Value = false;
            NumberOfGeneralParticles.Value = 100;
            DisablePlayerParticles.Value = false;
            DisableMapParticles.Value = false;
            DisableBackgroundParticles.Value = false;
            DisableOverheadLightAndShadows.Value = false;
            ScreenShakeStrength.Value = 100;
            ChromaticAberrationStrength.Value = 100;
            DisableOverheadLightShake.Value = false;
            DisableBackgroundParticleAnimations.Value = false;
            DisableForegroundParticleAnimations.Value = false;
            MaxNumberOfParticles.Value = 1000;
            FixProjectileObjectsToSpawn.Value = false;
            FixBulletHitParticleEffects.Value = false;
            FixMapLoadLag.Value = false;
            DisableBulletHitSurfaceParticleEffects.Value = false;
            DisableBulletHitBulletParticleEffects.Value = false;
            MaximumBulletHitParticlesPerFrame.Value = 100;
            RemoveOutOfBoundsBullets.Value = false;
            CycleArt();
            SyncOptionsMenus();
    }
        private static void OnlyBugFixPreset()
        {
            DisableCardParticleAnimations.Value = false;
            NumberOfGeneralParticles.Value = 100;
            DisablePlayerParticles.Value = false;
            DisableMapParticles.Value = false;
            DisableBackgroundParticles.Value = false;
            DisableOverheadLightAndShadows.Value = false;
            ScreenShakeStrength.Value = 100;
            ChromaticAberrationStrength.Value = 100;
            DisableOverheadLightShake.Value = false;
            DisableBackgroundParticleAnimations.Value = false;
            DisableForegroundParticleAnimations.Value = false;
            MaxNumberOfParticles.Value = 1000;
            FixProjectileObjectsToSpawn.Value = true;
            FixBulletHitParticleEffects.Value = true;
            FixMapLoadLag.Value = true;
            DisableBulletHitSurfaceParticleEffects.Value = false;
            DisableBulletHitBulletParticleEffects.Value = false;
            MaximumBulletHitParticlesPerFrame.Value = 100;
            RemoveOutOfBoundsBullets.Value = false;
            CycleArt();
            SyncOptionsMenus();
        }
        private static void BetterPerformancePreset()
        {
            DisableCardParticleAnimations.Value = false;
            NumberOfGeneralParticles.Value = 20;
            DisablePlayerParticles.Value = false;
            DisableMapParticles.Value = false;
            DisableBackgroundParticles.Value = false;
            DisableOverheadLightAndShadows.Value = false;
            ScreenShakeStrength.Value = 100;
            ChromaticAberrationStrength.Value = 100;
            DisableOverheadLightShake.Value = false;
            DisableBackgroundParticleAnimations.Value = false;
            DisableForegroundParticleAnimations.Value = false;
            MaxNumberOfParticles.Value = 500;
            FixProjectileObjectsToSpawn.Value = true;
            FixBulletHitParticleEffects.Value = true;
            FixMapLoadLag.Value = true;
            DisableBulletHitSurfaceParticleEffects.Value = false;
            DisableBulletHitBulletParticleEffects.Value = false;
            MaximumBulletHitParticlesPerFrame.Value = 10;
            RemoveOutOfBoundsBullets.Value = false;
            CycleArt();
            SyncOptionsMenus();
        }
        private static void HighPerformancePreset()
        {
            DisableCardParticleAnimations.Value = true;
            NumberOfGeneralParticles.Value = 20;
            DisablePlayerParticles.Value = false;
            DisableMapParticles.Value = false;
            DisableBackgroundParticles.Value = false;
            DisableOverheadLightAndShadows.Value = false;
            ScreenShakeStrength.Value = 50;
            ChromaticAberrationStrength.Value = 50;
            DisableOverheadLightShake.Value = false;
            DisableBackgroundParticleAnimations.Value = true;
            DisableForegroundParticleAnimations.Value = true;
            MaxNumberOfParticles.Value = 300;
            FixProjectileObjectsToSpawn.Value = true;
            FixBulletHitParticleEffects.Value = true;
            FixMapLoadLag.Value = true;
            DisableBulletHitSurfaceParticleEffects.Value = false;
            DisableBulletHitBulletParticleEffects.Value = false;
            MaximumBulletHitParticlesPerFrame.Value = 5;
            RemoveOutOfBoundsBullets.Value = false;
            CycleArt();
            SyncOptionsMenus();
        }
        private static void MaxPerformancePreset()
        {
            DisableCardParticleAnimations.Value = true;
            NumberOfGeneralParticles.Value = 10;
            DisablePlayerParticles.Value = true;
            DisableMapParticles.Value = true;
            DisableBackgroundParticles.Value = true;
            DisableOverheadLightAndShadows.Value = true;
            ScreenShakeStrength.Value = 0;
            ChromaticAberrationStrength.Value = 0;
            DisableOverheadLightShake.Value = true;
            DisableBackgroundParticleAnimations.Value = true;
            DisableForegroundParticleAnimations.Value = true;
            MaxNumberOfParticles.Value = 100;
            FixProjectileObjectsToSpawn.Value = true;
            FixBulletHitParticleEffects.Value = true;
            FixMapLoadLag.Value = true;
            DisableBulletHitSurfaceParticleEffects.Value = true;
            DisableBulletHitBulletParticleEffects.Value = true;
            MaximumBulletHitParticlesPerFrame.Value = 3;
            RemoveOutOfBoundsBullets.Value = true;
            CycleArt();
            SyncOptionsMenus();
        }

        private static void InitializeOptionsDictionaries()
        {
            if (!TogglesToSync.Keys.Contains(DisableCardParticleAnimations)) { TogglesToSync[DisableCardParticleAnimations] = new List<Toggle>() { }; }
            if (!SlidersToSync.Keys.Contains(NumberOfGeneralParticles)){ SlidersToSync[NumberOfGeneralParticles] = new List<Slider>(){};}
            if (!TogglesToSync.Keys.Contains(DisablePlayerParticles)){ TogglesToSync[DisablePlayerParticles] = new List<Toggle>(){};}
            if (!TogglesToSync.Keys.Contains(DisableMapParticles)){ TogglesToSync[DisableMapParticles] = new List<Toggle>(){};}
            if (!TogglesToSync.Keys.Contains(DisableBackgroundParticles)){ TogglesToSync[DisableBackgroundParticles] = new List<Toggle>(){};}
            if (!TogglesToSync.Keys.Contains(DisableOverheadLightAndShadows)){ TogglesToSync[DisableOverheadLightAndShadows] = new List<Toggle>(){};}
            if (!SlidersToSync.Keys.Contains(ScreenShakeStrength)){ SlidersToSync[ScreenShakeStrength] = new List<Slider>(){};}
            if (!SlidersToSync.Keys.Contains(ChromaticAberrationStrength)){ SlidersToSync[ChromaticAberrationStrength] = new List<Slider>(){};}
            if (!TogglesToSync.Keys.Contains(DisableOverheadLightShake)){ TogglesToSync[DisableOverheadLightShake] = new List<Toggle>(){};}
            if (!TogglesToSync.Keys.Contains(DisableBackgroundParticleAnimations)){ TogglesToSync[DisableBackgroundParticleAnimations] = new List<Toggle>(){};}
            if (!TogglesToSync.Keys.Contains(DisableForegroundParticleAnimations)){ TogglesToSync[DisableForegroundParticleAnimations] = new List<Toggle>(){};}
            if (!SlidersToSync.Keys.Contains(MaxNumberOfParticles)){ SlidersToSync[MaxNumberOfParticles] = new List<Slider>(){};}
            if (!TogglesToSync.Keys.Contains(FixProjectileObjectsToSpawn)){ TogglesToSync[FixProjectileObjectsToSpawn] = new List<Toggle>(){};}
            if (!TogglesToSync.Keys.Contains(FixBulletHitParticleEffects)) { TogglesToSync[FixBulletHitParticleEffects] = new List<Toggle>() { }; }
            if (!TogglesToSync.Keys.Contains(FixMapLoadLag)) { TogglesToSync[FixMapLoadLag] = new List<Toggle>() { }; }
            if (!TogglesToSync.Keys.Contains(DisableBulletHitSurfaceParticleEffects)){ TogglesToSync[DisableBulletHitSurfaceParticleEffects] = new List<Toggle>(){};}
            if (!TogglesToSync.Keys.Contains(DisableBulletHitBulletParticleEffects)){ TogglesToSync[DisableBulletHitBulletParticleEffects] = new List<Toggle>(){};}
            if (!SlidersToSync.Keys.Contains(MaximumBulletHitParticlesPerFrame)){ SlidersToSync[MaximumBulletHitParticlesPerFrame] = new List<Slider>(){};}
            if (!TogglesToSync.Keys.Contains(RemoveOutOfBoundsBullets)){ TogglesToSync[RemoveOutOfBoundsBullets] = new List<Toggle>(){};}
        }
        private static void SyncOptionsMenus(int recurse = 3)
        {
            foreach (Toggle toggle in TogglesToSync[DisableCardParticleAnimations]) { toggle.isOn = DisableCardParticleAnimations.Value; }
            foreach (Slider slider in SlidersToSync[NumberOfGeneralParticles]){ slider.value = NumberOfGeneralParticles.Value; }
            foreach (Toggle toggle in TogglesToSync[DisablePlayerParticles]){ toggle.isOn = DisablePlayerParticles.Value; }
            foreach (Toggle toggle in TogglesToSync[DisableMapParticles]){ toggle.isOn = DisableMapParticles.Value; }
            foreach (Toggle toggle in TogglesToSync[DisableBackgroundParticles]){ toggle.isOn = DisableBackgroundParticles.Value; }
            foreach (Toggle toggle in TogglesToSync[DisableOverheadLightAndShadows]){ toggle.isOn = DisableOverheadLightAndShadows.Value; }
            foreach (Slider slider in SlidersToSync[ScreenShakeStrength]){ slider.value = ScreenShakeStrength.Value; }
            foreach (Slider slider in SlidersToSync[ChromaticAberrationStrength]){ slider.value = ChromaticAberrationStrength.Value; }
            foreach (Toggle toggle in TogglesToSync[DisableOverheadLightShake]){ toggle.isOn = DisableOverheadLightShake.Value; }
            foreach (Toggle toggle in TogglesToSync[DisableBackgroundParticleAnimations]){ toggle.isOn = DisableBackgroundParticleAnimations.Value; }
            foreach (Toggle toggle in TogglesToSync[DisableForegroundParticleAnimations]){ toggle.isOn = DisableForegroundParticleAnimations.Value; }
            foreach (Slider slider in SlidersToSync[MaxNumberOfParticles]){ slider.value = MaxNumberOfParticles.Value; }
            foreach (Toggle toggle in TogglesToSync[FixProjectileObjectsToSpawn]){ toggle.isOn = FixProjectileObjectsToSpawn.Value; }
            foreach (Toggle toggle in TogglesToSync[FixBulletHitParticleEffects]) { toggle.isOn = FixBulletHitParticleEffects.Value; }
            foreach (Toggle toggle in TogglesToSync[FixMapLoadLag]) { toggle.isOn = FixMapLoadLag.Value; }
            foreach (Toggle toggle in TogglesToSync[DisableBulletHitSurfaceParticleEffects]){ toggle.isOn = DisableBulletHitSurfaceParticleEffects.Value; }
            foreach (Toggle toggle in TogglesToSync[DisableBulletHitBulletParticleEffects]){ toggle.isOn = DisableBulletHitBulletParticleEffects.Value; }
            foreach (Slider slider in SlidersToSync[MaximumBulletHitParticlesPerFrame]){ slider.value = MaximumBulletHitParticlesPerFrame.Value; }
            foreach (Toggle toggle in TogglesToSync[RemoveOutOfBoundsBullets]){ toggle.isOn = RemoveOutOfBoundsBullets.Value; }
            if (recurse > 0) { SyncOptionsMenus(recurse-1); }
        }

        private static void NewGUI(GameObject menu)
        {
            InitializeOptionsDictionaries();

            MenuHandler.CreateText(ModName + " Options", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            GameObject particleMenu = MenuHandler.CreateMenu("Particle Effects", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            ParticleOptionsMenu(particleMenu);
            GameObject fixMenu = MenuHandler.CreateMenu("Bug Fixes", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            FixOptionsMenu(fixMenu);
            GameObject bulletMenu = MenuHandler.CreateMenu("Bullet Effects", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            BulletEffectsOptionsMenu(bulletMenu);
            GameObject miscMenu = MenuHandler.CreateMenu("Miscellaneous", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            MiscellaneousOptionsMenu(miscMenu);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            GameObject presetMenu = MenuHandler.CreateMenu("Presets", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            PresetsMenu(presetMenu);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            GameObject helpMenu = MenuHandler.CreateMenu("Help", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            GeneralHelpMenu(helpMenu);


        }
        private static void PresetsMenu(GameObject menu)
        {
            MenuHandler.CreateButton("Vanilla", menu, VanillaPreset, 60, color: easyChangeColor);
            MenuHandler.CreateButton("Only Bug Fixes", menu, OnlyBugFixPreset, 60, color: easyChangeColor);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateButton("Better Performance", menu, BetterPerformancePreset, 60, color: easyChangeColor);
            MenuHandler.CreateButton("High Performance", menu, HighPerformancePreset, 60);
            MenuHandler.CreateButton("Maximum Performance", menu, MaxPerformancePreset, 60, color: hardChangeColor);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
        }
        private static void GeneralHelpMenu(GameObject menu)
        {
            MenuHandler.CreateText("Options in <b>green</b> cause effectively no appearance changes.", menu, out TextMeshProUGUI _, 45, false, color: easyChangeColor);
            MenuHandler.CreateText("Options in <b>white</b> cause minor appearance changes.", menu, out TextMeshProUGUI _, 45, false);
            MenuHandler.CreateText("Options in <b>red</b> can cause major appearance changes.", menu, out TextMeshProUGUI _, 45, false, color: hardChangeColor);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateText("<b>All options can result in significant performance and stability improvements, <u>regardless of their highlight color.</u></b>", menu, out TextMeshProUGUI _, 45, false);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateText("You can find curated settings in the \"<b>PRESETS</b>\" menu.", menu, out TextMeshProUGUI _, 45, false);
        }

        private static void ParticleOptionsMenu(GameObject menu)
        {
            MenuHandler.CreateText("Particle Effects Options", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            void MaxParticlesChanged(float val)
            {
                MaxNumberOfParticles.Value = (int)val;
                CycleArt();
                SyncOptionsMenus();
            }
            MenuHandler.CreateSlider("Maximum number of particles", menu, 30, 20f, 1000f, MaxNumberOfParticles.Value, MaxParticlesChanged, out Slider slider1, true, color: easyChangeColor);
            SlidersToSync[MaxNumberOfParticles].Add(slider1);
            void NumPartsChanged(float val)
            {
                NumberOfGeneralParticles.Value = (int)val;
                SyncOptionsMenus();
            }
            MenuHandler.CreateSlider("Max number of particles for GeneralParticleSystems", menu, 30, 10f, 100f, NumberOfGeneralParticles.Value, NumPartsChanged, out Slider slider, true, color: easyChangeColor);
            SlidersToSync[NumberOfGeneralParticles].Add(slider);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            void CardPartAnimChanged(bool val)
            {
                DisableCardParticleAnimations.Value = val;
                SyncOptionsMenus();
            }
            TogglesToSync[DisableCardParticleAnimations].Add(MenuHandler.CreateToggle(DisableCardParticleAnimations.Value, "Disable Card Particle Animations", menu, CardPartAnimChanged, 30).GetComponent<Toggle>());
            void BackPartAnimChanged(bool val)
            {
                DisableBackgroundParticleAnimations.Value = val;
                CycleArt();
                SyncOptionsMenus();
            }
            TogglesToSync[DisableBackgroundParticleAnimations].Add(MenuHandler.CreateToggle(DisableBackgroundParticleAnimations.Value, "Disable Background Particle Animations", menu, BackPartAnimChanged, 30).GetComponent<Toggle>());
            void ForePartAnimChanged(bool val)
            {
                DisableForegroundParticleAnimations.Value = val;
                CycleArt();
                SyncOptionsMenus();
            }
            TogglesToSync[DisableForegroundParticleAnimations].Add(MenuHandler.CreateToggle(DisableForegroundParticleAnimations.Value, "Disable Foreground Particle Animations", menu, ForePartAnimChanged, 30).GetComponent<Toggle>());
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            void SimpleSkinChanged(bool val)
            {
                DisablePlayerParticles.Value = val;
                CycleArt();
                SyncOptionsMenus();
            }
            TogglesToSync[DisablePlayerParticles].Add(MenuHandler.CreateToggle(DisablePlayerParticles.Value, "Use simple player skins", menu, SimpleSkinChanged, 30, color: hardChangeColor).GetComponent<Toggle>());

            void BackParticlesChanged(bool val)
            {
                DisableBackgroundParticles.Value = val;
                CycleArt();
                SyncOptionsMenus();
            }
            TogglesToSync[DisableBackgroundParticles].Add(MenuHandler.CreateToggle(DisableBackgroundParticles.Value, "Disable Background Particle Effects", menu, BackParticlesChanged, 30, color: hardChangeColor).GetComponent<Toggle>());
            void MapParticlesChanged(bool val)
            {
                DisableMapParticles.Value = val;
                CycleArt();
                SyncOptionsMenus();
            }
            TogglesToSync[DisableMapParticles].Add(MenuHandler.CreateToggle(DisableMapParticles.Value, "Disable Map Particle Effects", menu, MapParticlesChanged, 30, color: hardChangeColor).GetComponent<Toggle>());

            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            GameObject helpMenu = MenuHandler.CreateMenu("Help", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            ParticleOptionsHelp(helpMenu);
        }
        private static void ParticleOptionsHelp(GameObject menu)
        {
            MenuHandler.CreateText("<size=150%>MAXIMUM NUMBER OF PARTICLES:<size=100%>\nThe maximum number of particles per particle system. Can be set as low as about 300 without perceivable visual differences. Major performance impact.", menu, out TextMeshProUGUI _, 30, false, color: easyChangeColor, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>MAXIMUM NUMBER OF PARTICLES FOR GENERALPARTICLESYSTEMS:<size=100%>\nThe maximum number of particles per particle system specifically for card backgrounds and UI elements. Can be set as low as about 20 without perceivable visual differences. Medium performance impact, especially during pick phases.", menu, out TextMeshProUGUI _, 30, false, color: easyChangeColor, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>DISABLE CARD PARTICLE ANIMATIONS:<size=100%>\nReplace the animated card backgrounds with static sprites. Extreme performance impact during pick phase.", menu, out TextMeshProUGUI _, 30, false, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>DISABLE BACKGROUND PARTICLE ANIMATIONS:<size=100%>\nReplace the animated background with static sprites. Some performance improvement on low-end hardware.", menu, out TextMeshProUGUI _, 30, false, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>DISABLE FOREGROUND PARTICLE ANIMATIONS:<size=100%>\nReplace the animated map textures with static sprites. Some performance improvement on low-end hardware.", menu, out TextMeshProUGUI _, 30, false, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>USE SIMPLE PLAYER SKINS:<size=100%>\nReplace the particle effects for player skins with a static sprite. Medium performance improvement on most hardware.", menu, out TextMeshProUGUI _, 30, false, color: hardChangeColor, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>DISABLE BACKGROUND PARTICLE EFFECTS:<size=100%>\nCompletely remove the background particle effects. Noticeable performance improvement on low-end hardware.", menu, out TextMeshProUGUI _, 30, false, color: hardChangeColor, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>DISABLE FOREGROUND PARTICLE EFFECTS:<size=100%>\nCompletely remove the map particle effects. Noticeable performance improvement on low-end hardware.", menu, out TextMeshProUGUI _, 30, false, color: hardChangeColor, alignmentOptions: TextAlignmentOptions.Left);

        }
        private static void FixOptionsMenu(GameObject menu)
        {
            MenuHandler.CreateText("Bug Fixes Options", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            void FixObjToSpawnChanged(bool val)
            {
                FixProjectileObjectsToSpawn.Value = val;
                SyncOptionsMenus();
            }
            TogglesToSync[FixProjectileObjectsToSpawn].Add(MenuHandler.CreateToggle(FixProjectileObjectsToSpawn.Value, "Fix persistance issues\nwith Projectile.ObjectsToSpawn", menu, FixObjToSpawnChanged, 30, color: easyChangeColor).GetComponent<Toggle>());
            void FixBulletHitChanged(bool val)
            {
                FixBulletHitParticleEffects.Value = val;
                SyncOptionsMenus();
            }
            TogglesToSync[FixBulletHitParticleEffects].Add(MenuHandler.CreateToggle(FixBulletHitParticleEffects.Value, "Fix persistance issues\nwith BulletHit particle effects", menu, FixBulletHitChanged, 30, color: easyChangeColor).GetComponent<Toggle>());
            void FixMapLagChanged(bool val)
            {
                FixMapLoadLag.Value = val;
                SyncOptionsMenus();
            }
            TogglesToSync[FixMapLoadLag].Add(MenuHandler.CreateToggle(FixMapLoadLag.Value, "Reduce lag spikes when maps load", menu, FixMapLagChanged, 30, color: easyChangeColor).GetComponent<Toggle>());

            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            GameObject helpMenu = MenuHandler.CreateMenu("Help", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            FixOptionsHelp(helpMenu);
        }
        private static void FixOptionsHelp(GameObject menu)
        {
            MenuHandler.CreateText("<size=150%>FIX PERSISTANCE ISSUES WITH PROJECTILE.OBJECTSTOSPAWN:<size=100%>\nFix bug in the vanilla game where objects spawned from bullets would fail to despawn at the end of the round. Major performance and stability impact.", menu, out TextMeshProUGUI _, 30, false, color: easyChangeColor, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>FIX PERSISTANCE ISSUES WITH BULLETHIT PARTICLE EFFECTS:<size=100%>\nFix bug in the vanilla game where particle effects from bullets hitting the ground or other bullets would fail to despawn at the end of the round. Major performance and stability impact.", menu, out TextMeshProUGUI _, 30, false, color: easyChangeColor, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>REDUCE LAG SPIKES WHEN MAPS LOAD:<size=100%>\nFix an oversight in the vanilla game where dynamic objects can cause massive lag spikes related to screenshake and chromatic aberration when they load in. Major performance impact, especially on custom maps.", menu, out TextMeshProUGUI _, 30, false, color: easyChangeColor, alignmentOptions: TextAlignmentOptions.Left);

        }
        private static void BulletEffectsOptionsMenu(GameObject menu)
        {
            MenuHandler.CreateText("Bullet Effects Options", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            void MaxBulletHitChanged(float val)
            {
                MaximumBulletHitParticlesPerFrame.Value = (int)val;
                SyncOptionsMenus();
            }
            MenuHandler.CreateSlider("Global maximum number of bullethit particles per frame", menu, 30, 0f, 50f, (float)MaximumBulletHitParticlesPerFrame.Value, MaxBulletHitChanged, out Slider slider, true, color: easyChangeColor);
            SlidersToSync[MaximumBulletHitParticlesPerFrame].Add(slider);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            void DisableBulletHitChanged(bool val)
            {
                DisableBulletHitSurfaceParticleEffects.Value = val;
                SyncOptionsMenus();
            }
            TogglesToSync[DisableBulletHitSurfaceParticleEffects].Add(MenuHandler.CreateToggle(DisableBulletHitSurfaceParticleEffects.Value, "Disable BulletHitSurface particle effects", menu, DisableBulletHitChanged, 30, color: hardChangeColor).GetComponent<Toggle>());
            void DisableBulletHitBulletChanged(bool val)
            {
                DisableBulletHitBulletParticleEffects.Value = val;
                SyncOptionsMenus();
            }
            TogglesToSync[DisableBulletHitBulletParticleEffects].Add(MenuHandler.CreateToggle(DisableBulletHitBulletParticleEffects.Value, "Disable BulletHitBullet particle effects", menu, DisableBulletHitBulletChanged, 30, color: hardChangeColor).GetComponent<Toggle>());
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            GameObject helpMenu = MenuHandler.CreateMenu("Help", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            BulletEffectsOptionsHelp(helpMenu);
        }
        private static void BulletEffectsOptionsHelp(GameObject menu)
        {
            MenuHandler.CreateText("<size=150%>GLOBAL MAXIMUM NUMBER OF BULLETHIT PARTICLES PER FRAME:<size=100%>\nMaximum number of new particle effects per frame from bullets hitting the ground or other bullets. Can be set as low as 5 without perceivable visual differences. Extreme performance and stability impact.", menu, out TextMeshProUGUI _, 30, false, color: easyChangeColor, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>DISABLE BULLETHITSURFACE PARTICLE EFFECTS:<size=100%>\nCompletely remove the particle effects spawned when bullets hit the ground. Major performance and stability impact.", menu, out TextMeshProUGUI _, 30, false, color: hardChangeColor, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>DISABLE BULLETHITBULLET PARTICLE EFFECTS:<size=100%>\nCompletely remove the particle effects spawned when bullets hit other bullets. Major performance and stability impact.", menu, out TextMeshProUGUI _, 30, false, color: hardChangeColor, alignmentOptions: TextAlignmentOptions.Left);
        }

        private static void MiscellaneousOptionsMenu(GameObject menu)
        {
            MenuHandler.CreateText("Miscellaneous Options", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            
            void ShakeChanged(float val)
            {
                ScreenShakeStrength.Value = (int)val;
                SyncOptionsMenus();
            }
            void AberrationChanged(float val)
            {
                ChromaticAberrationStrength.Value = (int)val;
                SyncOptionsMenus();
            }

            MenuHandler.CreateSlider("Screen Shake Strength", menu, 30, 0f, 100f, (int)(ScreenShakeStrength.Value), ShakeChanged, out Slider slider, true);
            MenuHandler.CreateSlider("Chromatic Aberration Strength", menu, 30, 0f, 100f, (int)(ChromaticAberrationStrength.Value), AberrationChanged, out Slider slider1, true);
            SlidersToSync[ScreenShakeStrength].Add(slider);
            SlidersToSync[ChromaticAberrationStrength].Add(slider1);
            void LightShakeChanged(bool val)
            {
                DisableOverheadLightShake.Value = val;
                CycleArt();
                SyncOptionsMenus();
            }
            TogglesToSync[DisableOverheadLightShake].Add(MenuHandler.CreateToggle(DisableOverheadLightShake.Value, "Disable Overhead light shake", menu, LightShakeChanged, 30).GetComponent<Toggle>());

            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            void LightChanged(bool val)
            {
                DisableOverheadLightAndShadows.Value = val;
                CycleArt();
                SyncOptionsMenus();
            }
            TogglesToSync[DisableOverheadLightAndShadows].Add(MenuHandler.CreateToggle(DisableOverheadLightAndShadows.Value, "Disable Overhead light and shadows", menu, LightChanged, 30, color: hardChangeColor).GetComponent<Toggle>());
            void RemoveOOBChanged(bool val)
            {
                RemoveOutOfBoundsBullets.Value = val;
                SyncOptionsMenus();
            }
            TogglesToSync[RemoveOutOfBoundsBullets].Add(MenuHandler.CreateToggle(RemoveOutOfBoundsBullets.Value, "Remove out of bounds bullets", menu, RemoveOOBChanged, 30, color: hardChangeColor).GetComponent<Toggle>());
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            GameObject helpMenu = MenuHandler.CreateMenu("Help", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            MiscellaneousOptionsHelp(helpMenu);
        }
        private static void MiscellaneousOptionsHelp(GameObject menu)
        {
            MenuHandler.CreateText("<size=150%>SCREEN SHAKE STRENGTH:<size=100%>\nStrength of screen shake effects. 100 corresponds to the vanilla game and 0 disables screen shake entirely. Minor performance impact on low-end hardware.", menu, out TextMeshProUGUI _, 30, false, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>SCREEN SHAKE STRENGTH:<size=100%>\nStrength of chromatic aberration effects. 100 corresponds to the vanilla game and 0 disables chromatic aberration entirely. Minor performance impact on low-end hardware.", menu, out TextMeshProUGUI _, 30, false, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>DISABLE OVERHEAD LIGHT SHAKE:<size=100%>\nDisable the shaking of the overhead light and the associated movement of the shadows it causes. Some performance impact on low-end hardware.", menu, out TextMeshProUGUI _, 30, false, alignmentOptions: TextAlignmentOptions.Left);

            MenuHandler.CreateText("<size=150%>DISABLE OVERHEAD LIGHT AND SHADOWS:<size=100%>\nCompletely remove the overhead light and the shadows it causes. Some performance impact on low-end hardware.", menu, out TextMeshProUGUI _, 30, false, color: hardChangeColor, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>REMOVE OUT OF BOUNDS BULLETS:<size=100%>\nCompletely remove bullets that are sufficiently off the <u>sides or bottom</u> of the screen. Noticeable performance and stability impact. However, in extremely specific and rare scenarios, it is possible for this setting to cause gameplay issues. For example, if a bullet with negative gravity and very high speed was shot straight down off the map.", menu, out TextMeshProUGUI _, 30, false, color: hardChangeColor, alignmentOptions: TextAlignmentOptions.Left);
        }

        private static void CycleArt()
        {
            ArtHandler.instance.NextArt();
            ArtHandler.instance.NextArt();
        }

        private void Screenshaker_OnGameFeel(On.Screenshaker.orig_OnGameFeel orig, global::Screenshaker self, Vector2 feelDirection)
        {
            orig(self, feelDirection * (float)(PerformanceImprovements.instance.ScreenShakeValue)/100f);
        }

        private void ChomaticAberrationFeeler_OnGameFeel(On.ChomaticAberrationFeeler.orig_OnGameFeel orig, global::ChomaticAberrationFeeler self, Vector2 feelDirection)
        {
            orig(self, feelDirection * (float)(PerformanceImprovements.instance.AberrationValue)/100f);
        }

        internal IEnumerator MapTransitionScalePostFX(float transitionTime)
        {
            if (!PerformanceImprovements.FixMapLoadLag.Value) { yield break; }
            this.PostFXDampening = 0f;
            yield return new WaitForSecondsRealtime(transitionTime);
            yield return new WaitUntil(() => PerformanceImprovements.BattleInProgress);
            yield return new WaitForSecondsRealtime(PerformanceImprovements.mapTransitionExtraDelay);
            //float t = PerformanceImprovements.mapTransitionExtraDelay;
            float t = PerformanceImprovements.postFXRampDuration;
            while (t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                this.PostFXDampening = UnityEngine.Mathf.Lerp(1f, 0f, t / PerformanceImprovements.postFXRampDuration);
                yield return null;
            }
            this.PostFXDampening = 1f;
            yield break;
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
