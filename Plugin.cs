using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
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

	public static int iteration;

	private void Awake() {
		harmony.PatchAll();
		Logger = base.Logger;
		Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
	}

	[HarmonyPatch(typeof(GameInit), "Start")]
	[HarmonyPrefix]
	static private void PatchStart(GameInit __instance) {
		Logger.LogInfo("PatchStart()");
		LoadMapData loadData = (LoadMapData)typeof(GameInit).InvokeMember("LoadData",
    		BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic,
    		null, __instance, null);
		if (loadData.NewGame) {
			Plugin.iteration = 0;
			EndlessModeSaveData.SaveData(Plugin.iteration);
		} else {
			Plugin.iteration = EndlessModeSaveData.LoadData();
		}
	}

	[HarmonyPatch(typeof(Map.MapController), "ProceedAfterBattle")]
	[HarmonyPrefix]
	static private void PatchProceedAfterBattle(Map.MapController __instance) {
		Logger.LogInfo($"PatchProceedAfterBattle({__instance})");
		if (StaticGameData.loadNextScene && __instance.nextScene == Loading.PeglinSceneLoader.Scene.MINES_WIN) {
			__instance.nextScene = Loading.PeglinSceneLoader.Scene.FOREST_MAP;
			Plugin.iteration++;
			EndlessModeSaveData.SaveData(Plugin.iteration);
		}
	}

	[HarmonyPatch(typeof(Battle.Enemies.Enemy), "Initialize")]
	[HarmonyPrefix]
	static private void PatchEnemyInitialize(ref float ___StartingHealth) {
		// 1 << x == 2 ** x
		___StartingHealth *= (int)1 << Plugin.iteration;
	}

	[HarmonyPatch(typeof(PeglinUI.PlayerInfoUI), "ChangeFloorText")]
	[HarmonyPrefix]
	static private void PatchChangeFloorText(ref string mapName) {
		mapName = $"Iteration {Plugin.iteration+1}\n" + mapName;
	}
}

public class EndlessModeSaveData : SaveObjectData {
	public readonly static string KEY = $"{MyPluginInfo.PLUGIN_GUID}_SaveData";
	public override string Name => KEY;
	public int iteration;

	public EndlessModeSaveData() : base(true, ToolBox.Serialization.DataSerializer.SaveType.RUN) {
		this.iteration = 0;
	}

	public static int LoadData() {
		EndlessModeSaveData data = (EndlessModeSaveData)ToolBox.Serialization.DataSerializer.Load<SaveObjectData>(EndlessModeSaveData.KEY, ToolBox.Serialization.DataSerializer.SaveType.RUN);
		if (data == null) data = new EndlessModeSaveData();
		return data.iteration;
	}

	public static void SaveData(int iteration) {
		EndlessModeSaveData data = new EndlessModeSaveData();
		data.iteration = iteration;
		data.Save();
	}
}

