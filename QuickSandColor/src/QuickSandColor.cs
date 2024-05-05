using System;
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
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.SoftDependency)]
    internal class QuickSandColor : BaseUnityPlugin
    {
        public const string GUID = "mattymatty.QuickSandColor";
        public const string NAME = "QuickSandColor";
        public const string VERSION = "1.0.0";

        internal static ManualLogSource Log;

        internal static ConfigEntry<string> Color;

        private void Awake()
        {
            Log = Logger;
            try
            {
                if (LobbyCompatibilityChecker.Enabled)
                    LobbyCompatibilityChecker.Init();
                
                Log.LogInfo("Patching Methods");
                var harmony = new Harmony(GUID);
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                Color = Config.Bind("", "color", "#FFFF00", "the new color for quicksand ( in HEX format )");

                Color.SettingChanged += (_, _) =>
                {
                    if (RoundManager.Instance == null) 
                        return;
                    if (ColorUtility.TryParseHtmlString(Color.Value, out var color))
                        RoundManager.Instance.quicksandPrefab.GetComponentInChildren<DecalProjector>().material.color = color;
                    else
                        QuickSandColor.Log.LogFatal($"Color value of '{Color.Value}' cannot be parsed");
                };
                
                if (!ColorUtility.TryParseHtmlString(Color.Value, out _))
                    Log.LogFatal($"Color value of '{Color.Value}' cannot be parsed");

                Log.LogInfo(NAME + " v" + VERSION + " Loaded!");
            }
            catch (Exception ex)
            {
                Log.LogError("Exception while initializing: \n" + ex);
            }
        }

    }
}