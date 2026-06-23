using ADV;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using CharacterCreation;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Manager;
using SaveData;
using SV;
using SV.EntryScene;
using SV.MyRoomScene;
using SV.Title;
using SVS_MapLoader;

namespace MapLoader
{
    [BepInPlugin(GUID, DisplayName, Version)]
    [BepInDependency("SVS_MoreOutfits", BepInDependency.DependencyFlags.SoftDependency)]
    public class MapLoaderPlugin : BasePlugin
    {
        public const string DisplayName = Constants.Name;
        public const string GUID = "DS27.SVS.MapLoader";
        public const string Version = Constants.Version;

        internal static new ManualLogSource Log;
        private static Harmony patchedHooks;

        private static ConfigEntry<bool> _useCustomBGM;
        private static ConfigEntry<bool> _randomTittleMap;
        private static ConfigEntry<bool> _SetNewStartingLocation;
        private static ConfigEntry<bool> _SetNewChangingClothesMaps;
        private static ConfigEntry<bool> jobInteractableArea;
        private static ConfigEntry<bool> jobCustomLogic;

        private static ConfigEntry<bool> enableLog;

        private static bool _customBGMLoaded = false;
        public override void Load()
        {
            Log = base.Log;

            enableLog = Config.Bind("Debug Log", "Enable Log", false, new ConfigDescription("If you have issues with this mod, enable this so it can add the log info into the LogOutput.log and send it to the mod creator", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 10 }));

            //_SetNewStartingLocation = Config.Bind("Basic Settings", "Use Custom Map BGMs", true, "When you are in a custom map, it will play a custom BGM if the Map Pack use one");
            //_SetNewChangingClothesMaps = Config.Bind("Basic Settings", "Use Custom Map BGMs", true, "When you are in a custom map, it will play a custom BGM if the Map Pack use one");

            _useCustomBGM = Config.Bind("Custom BGM", "Use Custom Map BGMs", true, "When you are in a custom map, it will play a custom BGM if the Map Pack use one");
            _randomTittleMap = Config.Bind("Tittle", "Randomize Tittle Screen", true, "Set a random map on the Tittle Screen. 3D maps only");

            jobInteractableArea = Config.Bind("Custom Job", "Jobs interactable areas", true, new ConfigDescription("NPCs with jobs now have a designated area (maps) where they can move around and interact, if a NPC is ouside these areas, the character will have a small chance to go toward that NPC for interactions (this does not affect PC)", null, new ConfigurationManagerAttributes { Order = 10 }));
            //jobCustomLogic = Config.Bind("Custom Job", "Job Conditions", false, new ConfigDescription("It will apply custom conditions for jobs that have them", null, new ConfigurationManagerAttributes { Order = 9 }));

            patchedHooks = Harmony.CreateAndPatchAll(typeof(Hooks));
        }

