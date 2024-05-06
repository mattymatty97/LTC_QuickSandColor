using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using QuickSandColor.Dependency;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace QuickSandColor
{
    [HarmonyPatch]
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.SoftDependency)]
    internal class QuickSandColor : BaseUnityPlugin
    {
        internal static QuickSandColor INSTANCE;
        
        public const string GUID = "mattymatty.QuickSandColor";
        public const string NAME = "QuickSandColor";
        public const string VERSION = "1.1.0";

        internal static ManualLogSource Log;

        internal static readonly Dictionary<string,ConfigEntry<string>> ColorMap = new();

        private void Awake()
        {
            INSTANCE = this;
            Log = Logger;
            try
            {
                if (LobbyCompatibilityChecker.Enabled)
                    LobbyCompatibilityChecker.Init();
                
                Log.LogInfo("Patching Methods");
                var harmony = new Harmony(GUID);
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                var colorSetting = Config.Bind("Default", "default_color", "#FFFF00",
                    "the new color for quicksand ( in HEX format )");
                
                colorSetting.SettingChanged += OnConfigChange;
                
                ColorMap.Add("default", colorSetting);
                
                
                if (!ColorUtility.TryParseHtmlString(colorSetting.Value, out _))
                    Log.LogFatal($"Color value for moon 'Default' ('{colorSetting.Value}') cannot be parsed");

                Log.LogInfo(NAME + " v" + VERSION + " Loaded!");
            }
            catch (Exception ex)
            {
                Log.LogError("Exception while initializing: \n" + ex);
            }
        }

        internal static bool GetOrAddMoon(string source, string name, out Color color)
        {
            var entryName = $"{source}.{name}";
            if (!ColorMap.TryGetValue(entryName, out var configEntry))
            {
                configEntry = INSTANCE.Config.Bind(source, name, "Default",
                    "overriden quicksand color for this moon ( in HEX format )");
                
                configEntry.SettingChanged += OnConfigChange;
                
                ColorMap.Add(entryName, configEntry);
            }
            
            var colorString = ColorMap["default"].Value;
            if (configEntry.Value.ToLower() != "default")
            {
                colorString = configEntry.Value;
            }

            if (ColorUtility.TryParseHtmlString(colorString, out color))
                return true;
                
            Log.LogFatal($"Color value for moon '{entryName}' ('{colorString}') cannot be parsed");

            color = new Color();
            return false;
        }

        private static void OnConfigChange(object sender, EventArgs eventArgs)
        {
            UpdateQuicksand();
        }
        
        internal static void UpdateQuicksand()
        {
            if (RoundManager.Instance == null) 
                return;
            StartOfRound startOfRound = StartOfRound.Instance;
            SelectableLevel level = startOfRound.currentLevel;
            if (GetOrAddMoon("Moons", level.PlanetName, out var color))
                RoundManager.Instance.quicksandPrefab.GetComponentInChildren<DecalProjector>().material.color = color;
        }
        
        [HarmonyPatch]
        internal class RoundManagerPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.Awake))]
            private static void OnAwake()
            {
                UpdateQuicksand();
            }
        }
        
        [HarmonyPatch]
        internal class StartOfRoundPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.ChangeLevel))]
            private static void AfterMoonChange()
            {
                UpdateQuicksand();
            }
            
            [HarmonyPostfix]
            [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
            private static void AfterBoot(StartOfRound __instance)
            {
                foreach (SelectableLevel level in __instance.levels)
                {
                    if (level.PlanetName.IsNullOrWhiteSpace())
                        continue;
                    
                    GetOrAddMoon("Moons", level.PlanetName, out _);
                }
            }
            
        }
        
    }
}