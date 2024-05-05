using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace QuickSandColor.Patches
{
    [HarmonyPatch]
    internal class RoundManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.Awake))]
        private static void OnAwake(RoundManager __instance)
        {
            if (RoundManager.Instance == null) 
                return;
            if (ColorUtility.TryParseHtmlString(QuickSandColor.Color.Value, out var color))
                __instance.quicksandPrefab.GetComponentInChildren<DecalProjector>().material.color = color;
            else
                QuickSandColor.Log.LogFatal($"Color value of '{QuickSandColor.Color.Value}' cannot be parsed");
        }
    }
}