        public static bool GetMoreOutfitsPlugin(bool showWarning)
        {
            if (IL2CPPChainloader.Instance.Plugins.ContainsKey("SVS_MoreOutfits"))
            {
                var version = IL2CPPChainloader.Instance.Plugins["SVS_MoreOutfits"].Metadata.Version;
                var vNew = new SemanticVersioning.Version("1.2.0");
                int result = version.CompareTo(vNew);
                if (result < 0 && showWarning)
                {
                    Log.Log(BepInEx.Logging.LogLevel.Message,"MapLoader dependency: Old SVS_MoreOutfits detected, please update to v1.2.0 or latest");
                    return false;
                }
                return true;
            }
            return false;
        }
        public override bool Unload()
        {
            patchedHooks?.UnpatchSelf();
            return true;
        }
        public static bool IsLogEnable()
        {
            return enableLog.Value;
        }
        public static bool GetJobInteractableArea()
        {
            return jobInteractableArea.Value;
        }
        public static bool GetIsCustomBGMOn()
        {
            return _useCustomBGM.Value;
        }
        internal static class Hooks
        {
            static int _previousMap = 0;
            static bool restoreBGM = false; //Used on ChangeBMGDependingOnMap()

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TitleScene), nameof(TitleScene.Start))]
            public static void MapLoaderPreStartTittleScreen(TitleScene __instance)
            {
                if (!GetMoreOutfitsPlugin(true)) Log.LogInfo("More Outfits plugin not found");
                MapLoader.InitLoad();
                if (_randomTittleMap.Value) __instance._mapID = MapLoader.RandomTittleScreenMap();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SimulationScene), nameof(SimulationScene.Start))]
            public static void MapLoaderPreStartSimulation(SimulationScene __instance)
            {
                MapLoader.Init();
                MapLoader.LoadCustomBGM();
                MapLoaderCustomBGM.InitCustomBGM();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(HumanCustom), nameof(HumanCustom.Start))]
            public static void MapLoaderPreStartCharaMaker(HumanCustom __instance)
            {
                MapLoader.Init();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SV.MyRoomScene.ImageSprite), nameof(ImageSprite.Set))]
            public static void AddCustomJobIconsOnProfile(ImageSprite __instance)
            {
                if (__instance._sprites.Count != GlobalListLoad.Instance.jobNameTable.Count)
                {
                    MapLoader.CreateCustomJobsSpritesOnRoster(__instance);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(EntryFileInfoComponent), nameof(EntryFileInfoComponent.Refresh))]
            public static void AddCustomJobIcons(EntryFileInfoComponent __instance)
            {
                if (__instance.Info.Job > 3)
                {
                    if (__instance._spWorkIcons.Length != GlobalListLoad.Instance.jobNameTable.Count)
                    {
                        MapLoader.CreateCustomJobsSprites(__instance);
                    }
                }

                if (!GlobalListLoad.Instance.jobNameTable.ContainsKey(__instance.Info.Job))
                {
                    Log.Log(LogLevel.Message, "Invalid Job ID for Character: " + __instance.Info.Name + " Missing mods or files could be the cause.");
                    Log.LogInfo($"Invalid Job ID for Character: " + __instance.Info.FileName);
                }
            }

            /*[HarmonyPrefix]
            [HarmonyPatch(typeof(CharacterCreation.UI.JobSelectView), nameof(JobSelectView.Awake))]
            public static void AddMakerJobList(JobSelectView __instance)
            {
                if (__instance._jobNameTable.Count != GlobalListLoad.Instance.jobNameTable.Count)
                {
                    MapLoaderFunctions.MakerCustomJobList(__instance);
                }
            }*/

            [HarmonyPrefix]
            [HarmonyPatch(typeof(MapMoveUI), nameof(MapMoveUI.SetRooftopButtonVisible))]
            public static void IncreaseMapMoveUIJobList(MapMoveUI __instance)
            {
                var _jobMaps = MapManager.Instance;
                if (_jobMaps != null)
                {
                    if (__instance.jobMap.Length != _jobMaps.gojobmaps.Length) __instance.jobMap = _jobMaps.gojobmaps;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(AtmosphereAndBGMManager), nameof(AtmosphereAndBGMManager.Play))]
            public static void ChangeBMGDependingOnMap(AtmosphereAndBGMManager __instance, Manager.Sound.Loader _loader)
            {
                MapLoaderCustomBGM.PlayCustomBGM(_loader);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SimulationScene), nameof(SimulationScene.Update))]
            public static void ResetBGMOnEnteringNewMap(SimulationScene __instance)
            {
                MapLoaderCustomBGM.ChangeBGMWhenChangingMap(_useCustomBGM.Value);
            }

            [HarmonyPriority(300)]
            [HarmonyPostfix]
            [HarmonyPatch(typeof(ThinkingManager), nameof(ThinkingManager.InterpersonalCommandSelectionTarget))]
            public static int SetTarget(int __result, Actor _actor, int _commandID)
            {
                if (__result != -1) __result = MapLoader.CheckTargetLocation(_actor, _commandID, __result);
                return __result;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ThinkingManager), nameof(ThinkingManager.GetChangeOfClothesMapID))]
            public static int SetChangingMapID(int __result, Actor _actor, int timezone)
            {
                if (__result > 16)
                {
                    __result = MapLoader.CheckChangingRoomSex(_actor, __result);                   
                }
                return __result;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(BaseAnswer), nameof(BaseAnswer.Judge))]
            public static void CustomMapActionSuccessRate(BaseAnswer __instance, bool __result, YesNoJudgeManager.AnswerInfo _ansInfo, YesNoJudgeManager.YesNoInfo _ynInfo, int _commandID, int _questionCount, Il2CppStructArray<bool> _calcs)
            {
                if (__result)
                {
                    _ansInfo = MapLoader.ActionSuccessRate(_ansInfo, _ynInfo, _commandID);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ADV.Setup), nameof(ADV.Setup.Open))]
            public static void CustomJobADVLoader(OpenData openData)
            {
                MapLoader.CustomJobADV(openData);
            }

            [HarmonyPriority(300)]
            [HarmonyPostfix]
            [HarmonyPatch(typeof(SVThinking), nameof(SVThinking.OnUpdate))]
            public static void ChangeAction(SVThinking __instance)
            {
                if (IsLogEnable()) Log.LogInfo($"ChangeAction calls");
                if (__instance != null) MapLoaderJobs.JobCharaAction(__instance.CharaCtrl, null);
            }

            [HarmonyPriority(300)]
            [HarmonyPostfix]
            [HarmonyPatch(typeof(MapManager), nameof(MapManager.SetCharaMapMove))]
            public static void ChangeActionAfterInteraction(MapManager __instance, BehaviourController.BaseActionKind _kind, BehaviourController _bCtrl, MapManager.MapTargetInfo _mapTarget, bool isResotreNavMeshCollisionRadius = false)
            {
                if (IsLogEnable()) Log.LogInfo($"ChangeActionAfterInteraction calls");
                MapLoaderJobs.JobCharaAction(_bCtrl, _mapTarget);
            }
        }
    }
}
