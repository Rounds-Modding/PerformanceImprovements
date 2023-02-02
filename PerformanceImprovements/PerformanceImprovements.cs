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
    [BepInPlugin(ModId, ModName, "0.2.0")]
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

        internal static bool mapTransitionPatchInProgress = false;

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
        /// <summary>
        /// case-insensitive settings key
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static string ConfigKey(string name)
        {
            return $"{PerformanceImprovements.CompatibilityModName}_{name.ToLower()}";
        }

        internal static bool GetBool(string name, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(ConfigKey(name), defaultValue ? 1 : 0) == 1;
        }
        internal static void SetBool(string name, bool value)
        {
            PlayerPrefs.SetInt(ConfigKey(name), value ? 1 : 0);
        }
        internal static int GetInt(string name, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(ConfigKey(name), defaultValue);
        }
        internal static void SetInt(string name, int value)
        {
            PlayerPrefs.SetInt(ConfigKey(name), value);
        }
        internal static float GetFloat(string name, float defaultValue = 0)
        {
            return PlayerPrefs.GetFloat(ConfigKey(name), defaultValue);
        }
        internal static void SetFloat(string name, float value)
        {
            PlayerPrefs.SetFloat(ConfigKey(name), value);
        }

        public static bool AdaptivePerformance
        {
            get
            {
                return GetBool("AdaptivePerformance", false);
            }
            set
            {
                SetBool("AdaptivePerformance", value);
            }
        }
        public static bool AdaptiveOverrideScreenShake
        {
            get
            {
                return GetBool("AdaptiveOverrideScreenShake", false);
            }
            set
            {
                SetBool("AdaptiveOverrideScreenShake", value);
            }
        }
        public static bool AdaptiveOverrideChromaticAberration
        {
            get
            {
                return GetBool("AdaptiveOverrideChromaticAberration", false);
            }
            set
            {
                SetBool("AdaptiveOverrideChromaticAberration", value);
            }
        }
        public static bool DisableCardParticleAnimations
        {
            get
            {
                return GetBool("DisableCardParticleAnimations", false);
            }
            set
            {
                SetBool("DisableCardParticleAnimations", value);
            }
        }
        public static int NumberOfGeneralParticles
        {
            get
            {
                return GetInt("NumberOfGeneralParticles", 100);
            }
            set
            {
                SetInt("NumberOfGeneralParticles", value);
            }
        }
        public static bool DisablePlayerParticles
        {
            get
            {
                return GetBool("DisablePlayerParticles", false);
            }
            set
            {
                SetBool("DisablePlayerParticles", value);
            }
        }
        public static bool DisableMapParticles
        {
            get
            {
                return GetBool("DisableMapParticles", false);
            }
            set
            {
                SetBool("DisableMapParticles", value);
            }
        }
        public static bool DisableBackgroundParticles
        {
            get
            {
                return GetBool("DisableBackgroundParticles", false);
            }
            set
            {
                SetBool("DisableBackgroundParticles", value);
            }
        }
        public static bool DisableOverheadLightAndShadows
        {
            get
            {
                return GetBool("DisableOverheadLightAndShadows", false);
            }
            set
            {
                SetBool("DisableOverheadLightAndShadows", value);
            }
        }
        public static int ScreenShakeStrength
        {
            get
            {
                return GetInt("ScreenShakeStrength", 100);
            }
            set
            {
                SetInt("ScreenShakeStrength", value);
            }
        }
        public static int ChromaticAberrationStrength
        {
            get
            {
                return GetInt("ChromaticAberrationStrength", 100);
            }
            set
            {
                SetInt("ChromaticAberrationStrength", value);
            }
        }
        public static bool DisableOverheadLightShake
        {
            get
            {
                return GetBool("DisableOverheadLightShake", false);
            }
            set
            {
                SetBool("DisableOverheadLightShake", value);
            }
        }
        public static bool DisableBackgroundParticleAnimations
        {
            get
            {
                return GetBool("DisableBackgroundParticleAnimations", false);
            }
            set
            {
                SetBool("DisableBackgroundParticleAnimations", value);
            }
        }
        public static bool DisableForegroundParticleAnimations
        {
            get
            {
                return GetBool("DisableForegroundParticleAnimations", false);
            }
            set
            {
                SetBool("DisableForegroundParticleAnimations", value);
            }
        }
        public static int MaxNumberOfParticles
        {
            get
            {
                return GetInt("MaxNumberOfParticles", 300);
            }
            set
            {
                SetInt("MaxNumberOfParticles", value);
            }
        }
        public static bool FixProjectileObjectsToSpawn
        {
            get
            {
                return GetBool("FixProjectileObjectsToSpawn", true);
            }
            set
            {
                SetBool("FixProjectileObjectsToSpawn", value);
            }
        }
        public static bool FixBulletHitParticleEffects
        {
            get
            {
                return GetBool("FixBulletHitParticleEffects", true);
            }
            set
            {
                SetBool("FixBulletHitParticleEffects", value);
            }
        }
        public static bool FixMapLoadLag
        {
            get
            {
                return GetBool("FixMapLoadLag", true);
            }
            set
            {
                SetBool("FixMapLoadLag", value);
            }
        }
        public static bool FixStunPlayer
        {
            get
            {
                return GetBool("FixStunPlayer", true);
            }
            set
            {
                SetBool("FixStunPlayer", value);
            }
        }
        public static bool FixControllerLag
        {
            get
            {
                return GetBool("FixControllerLag", true);
            }
            set
            {
                SetBool("FixControllerLag", value);
            }
        }
        public static bool DisableBulletHitSurfaceParticleEffects
        {
            get
            {
                return GetBool("DisableBulletHitSurfaceParticleEffects", false);
            }
            set
            {
                SetBool("DisableBulletHitSurfaceParticleEffects", value);
            }
        }
        public static bool DisableBulletHitBulletParticleEffects
        {
            get
            {
                return GetBool("DisableBulletHitBulletParticleEffects", false);
            }
            set
            {
                SetBool("DisableBulletHitBulletParticleEffects", value);
            }
        }
        public static int MaximumBulletHitParticlesPerFrame
        {
            get
            {
                return GetInt("MaximumBulletHitParticlesPerFrame", 5);
            }
            set
            {
                SetInt("MaximumBulletHitParticlesPerFrame", value);
            }
        }
        public static bool RemoveOutOfBoundsBullets
        {
            get
            {
                return GetBool("RemoveOutOfBoundsBullets", false);
            }
            set
            {
                SetBool("RemoveOutOfBoundsBullets", value);
            }
        }

        internal static Dictionary<string, List<Toggle>> TogglesToSync = new Dictionary<string, List<Toggle>>();
        internal static Dictionary<string, List<Slider>> SlidersToSync = new Dictionary<string, List<Slider>>();
        internal static List<GameObject> adaptiveEnabledWarnings = new List<GameObject>();
        internal static List<float> frameTimeHistory = new List<float>();
        internal float lastAdaptiveUpdateTime = 0f, adaptiveUpdateInterval = 3f, framesToAverage = 240f;

        public static PerformanceImprovements instance;
        
        private enum AdaptivePresetLevel
        {
            NONE, BETTER, HIGH, MAX
        }
        private AdaptivePresetLevel currentAdaptiveLevel = AdaptivePresetLevel.NONE;

        private PerformanceImprovements()
        {
            instance = this;
        }
        private float ScreenShakeValue
        {
            get
            {
                return PerformanceImprovements.ScreenShakeStrength * this.PostFXDampening;
            }
            set { }
        }
        private float AberrationValue
        {
            get
            {
                return PerformanceImprovements.ChromaticAberrationStrength * this.PostFXDampening;
            }
            set { }
        }

        private void Awake()
        {
            // apply patches
            new Harmony(ModId).PatchAll();
        }
        private void Start()
        {
            // load assets
            //PerformanceImprovements.Assets = AssetUtils.LoadAssetBundleFromResources("performance", typeof(PerformanceImprovements).Assembly);

            // add credits
            Unbound.RegisterCredits(ModName, new string[] { "Pykess", "Ascyst (Original RemovePostFX mod)" }, new string[] { "github", "Support Pykess", "Support Ascyst" }, new string[] { "https://github.com/Rounds-Modding/PerformanceImprovements", "https://ko-fi.com/pykess", "https://www.buymeacoffee.com/Ascyst" });

            // add GUI to modoptions menu
            Unbound.RegisterMenu(ModName, () => { }, NewGUI, null, false);

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

        void Update()
        {
            if (!AdaptivePerformance) return;

            // track frame times
            frameTimeHistory.Add(Time.deltaTime);
            while (frameTimeHistory.Count > framesToAverage)
            {
                frameTimeHistory.RemoveAt(0);
            }

            // calculate fps
            var fps = frameTimeHistory.Count / frameTimeHistory.Sum();

            // set adaptive performance preset
            if (fps > 50)
            {
                UnityEngine.Debug.Log("\n[PerformanceImprovements] Detected high FPS");
                SetAdaptiveLevel(AdaptivePresetLevel.NONE);
            }
            else if (fps > 30)
            {
                UnityEngine.Debug.Log("\n[PerformanceImprovements] Detected FPS fell below 50");
                SetAdaptiveLevel(AdaptivePresetLevel.BETTER);
            }
            else if (fps > 20)
            {
                UnityEngine.Debug.Log("\n[PerformanceImprovements] Detected FPS fell below 30");
                SetAdaptiveLevel(AdaptivePresetLevel.HIGH);
            }
            else
            {
                UnityEngine.Debug.Log("\n[PerformanceImprovements] Detected FPS fell below 20");
                SetAdaptiveLevel(AdaptivePresetLevel.MAX);
            }
        }

        private void SetAdaptiveLevel(AdaptivePresetLevel targetLevel)
        {
            if (currentAdaptiveLevel == targetLevel || Time.time - lastAdaptiveUpdateTime < adaptiveUpdateInterval) return;

            switch (targetLevel)
            {
                case AdaptivePresetLevel.NONE:
                    OnlyBugFixPreset(AdaptiveOverrideScreenShake, AdaptiveOverrideChromaticAberration);
                    break;
                case AdaptivePresetLevel.BETTER:
                    BetterPerformancePreset(AdaptiveOverrideScreenShake, AdaptiveOverrideChromaticAberration);
                    break;
                case AdaptivePresetLevel.HIGH:
                    HighPerformancePreset(AdaptiveOverrideScreenShake, AdaptiveOverrideChromaticAberration);
                    break;
                case AdaptivePresetLevel.MAX:
                    MaxPerformancePreset(AdaptiveOverrideScreenShake, AdaptiveOverrideChromaticAberration);
                    break;
            }

            UnityEngine.Debug.Log(
                $"[PerformanceImprovements] Auto-set preset to {targetLevel}\n" +
                $"[PerformanceImprovements] Override Screen Shake: {AdaptiveOverrideScreenShake}\n" +
                $"[PerformanceImprovements] Override Chromatic Aberration: {AdaptiveOverrideChromaticAberration}\n");

            currentAdaptiveLevel = targetLevel;
            lastAdaptiveUpdateTime = Time.time;
        }

        private static void CreateAdaptiveActiveWarning(GameObject menu)
        {
            MenuHandler.CreateText("Adaptive Performance is enabled! Manual changes may not be respected", menu, out TextMeshProUGUI warning, 45, false, color: hardChangeColor);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            adaptiveEnabledWarnings.Add(warning.gameObject);
        }

        private static void ToggleAdaptiveWarningEnabled()
        {
            var enabled = AdaptivePerformance;
            adaptiveEnabledWarnings.RemoveAll(go => go == null);
            foreach (var warning in adaptiveEnabledWarnings)
            {
                warning.SetActive(enabled);
            }
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
            DisableCardParticleAnimations = false;
            NumberOfGeneralParticles = 100;
            DisablePlayerParticles = false;
            DisableMapParticles = false;
            DisableBackgroundParticles = false;
            DisableOverheadLightAndShadows = false;
            ScreenShakeStrength = 100;
            ChromaticAberrationStrength = 100;
            DisableOverheadLightShake = false;
            DisableBackgroundParticleAnimations = false;
            DisableForegroundParticleAnimations = false;
            MaxNumberOfParticles = 1000;
            FixProjectileObjectsToSpawn = false;
            FixBulletHitParticleEffects = false;
            FixMapLoadLag = false;
            FixStunPlayer = false;
            FixControllerLag = false;
            DisableBulletHitSurfaceParticleEffects = false;
            DisableBulletHitBulletParticleEffects = false;
            MaximumBulletHitParticlesPerFrame = 100;
            RemoveOutOfBoundsBullets = false;
            CycleArt();
            SyncOptionsMenus();
    }
        private static void OnlyBugFixPreset(bool overrideShake = true, bool overrideChromatic = true)
        {
            DisableCardParticleAnimations = false;
            NumberOfGeneralParticles = 100;
            DisablePlayerParticles = false;
            DisableMapParticles = false;
            DisableBackgroundParticles = false;
            DisableOverheadLightAndShadows = false;
            if (overrideShake)
            {
                ScreenShakeStrength = 100;
            }
            if (overrideChromatic)
            {
                ChromaticAberrationStrength = 100;
            }
            DisableOverheadLightShake = false;
            DisableBackgroundParticleAnimations = false;
            DisableForegroundParticleAnimations = false;
            MaxNumberOfParticles = 1000;
            FixProjectileObjectsToSpawn = true;
            FixBulletHitParticleEffects = true;
            FixMapLoadLag = true;
            FixStunPlayer = true;
            FixControllerLag = true;
            DisableBulletHitSurfaceParticleEffects = false;
            DisableBulletHitBulletParticleEffects = false;
            MaximumBulletHitParticlesPerFrame = 100;
            RemoveOutOfBoundsBullets = false;
            CycleArt();
            SyncOptionsMenus();
        }
        private static void BetterPerformancePreset(bool overrideShake = true, bool overrideChromatic = true)
        {
            DisableCardParticleAnimations = false;
            NumberOfGeneralParticles = 20;
            DisablePlayerParticles = false;
            DisableMapParticles = false;
            DisableBackgroundParticles = false;
            DisableOverheadLightAndShadows = false;
            if (overrideShake)
            {
                ScreenShakeStrength = 100;
            }
            if (overrideChromatic)
            {
                ChromaticAberrationStrength = 100;
            }
            DisableOverheadLightShake = false;
            DisableBackgroundParticleAnimations = false;
            DisableForegroundParticleAnimations = false;
            MaxNumberOfParticles = 500;
            FixProjectileObjectsToSpawn = true;
            FixBulletHitParticleEffects = true;
            FixMapLoadLag = true;
            FixStunPlayer = true;
            FixControllerLag = true;
            DisableBulletHitSurfaceParticleEffects = false;
            DisableBulletHitBulletParticleEffects = false;
            MaximumBulletHitParticlesPerFrame = 10;
            RemoveOutOfBoundsBullets = false;
            CycleArt();
            SyncOptionsMenus();
        }
        private static void HighPerformancePreset(bool overrideShake = true, bool overrideChromatic = true)
        {
            DisableCardParticleAnimations = true;
            NumberOfGeneralParticles = 20;
            DisablePlayerParticles = false;
            DisableMapParticles = false;
            DisableBackgroundParticles = false;
            DisableOverheadLightAndShadows = false;
            if (overrideShake)
            {
                ScreenShakeStrength = 50;
            }
            if (overrideChromatic)
            {
                ChromaticAberrationStrength = 50;
            }
            DisableOverheadLightShake = false;
            DisableBackgroundParticleAnimations = true;
            DisableForegroundParticleAnimations = true;
            MaxNumberOfParticles = 300;
            FixProjectileObjectsToSpawn = true;
            FixBulletHitParticleEffects = true;
            FixMapLoadLag = true;
            FixStunPlayer = true;
            FixControllerLag = true;
            DisableBulletHitSurfaceParticleEffects = false;
            DisableBulletHitBulletParticleEffects = false;
            MaximumBulletHitParticlesPerFrame = 5;
            RemoveOutOfBoundsBullets = false;
            CycleArt();
            SyncOptionsMenus();
        }
        private static void MaxPerformancePreset(bool overrideShake = true, bool overrideChromatic = true)
        {
            DisableCardParticleAnimations = true;
            NumberOfGeneralParticles = 10;
            DisablePlayerParticles = true;
            DisableMapParticles = true;
            DisableBackgroundParticles = true;
            DisableOverheadLightAndShadows = true;
            if (overrideShake)
            {
                ScreenShakeStrength = 0;
            }
            if (overrideChromatic)
            {
                ChromaticAberrationStrength = 0;
            }
            DisableOverheadLightShake = true;
            DisableBackgroundParticleAnimations = true;
            DisableForegroundParticleAnimations = true;
            MaxNumberOfParticles = 100;
            FixProjectileObjectsToSpawn = true;
            FixBulletHitParticleEffects = true;
            FixMapLoadLag = true;
            FixStunPlayer = true;
            FixControllerLag = true;
            DisableBulletHitSurfaceParticleEffects = true;
            DisableBulletHitBulletParticleEffects = true;
            MaximumBulletHitParticlesPerFrame = 3;
            RemoveOutOfBoundsBullets = true;
            CycleArt();
            SyncOptionsMenus();
        }

        private static void InitializeOptionsDictionaries()
        {
            if (!TogglesToSync.Keys.Contains("DisableCardParticleAnimations")) { TogglesToSync["DisableCardParticleAnimations"] = new List<Toggle>() { }; }
            if (!SlidersToSync.Keys.Contains("NumberOfGeneralParticles")){ SlidersToSync["NumberOfGeneralParticles"] = new List<Slider>(){};}
            if (!TogglesToSync.Keys.Contains("DisablePlayerParticles")){ TogglesToSync["DisablePlayerParticles"] = new List<Toggle>(){};}
            if (!TogglesToSync.Keys.Contains("DisableMapParticles")){ TogglesToSync["DisableMapParticles"] = new List<Toggle>(){};}
            if (!TogglesToSync.Keys.Contains("DisableBackgroundParticles")){ TogglesToSync["DisableBackgroundParticles"] = new List<Toggle>(){};}
            if (!TogglesToSync.Keys.Contains("DisableOverheadLightAndShadows")){ TogglesToSync["DisableOverheadLightAndShadows"] = new List<Toggle>(){};}
            if (!SlidersToSync.Keys.Contains("ScreenShakeStrength")){ SlidersToSync["ScreenShakeStrength"] = new List<Slider>(){};}
            if (!SlidersToSync.Keys.Contains("ChromaticAberrationStrength")){ SlidersToSync["ChromaticAberrationStrength"] = new List<Slider>(){};}
            if (!TogglesToSync.Keys.Contains("DisableOverheadLightShake")){ TogglesToSync["DisableOverheadLightShake"] = new List<Toggle>(){};}
            if (!TogglesToSync.Keys.Contains("DisableBackgroundParticleAnimations")){ TogglesToSync["DisableBackgroundParticleAnimations"] = new List<Toggle>(){};}
            if (!TogglesToSync.Keys.Contains("DisableForegroundParticleAnimations")){ TogglesToSync["DisableForegroundParticleAnimations"] = new List<Toggle>(){};}
            if (!SlidersToSync.Keys.Contains("MaxNumberOfParticles")){ SlidersToSync["MaxNumberOfParticles"] = new List<Slider>(){};}
            if (!TogglesToSync.Keys.Contains("FixProjectileObjectsToSpawn")){ TogglesToSync["FixProjectileObjectsToSpawn"] = new List<Toggle>(){};}
            if (!TogglesToSync.Keys.Contains("FixBulletHitParticleEffects")) { TogglesToSync["FixBulletHitParticleEffects"] = new List<Toggle>() { }; }
            if (!TogglesToSync.Keys.Contains("FixMapLoadLag")) { TogglesToSync["FixMapLoadLag"] = new List<Toggle>() { }; }
            if (!TogglesToSync.Keys.Contains("FixStunPlayer")) { TogglesToSync["FixStunPlayer"] = new List<Toggle>() { }; }
            if (!TogglesToSync.Keys.Contains("FixControllerLag")) { TogglesToSync["FixControllerLag"] = new List<Toggle>() { }; }
            if (!TogglesToSync.Keys.Contains("DisableBulletHitSurfaceParticleEffects")){ TogglesToSync["DisableBulletHitSurfaceParticleEffects"] = new List<Toggle>(){};}
            if (!TogglesToSync.Keys.Contains("DisableBulletHitBulletParticleEffects")){ TogglesToSync["DisableBulletHitBulletParticleEffects"] = new List<Toggle>(){};}
            if (!SlidersToSync.Keys.Contains("MaximumBulletHitParticlesPerFrame")){ SlidersToSync["MaximumBulletHitParticlesPerFrame"] = new List<Slider>(){};}
            if (!TogglesToSync.Keys.Contains("RemoveOutOfBoundsBullets")){ TogglesToSync["RemoveOutOfBoundsBullets"] = new List<Toggle>(){};}
        }
        private static void SyncOptionsMenus(int recurse = 3)
        {
            foreach (Toggle toggle in TogglesToSync["DisableCardParticleAnimations"]) { toggle.isOn = DisableCardParticleAnimations; }
            foreach (Slider slider in SlidersToSync["NumberOfGeneralParticles"]){ slider.value = NumberOfGeneralParticles; }
            foreach (Toggle toggle in TogglesToSync["DisablePlayerParticles"]){ toggle.isOn = DisablePlayerParticles; }
            foreach (Toggle toggle in TogglesToSync["DisableMapParticles"]){ toggle.isOn = DisableMapParticles; }
            foreach (Toggle toggle in TogglesToSync["DisableBackgroundParticles"]){ toggle.isOn = DisableBackgroundParticles; }
            foreach (Toggle toggle in TogglesToSync["DisableOverheadLightAndShadows"]){ toggle.isOn = DisableOverheadLightAndShadows; }
            foreach (Slider slider in SlidersToSync["ScreenShakeStrength"]){ slider.value = ScreenShakeStrength; }
            foreach (Slider slider in SlidersToSync["ChromaticAberrationStrength"]){ slider.value = ChromaticAberrationStrength; }
            foreach (Toggle toggle in TogglesToSync["DisableOverheadLightShake"]){ toggle.isOn = DisableOverheadLightShake; }
            foreach (Toggle toggle in TogglesToSync["DisableBackgroundParticleAnimations"]){ toggle.isOn = DisableBackgroundParticleAnimations; }
            foreach (Toggle toggle in TogglesToSync["DisableForegroundParticleAnimations"]){ toggle.isOn = DisableForegroundParticleAnimations; }
            foreach (Slider slider in SlidersToSync["MaxNumberOfParticles"]){ slider.value = MaxNumberOfParticles; }
            foreach (Toggle toggle in TogglesToSync["FixProjectileObjectsToSpawn"]){ toggle.isOn = FixProjectileObjectsToSpawn; }
            foreach (Toggle toggle in TogglesToSync["FixBulletHitParticleEffects"]) { toggle.isOn = FixBulletHitParticleEffects; }
            foreach (Toggle toggle in TogglesToSync["FixMapLoadLag"]) { toggle.isOn = FixMapLoadLag; }
            foreach (Toggle toggle in TogglesToSync["FixStunPlayer"]) { toggle.isOn = FixStunPlayer; }
            foreach (Toggle toggle in TogglesToSync["FixControllerLag"]) { toggle.isOn = FixControllerLag; }
            foreach (Toggle toggle in TogglesToSync["DisableBulletHitSurfaceParticleEffects"]){ toggle.isOn = DisableBulletHitSurfaceParticleEffects; }
            foreach (Toggle toggle in TogglesToSync["DisableBulletHitBulletParticleEffects"]){ toggle.isOn = DisableBulletHitBulletParticleEffects; }
            foreach (Slider slider in SlidersToSync["MaximumBulletHitParticlesPerFrame"]){ slider.value = MaximumBulletHitParticlesPerFrame; }
            foreach (Toggle toggle in TogglesToSync["RemoveOutOfBoundsBullets"]){ toggle.isOn = RemoveOutOfBoundsBullets; }
            if (recurse > 0) { SyncOptionsMenus(recurse-1); }
        }

        private static void NewGUI(GameObject menu)
        {
            InitializeOptionsDictionaries();

            MenuHandler.CreateText(ModName + " Options", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            GameObject adaptiveMenu = MenuHandler.CreateMenu("Adaptive Mode", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            AdaptiveMenu(adaptiveMenu);
            GameObject particleMenu = MenuHandler.CreateMenu("Particle Effects", ToggleAdaptiveWarningEnabled, menu, 60, true, true, menu.transform.parent.gameObject);
            ParticleOptionsMenu(particleMenu);
            GameObject fixMenu = MenuHandler.CreateMenu("Bug Fixes", ToggleAdaptiveWarningEnabled, menu, 60, true, true, menu.transform.parent.gameObject);
            FixOptionsMenu(fixMenu);
            GameObject bulletMenu = MenuHandler.CreateMenu("Bullet Effects", ToggleAdaptiveWarningEnabled, menu, 60, true, true, menu.transform.parent.gameObject);
            BulletEffectsOptionsMenu(bulletMenu);
            GameObject miscMenu = MenuHandler.CreateMenu("Miscellaneous", ToggleAdaptiveWarningEnabled, menu, 60, true, true, menu.transform.parent.gameObject);
            MiscellaneousOptionsMenu(miscMenu);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            GameObject presetMenu = MenuHandler.CreateMenu("Presets", ToggleAdaptiveWarningEnabled, menu, 60, true, true, menu.transform.parent.gameObject);
            PresetsMenu(presetMenu);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            GameObject helpMenu = MenuHandler.CreateMenu("Help", ToggleAdaptiveWarningEnabled, menu, 60, true, true, menu.transform.parent.gameObject);
            GeneralHelpMenu(helpMenu);
        }
        private static void AdaptiveMenu(GameObject menu)
        {
            MenuHandler.CreateText("Adaptive Performance Options", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateText("<b>Adaptive performance mode will monitor your FPS and automatically change your performance presets when it falls below the given thresholds.\nNOTE: While enabled manual changes to most settings will be overwritten!</b>", menu, out TextMeshProUGUI _, 45, false, color: hardChangeColor);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateToggle(AdaptivePerformance, "Adaptive Mode Active", menu, val => AdaptivePerformance = val, 60, color: hardChangeColor);
            MenuHandler.CreateToggle(AdaptiveOverrideScreenShake, "Prevent Overriding Screen Shake", menu, val => AdaptiveOverrideScreenShake = !val, 60, color: easyChangeColor);
            MenuHandler.CreateToggle(AdaptiveOverrideChromaticAberration, "Prevent Overriding Chromatic Aberration", menu, val => AdaptiveOverrideChromaticAberration = !val, 60, color: easyChangeColor);
        }
        private static void PresetsMenu(GameObject menu)
        {
            CreateAdaptiveActiveWarning(menu);

            MenuHandler.CreateButton("Vanilla", menu, VanillaPreset, 60, color: easyChangeColor);
            MenuHandler.CreateButton("Only Bug Fixes", menu, () => OnlyBugFixPreset(), 60, color: easyChangeColor);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateButton("Better Performance", menu, () => BetterPerformancePreset(), 60, color: easyChangeColor);
            MenuHandler.CreateButton("High Performance", menu, () => HighPerformancePreset(), 60);
            MenuHandler.CreateButton("Maximum Performance", menu, () => MaxPerformancePreset(), 60, color: hardChangeColor);
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
            CreateAdaptiveActiveWarning(menu);

            MenuHandler.CreateText("Particle Effects Options", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            void MaxParticlesChanged(float val)
            {
                MaxNumberOfParticles = (int)val;
                CycleArt();
                SyncOptionsMenus();
            }
            MenuHandler.CreateSlider("Maximum number of particles", menu, 30, 20f, 1000f, MaxNumberOfParticles, MaxParticlesChanged, out Slider slider1, true, color: easyChangeColor);
            SlidersToSync["MaxNumberOfParticles"].Add(slider1);
            void NumPartsChanged(float val)
            {
                NumberOfGeneralParticles = (int)val;
                SyncOptionsMenus();
            }
            MenuHandler.CreateSlider("Max number of particles for GeneralParticleSystems", menu, 30, 10f, 100f, NumberOfGeneralParticles, NumPartsChanged, out Slider slider, true, color: easyChangeColor);
            SlidersToSync["NumberOfGeneralParticles"].Add(slider);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            void CardPartAnimChanged(bool val)
            {
                DisableCardParticleAnimations = val;
                SyncOptionsMenus();
            }
            TogglesToSync["DisableCardParticleAnimations"].Add(MenuHandler.CreateToggle(DisableCardParticleAnimations, "Disable Card Particle Animations", menu, CardPartAnimChanged, 30).GetComponent<Toggle>());
            void BackPartAnimChanged(bool val)
            {
                DisableBackgroundParticleAnimations = val;
                CycleArt();
                SyncOptionsMenus();
            }
            TogglesToSync["DisableBackgroundParticleAnimations"].Add(MenuHandler.CreateToggle(DisableBackgroundParticleAnimations, "Disable Background Particle Animations", menu, BackPartAnimChanged, 30).GetComponent<Toggle>());
            void ForePartAnimChanged(bool val)
            {
                DisableForegroundParticleAnimations = val;
                CycleArt();
                SyncOptionsMenus();
            }
            TogglesToSync["DisableForegroundParticleAnimations"].Add(MenuHandler.CreateToggle(DisableForegroundParticleAnimations, "Disable Foreground Particle Animations", menu, ForePartAnimChanged, 30).GetComponent<Toggle>());
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            void SimpleSkinChanged(bool val)
            {
                DisablePlayerParticles = val;
                CycleArt();
                SyncOptionsMenus();
            }
            TogglesToSync["DisablePlayerParticles"].Add(MenuHandler.CreateToggle(DisablePlayerParticles, "Use simple player skins", menu, SimpleSkinChanged, 30, color: hardChangeColor).GetComponent<Toggle>());

            void BackParticlesChanged(bool val)
            {
                DisableBackgroundParticles = val;
                CycleArt();
                SyncOptionsMenus();
            }
            TogglesToSync["DisableBackgroundParticles"].Add(MenuHandler.CreateToggle(DisableBackgroundParticles, "Disable Background Particle Effects", menu, BackParticlesChanged, 30, color: hardChangeColor).GetComponent<Toggle>());
            void MapParticlesChanged(bool val)
            {
                DisableMapParticles = val;
                CycleArt();
                SyncOptionsMenus();
            }
            TogglesToSync["DisableMapParticles"].Add(MenuHandler.CreateToggle(DisableMapParticles, "Disable Map Particle Effects", menu, MapParticlesChanged, 30, color: hardChangeColor).GetComponent<Toggle>());

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
            CreateAdaptiveActiveWarning(menu);

            MenuHandler.CreateText("Bug Fixes Options", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            void FixObjToSpawnChanged(bool val)
            {
                FixProjectileObjectsToSpawn = val;
                SyncOptionsMenus();
            }
            TogglesToSync["FixProjectileObjectsToSpawn"].Add(MenuHandler.CreateToggle(FixProjectileObjectsToSpawn, "Fix persistance issues\nwith Projectile.ObjectsToSpawn", menu, FixObjToSpawnChanged, 30, color: easyChangeColor).GetComponent<Toggle>());
            void FixBulletHitChanged(bool val)
            {
                FixBulletHitParticleEffects = val;
                SyncOptionsMenus();
            }
            TogglesToSync["FixBulletHitParticleEffects"].Add(MenuHandler.CreateToggle(FixBulletHitParticleEffects, "Fix persistance issues\nwith BulletHit particle effects", menu, FixBulletHitChanged, 30, color: easyChangeColor).GetComponent<Toggle>());
            void FixMapLagChanged(bool val)
            {
                FixMapLoadLag = val;
                SyncOptionsMenus();
            }
            TogglesToSync["FixMapLoadLag"].Add(MenuHandler.CreateToggle(FixMapLoadLag, "Reduce lag spikes when maps load", menu, FixMapLagChanged, 30, color: easyChangeColor).GetComponent<Toggle>());
            void FixStunPlayerChanged(bool val)
            {
                FixStunPlayer = val;
                SyncOptionsMenus();
            }
            TogglesToSync["FixStunPlayer"].Add(MenuHandler.CreateToggle(FixStunPlayer, "Fix null reference exceptions from player stun effects", menu, FixStunPlayerChanged, 30, color: easyChangeColor).GetComponent<Toggle>());
            void FixControllerLagChanged(bool val)
            {
                FixControllerLag = val;
                SyncOptionsMenus();
            }
            TogglesToSync["FixControllerLag"].Add(MenuHandler.CreateToggle(FixControllerLag, "Fix heavy framerate stuttering during controller inputs", menu, FixControllerLagChanged, 30, color: easyChangeColor).GetComponent<Toggle>());

            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            GameObject helpMenu = MenuHandler.CreateMenu("Help", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            FixOptionsHelp(helpMenu);
        }
        private static void FixOptionsHelp(GameObject menu)
        {
            MenuHandler.CreateText("<size=150%>FIX PERSISTANCE ISSUES WITH PROJECTILE.OBJECTSTOSPAWN:<size=100%>\nFix bug in the vanilla game where objects spawned from bullets would fail to despawn at the end of the round. Major performance and stability impact.", menu, out TextMeshProUGUI _, 30, false, color: easyChangeColor, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>FIX PERSISTANCE ISSUES WITH BULLETHIT PARTICLE EFFECTS:<size=100%>\nFix bug in the vanilla game where particle effects from bullets hitting the ground or other bullets would fail to despawn at the end of the round. Major performance and stability impact.", menu, out TextMeshProUGUI _, 30, false, color: easyChangeColor, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>REDUCE LAG SPIKES WHEN MAPS LOAD:<size=100%>\nFix an oversight in the vanilla game where dynamic objects can cause massive lag spikes related to screenshake and chromatic aberration when they load in. Major performance impact, especially on custom maps.", menu, out TextMeshProUGUI _, 30, false, color: easyChangeColor, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>FIX NULL REFERENCE EXCEPTIONS FROM PLAYER STUN EFFECTS:<size=100%>\nFix bug in the vanilla game where the StunPlayer.Go method will throw an unhandled null reference exception. Minor performance impact.", menu, out TextMeshProUGUI _, 30, false, color: easyChangeColor, alignmentOptions: TextAlignmentOptions.Left);
            MenuHandler.CreateText("<size=150%>FIX HEAVY FRAMERATE STUTTERING DURING CONTROLLER INPUTS:<size=100%>\nFix bug in the vanilla game where controller inputs can cause extreme framerate stutter. Major performance impact.", menu, out TextMeshProUGUI _, 30, false, color: easyChangeColor, alignmentOptions: TextAlignmentOptions.Left);

        }
        private static void BulletEffectsOptionsMenu(GameObject menu)
        {
            CreateAdaptiveActiveWarning(menu);

            MenuHandler.CreateText("Bullet Effects Options", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            void MaxBulletHitChanged(float val)
            {
                MaximumBulletHitParticlesPerFrame = (int)val;
                SyncOptionsMenus();
            }
            MenuHandler.CreateSlider("Global maximum number of bullethit particles per frame", menu, 30, 0f, 50f, (float)MaximumBulletHitParticlesPerFrame, MaxBulletHitChanged, out Slider slider, true, color: easyChangeColor);
            SlidersToSync["MaximumBulletHitParticlesPerFrame"].Add(slider);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            void DisableBulletHitChanged(bool val)
            {
                DisableBulletHitSurfaceParticleEffects = val;
                SyncOptionsMenus();
            }
            TogglesToSync["DisableBulletHitSurfaceParticleEffects"].Add(MenuHandler.CreateToggle(DisableBulletHitSurfaceParticleEffects, "Disable BulletHitSurface particle effects", menu, DisableBulletHitChanged, 30, color: hardChangeColor).GetComponent<Toggle>());
            void DisableBulletHitBulletChanged(bool val)
            {
                DisableBulletHitBulletParticleEffects = val;
                SyncOptionsMenus();
            }
            TogglesToSync["DisableBulletHitBulletParticleEffects"].Add(MenuHandler.CreateToggle(DisableBulletHitBulletParticleEffects, "Disable BulletHitBullet particle effects", menu, DisableBulletHitBulletChanged, 30, color: hardChangeColor).GetComponent<Toggle>());
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
            CreateAdaptiveActiveWarning(menu);

            MenuHandler.CreateText("Miscellaneous Options", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            
            void ShakeChanged(float val)
            {
                ScreenShakeStrength = (int)val;
                SyncOptionsMenus();
            }
            void AberrationChanged(float val)
            {
                ChromaticAberrationStrength = (int)val;
                SyncOptionsMenus();
            }

            MenuHandler.CreateSlider("Screen Shake Strength", menu, 30, 0f, 100f, (int)(ScreenShakeStrength), ShakeChanged, out Slider slider, true);
            MenuHandler.CreateSlider("Chromatic Aberration Strength", menu, 30, 0f, 100f, (int)(ChromaticAberrationStrength), AberrationChanged, out Slider slider1, true);
            SlidersToSync["ScreenShakeStrength"].Add(slider);
            SlidersToSync["ChromaticAberrationStrength"].Add(slider1);
            void LightShakeChanged(bool val)
            {
                DisableOverheadLightShake = val;
                CycleArt();
                SyncOptionsMenus();
            }
            TogglesToSync["DisableOverheadLightShake"].Add(MenuHandler.CreateToggle(DisableOverheadLightShake, "Disable Overhead light shake", menu, LightShakeChanged, 30).GetComponent<Toggle>());

            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            void LightChanged(bool val)
            {
                DisableOverheadLightAndShadows = val;
                CycleArt();
                SyncOptionsMenus();
            }
            TogglesToSync["DisableOverheadLightAndShadows"].Add(MenuHandler.CreateToggle(DisableOverheadLightAndShadows, "Disable Overhead light and shadows", menu, LightChanged, 30, color: hardChangeColor).GetComponent<Toggle>());
            void RemoveOOBChanged(bool val)
            {
                RemoveOutOfBoundsBullets = val;
                SyncOptionsMenus();
            }
            TogglesToSync["RemoveOutOfBoundsBullets"].Add(MenuHandler.CreateToggle(RemoveOutOfBoundsBullets, "Remove out of bounds bullets", menu, RemoveOOBChanged, 30, color: hardChangeColor).GetComponent<Toggle>());
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
            if (!PerformanceImprovements.FixMapLoadLag || PerformanceImprovements.mapTransitionPatchInProgress) { yield break; }

            PerformanceImprovements.mapTransitionPatchInProgress = true;

            this.PostFXDampening = 0f;
            yield return new WaitForSecondsRealtime(transitionTime);
            yield return new WaitUntil(() => PerformanceImprovements.BattleInProgress);
            yield return new WaitForSecondsRealtime(PerformanceImprovements.mapTransitionExtraDelay);
            float t = PerformanceImprovements.postFXRampDuration;
            while (t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                this.PostFXDampening = UnityEngine.Mathf.Lerp(1f, 0f, t / PerformanceImprovements.postFXRampDuration);
                yield return null;
            }
            this.PostFXDampening = 1f;

            PerformanceImprovements.mapTransitionPatchInProgress = false;
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
