using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace ScalingEndlessMode;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Peglin.exe")]
[HarmonyPatch]
public class Plugin : BaseUnityPlugin {
	private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
	internal static new ManualLogSource Logger;

	public static long enemyHpMult = 1;

	private void Awake() {
		harmony.PatchAll();
		Logger = base.Logger;
		Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
	}

	[HarmonyPatch(typeof(GameInit), "Start")]
	[HarmonyPrefix]
	static private void PatchStart() {
		Plugin.enemyHpMult = 1;
	}

	[HarmonyPatch(typeof(Map.MapController), "ProceedAfterBattle")]
	[HarmonyPrefix]
	static private void PatchProceedAfterBattle(Map.MapController __instance) {
		Logger.LogInfo($"PatchProceedAfterBattle({__instance})");
		if (StaticGameData.loadNextScene && __instance.nextScene == Loading.PeglinSceneLoader.Scene.MINES_WIN) {
			__instance.nextScene = Loading.PeglinSceneLoader.Scene.FOREST_MAP;
			Plugin.enemyHpMult *= 2;
		}
	}

	[HarmonyPatch(typeof(Battle.Enemies.Enemy), "Initialize")]
	[HarmonyPrefix]
	static private void PatchEnemyInitialize(ref float ___StartingHealth) {
		___StartingHealth *= Plugin.enemyHpMult;
	}
}

