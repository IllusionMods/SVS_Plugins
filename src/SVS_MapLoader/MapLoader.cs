using ADV;
using BepInEx;
using BepInEx.Logging;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using ILLGames.Extensions;
using Manager;
using SaveData;
using SV;
using SV.EntryScene;
using SV.MyRoomScene;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace MapLoader
{
    internal static class MapLoader
    {
        public static Dictionary<int, List<string>> oldJobNameTable = new Dictionary<int, List<string>>();
       
        private static System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<MapLoaderParam.JobsParam>> jobsParamDic = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<MapLoaderParam.JobsParam>>();
        private static System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<MapLoaderParam.MapActionInfoParam>> mapActionDic = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<MapLoaderParam.MapActionInfoParam>>();
        private static System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<MapLoaderParam.JobADV>> jobADVDic = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<MapLoaderParam.JobADV>>();

        private static System.Collections.Generic.Dictionary<int, AssetBundle> _oldBundlesDic = new System.Collections.Generic.Dictionary<int, AssetBundle>();
        private static System.Collections.Generic.Dictionary<string, AssetBundle> _newBundlesDic = new System.Collections.Generic.Dictionary<string, AssetBundle>();

        private static System.Collections.Generic.Dictionary<int, List<int[]>> explorableMapList = new System.Collections.Generic.Dictionary<int, List<int[]>>();

        private static System.Collections.Generic.List<int> tittleMapID = new System.Collections.Generic.List<int>();

        private static System.Collections.Generic.List<int> _addedLocationsToIDs = new System.Collections.Generic.List<int>();
        private static System.Collections.Generic.List<int> _newBGMListIndex = new System.Collections.Generic.List<int>();
        private static System.Collections.Generic.List<int[]> _oldIDtoNewIDList = new System.Collections.Generic.List<int[]>();
        private static System.Collections.Generic.List<string> _MapPacksList = new System.Collections.Generic.List<string>();
        //private static System.Collections.Generic.List<int[]> _startingMapsIDs = new System.Collections.Generic.List<int[]>();

        private static System.Collections.Generic.List<int> soloMapsList = new System.Collections.Generic.List<int>();

        private static System.Collections.Generic.List<MapLoaderParam.BGMInfo> _customBGMList = new System.Collections.Generic.List<MapLoaderParam.BGMInfo>();
        private static System.Collections.Generic.List<AssetBundle> _newBundles = new System.Collections.Generic.List<AssetBundle>();
        private static System.Collections.Generic.List<AssetBundle> _oldBundles = new System.Collections.Generic.List<AssetBundle>();
       
        //private static System.Collections.Generic.List<int> femaleChangingRoomsList = new System.Collections.Generic.List<int>();
        //private static System.Collections.Generic.List<int> maleChangingRoomsList = new System.Collections.Generic.List<int>();

        public static System.Random _rnd = new System.Random();

        public static bool _taskMapLoader = false;
        public static bool _customMapsLoaded = false;

        private static int tittleMapCount = 0;

        public static void InitLoad()
        {
            if (_customMapsLoaded) return;
            string CustomMapDirectory = GetCustomMapPath();
            if (!Directory.Exists(CustomMapDirectory))
            {
                MapLoaderPlugin.Log.LogInfo("No Custom Maps Detected");
                _customMapsLoaded = true;
                return;
            }
            var _mapManagerInstance = MapManager.Instance;

            if (_mapManagerInstance == null) return;

            _customMapsLoaded = LoadCustomMaps(_mapManagerInstance, CustomMapDirectory);
            if (!_customMapsLoaded) _customMapsLoaded = LoadCustomMapsOld(_mapManagerInstance, CustomMapDirectory);
        }
        public static void Init()
        {
            if (!_customMapsLoaded) return;
            if (_taskMapLoader) return;
            string CustomMapDirectory = GetCustomMapPath();
            if (!Directory.Exists(CustomMapDirectory))
            {
                MapLoaderPlugin.Log.LogInfo("Can not find Custom Map folder");
                return;
            }
            var _mapManagerInstance = MapManager.Instance;
            var _jobNameTable = GlobalListLoad.Instance.jobNameTable;

            if (_mapManagerInstance == null || _jobNameTable == null) return;

            if (LoadMapActionInfo(CustomMapDirectory))
            {
                SetCustomMapIndependentActionTable(_mapManagerInstance);
                GetSoloLocations();
            }

            if (GetJobList(CustomMapDirectory))
            {
                SetCustomJobs(_mapManagerInstance);
                SetStartingLocationByJob(_mapManagerInstance);
                SetChangingClothesLocationByJob(_mapManagerInstance);
                SetJobClothes();
                SetJobPeriodADV();
            }

            _taskMapLoader = true;
        }
        //
        public static System.Collections.Generic.List<MapLoaderParam.BGMInfo> GetCustomBGMList()
        {
            return _customBGMList;
        }
        //
        public static int RandomTittleScreenMap()
        {
            if (tittleMapID.Count == 0)
            {
                foreach (var map in MapManager.Instance.MapListTable)
                {
                    if (map.Value.Kind == 0)
                    {
                        if (map.Value.IsUseTimezone2D) continue;
                        tittleMapID.Add(map.Key);
                        tittleMapCount++;
                    }
                }
            }
            int mapID = tittleMapID[_rnd.Next(0, tittleMapCount)];
            if (mapID == 1 || mapID == 4 || mapID == 3)
            {
                var light = GameObject.Find("Directional Light");
                if (light != null)
                {
                    light.active = false;
                }
            }
            else
            {
                var light = GameObject.Find("Directional Light");
                if (light != null)
                {
                    if (!light.active) light.active = true;
                }
            }
            return mapID;
        }
        public static string GetCustomMapPath()
        {
            string customMapPath = System.IO.Path.Combine(Paths.GameRootPath, "abdata\\mods\\CustomMaps");
            return customMapPath;
        }
        public static string GetGamePath()
        {
            string GamePath = System.IO.Path.GetFullPath(Paths.GameRootPath);
            return GamePath;
        }
        //
        public static T? LoadAsset<T>(this AssetBundle bundle, string name) where T : UnityEngine.Object
        {
            return bundle.LoadAsset(name, Il2CppType.Of<T>())?.Cast<T>();
        }
        private static byte[] GetPngResourceAsByteArray(string resourceName)
        {
            // Get the assembly containing the embedded resource
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Construct the full resource name (Namespace.FolderName.FileName.Extension)
            // Example: "YourProjectNamespace.Images.MyImage.png"
            // You can get all resource names using assembly.GetManifestResourceNames() for debugging.
            string fullResourceName = resourceName;

            using (Stream stream = assembly.GetManifestResourceStream(fullResourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Embedded resource '{fullResourceName}' not found.");
                }

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
        //
        public static bool LoadCustomMaps(MapManager _mapManager, string customMapDirectory)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(customMapDirectory);
            var mapPacks = dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
            if (mapPacks.Length == 0 || mapPacks == null) return false;
            if (mapPacks.Length > 1)
            {
                MapLoaderPlugin.Log.Log(LogLevel.Message, $"Multiple Map Packs Detected! Only one will be loaded");
            }

            foreach (var pack in mapPacks)
            {
                var _checkPath = Path.Combine(pack.FullName, "mapinfo");
                if (Directory.Exists(_checkPath))
                {
                    var _mapListFilePath = Path.Combine(pack.FullName, "mapinfo\\pack.json");
                    if (!System.IO.File.Exists(_mapListFilePath))
                    {
                        MapLoaderPlugin.Log.Log(LogLevel.Message, $"Missing pack.json file for Custom map {pack.Name}");
                        continue;
                    }

                    var _mapInfoList = MapLoaderParam.DeserializeMapInfo(_mapListFilePath);
                    if (_mapInfoList == null)
                    {
                        MapLoaderPlugin.Log.LogInfo($"Invalid pack.json file");
                        continue;
                    }

                    if (!_mapInfoList.Enable) continue;

                    List<string> _mapList = new List<string>();
                    foreach (var bundle in _mapInfoList.MapListBundle)
                    {
                        _mapList.Add(bundle);
                    }
                    MapManager.Instance.LoadMapInfo(_mapList);
                    MapManager.Instance.LoadNavMeshInfo(_mapList);

                    _MapPacksList.Add(pack.FullName); //Add Map Pack Path

                    if (_mapInfoList.JobsADVList != null)
                    {
                        foreach (var advList in _mapInfoList.JobsADVList)
                        {
                            GetJobsADV(advList);
                        }                      
                        if (jobADVDic.Count > 0) MapLoaderPlugin.Log.LogDebug($"Found Custom Jobs ADVs");
                    }
                    else MapLoaderPlugin.Log.LogInfo($"Missing path for Custom Jobs ADVs");

                    MapLoaderPlugin.Log.Log(LogLevel.Message, $"Custom Map: {_mapInfoList.Name} version: {_mapInfoList.Version} has been loaded!");
                    return true;
                }
                else MapLoaderPlugin.Log.Log(LogLevel.Message, $"Missing mapinfo folder for Custom map {pack.Name}");
            }
            return false;
        }
        public static bool LoadCustomMapsOld(MapManager _mapManager, string customMapDirectory)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(customMapDirectory);
            var mapPacks = dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
            if (mapPacks.Length == 0 || mapPacks == null) return false;
            MapLoaderPlugin.Log.LogInfo($"Loading old pack.json file");

            foreach (var pack in mapPacks)
            {
                var _checkPath = Path.Combine(pack.FullName, "mapinfo");
                if (Directory.Exists(_checkPath))
                {
                    var _mapListFilePath = Path.Combine(pack.FullName, "mapinfo\\pack.json");
                    if (!System.IO.File.Exists(_mapListFilePath))
                    {
                        MapLoaderPlugin.Log.Log(LogLevel.Message, $"OLD Missing pack.json file for Custom map {pack.Name}");
                        continue;
                    }

                    var _oldMapInfoList = MapLoaderParam.DeserializeOldMapInfo(_mapListFilePath);
                    if (_oldMapInfoList == null)
                    {
                        MapLoaderPlugin.Log.LogInfo($"OLD Invalid old pack.json File");
                        continue;
                    }
                    if (!_oldMapInfoList.Enable) continue;

                    List<string> _oldmapList = new List<string>();
                    foreach (var bundle in _oldMapInfoList.MapListBundle)
                    {
                        _oldmapList.Add(bundle);
                    }
                    //_mapManager.LoadMapInfo(_oldmapList);
                    //_mapManager.LoadNavMeshInfo(_oldmapList);
                    MapManager.Instance.LoadMapInfo(_oldmapList);
                    MapManager.Instance.LoadNavMeshInfo(_oldmapList);

                    MapLoaderPlugin.Log.Log(LogLevel.Message, $"Custom Map:{_oldMapInfoList.Name} version:{_oldMapInfoList.Version} has been loaded!");
                    return true;
                }
                else MapLoaderPlugin.Log.Log(LogLevel.Message, $"Missing mapinfo folder for Custom map {pack.Name}");
            }
            return false;
        }
        //
        public static void LoadCustomBGM()
        {
            if (_MapPacksList.Count == 0) return;

            var BGMCustomDirectory = GetGamePath();
            if (!Directory.Exists(BGMCustomDirectory)) return;
            bool bgmLoaded = false;

            //Search for MapPack Folders
            foreach (var pack in _MapPacksList)
            {
                var _checkPath = Path.Combine(pack, "mapinfo");
                if (Directory.Exists(_checkPath))
                {
                    var _bgmFilePath = Path.Combine(pack, "mapinfo\\bgm.json");
                    if (!System.IO.File.Exists(_bgmFilePath)) continue;

                    _customBGMList = MapLoaderParam.DeserializeBGM(_bgmFilePath);
                    if (_customBGMList == null || _customBGMList.Count <= 0)
                    {
                        MapLoaderPlugin.Log.LogInfo($"Invalid BGM Json File");
                        continue;
                    }
                    bgmLoaded = true;
                }
            }
            if (bgmLoaded) MapLoaderPlugin.Log.LogInfo($"Loaded {_customBGMList.Count} Custom BGMs");
            else MapLoaderPlugin.Log.LogInfo($"No Custom BGMs detected");
        }       
        //
        public static bool LoadMapActionInfo(string _customMapPath)
        {
            if (_MapPacksList.Count == 0) return false;

            DirectoryInfo dirInfo = new DirectoryInfo(_customMapPath);
            var mapPacks = dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
            if (mapPacks.Length == 0 || mapPacks == null) return false;
            int key = 100;
            foreach (var pack in mapPacks)
            {
                var _checkPath = Path.Combine(pack.FullName, "mapinfo");
                if (Directory.Exists(_checkPath))
                {
                    var _mapActionFilePath = Path.Combine(pack.FullName, "mapinfo\\actions.json");
                    if (!System.IO.File.Exists(_mapActionFilePath)) continue;
                    System.Collections.Generic.List<MapLoaderParam.MapActionInfoParam> _mapActionList = new System.Collections.Generic.List<MapLoaderParam.MapActionInfoParam>();
                    _mapActionList = MapLoaderParam.DeserializeMapAction(_mapActionFilePath);
                    if (_mapActionList == null || _mapActionList.Count <= 0)
                    {
                        MapLoaderPlugin.Log.LogInfo($"Invalid Map Actions Json File");
                        continue;
                    }
                    mapActionDic.Add(key, _mapActionList);
                }
                key++;
            }
            if (mapActionDic.Count > 0) return true;
            return false;
        }
        public static void GetSoloLocations()
        {
            foreach (var mapActionList in mapActionDic.Values)
            {
                foreach (var mapSolo in mapActionList)
                {
                    if (mapSolo.SoloLocation)
                    {
                        if (!soloMapsList.Contains(mapSolo.ID)) soloMapsList.Add(mapSolo.ID);
                    }
                }
            }
        }
        public static System.Collections.Generic.List<MapLoaderParam.MapActionInfoParam> GetMapActionList()
        {
            System.Collections.Generic.List<MapLoaderParam.MapActionInfoParam> mapActionList = new();
            foreach (var mapActions in mapActionDic.Values)
            {
                mapActionList = mapActions;
                break;
            }
            return mapActionList;
        }
        public static void SetCustomMapIndependentActionTable(MapManager mapManager)
        {
            var _independentAction = ThinkingManager.Instance.independentActionTable;
            var _mapList = MapManager._instance.MapListTable;

            foreach (var mapActionList in mapActionDic.Values)
            {
                foreach (var mapAction in mapActionList)
                {
                    if (!_mapList.ContainsKey(mapAction.ID)) continue;
                    if (!_independentAction.ContainsKey(mapAction.ID))
                    {
                        int ID = mapAction.ID;
                        ThinkingBaseFloatDataParam thinkingBaseFloatDataParam = new ThinkingBaseFloatDataParam();
                        thinkingBaseFloatDataParam.ID = ID;
                        thinkingBaseFloatDataParam.BaseRate = mapAction.BaseRate;
                        thinkingBaseFloatDataParam.Rates = new Il2CppSystem.Collections.Generic.List<float>();
                        foreach (var rate in mapAction.Rates)
                        {
                            thinkingBaseFloatDataParam.Rates.Add(rate);
                        }
                        _independentAction.Add(ID, thinkingBaseFloatDataParam);
                    }
                }
            }
            ThinkingManager.Instance.independentActionTable = _independentAction;
            MapLoaderPlugin.Log.LogDebug($"Loaded Custom Independent Action Table");
        }
        //
        public static int CheckTargetLocation(Actor actor, int commandID, int targetID)
        {
            int isNewTarget = targetID;

            if (MapLoaderPlugin.GetJobInteractableArea())
            {
                System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int[]>> jobParamList = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int[]>>();

                int weekDay = Manager.Game.saveData.Week;
                int charaJob = actor.Job;
                int mapChara = actor.charaBase.BehaviourCtrl.nowMapID;
                int mapNPC = -2;

                foreach (var jobParam in jobsParamDic.Values)
                {
                    if (jobParam[charaJob].ExplorationArea != null) jobParamList = jobParam[charaJob].ExplorationArea;
                    else MapLoaderPlugin.Log.LogInfo($"ExplorationArea is Null");
                    break;
                }

                if (jobParamList.Count > 0)
                {
                    var tempAI = SimulationScene.Instance.tempAIs;
                    //bool[] foundChara = [false,false];  
                    foreach (var _ai in tempAI)
                    {
                        /*if (_ai.charaData.charasGameParam.Index == actor.charasGameParam.Index)
                        {
                            //npcAI = _ai;
                            mapChara = _ai.BehaviourCtrl.nowMapID;
                            foundChara[0] = true;
                        }*/
                        if (_ai.charaData.charasGameParam.Index == targetID)
                        {
                            //npcAI = _ai;
                            mapNPC = _ai.BehaviourCtrl.nowMapID;
                            //foundChara[1] = true;
                            break;
                        }
                        //if (foundChara[0] && foundChara[1]) break;
                    }

                    if (mapChara != mapNPC)
                    {
                        var chance = _rnd.Next(0, 100);
                        var time = MapLoaderUtils.GetTimeZone();

                        string day = "Monday";
                        switch (weekDay)
                        {
                            case 0:
                                day = "Monday";
                                if (!jobParamList[day][time].Contains(mapNPC) && !jobParamList[day][time].Contains(-1)) if (chance > 10) isNewTarget = -1;
                                break;
                            case 1:
                                day = "Tuesday";
                                if (!jobParamList[day][time].Contains(mapNPC) && !jobParamList[day][time].Contains(-1)) if (chance > 10) isNewTarget = -1;

                                break;
                            case 2:
                                day = "Wesnesday";
                                if (!jobParamList[day][time].Contains(mapNPC) && !jobParamList[day][time].Contains(-1)) if (chance > 10) isNewTarget = -1;

                                break;
                            case 3:
                                day = "Thursday";
                                if (!jobParamList[day][time].Contains(mapNPC) && !jobParamList[day][time].Contains(-1)) if (chance > 10) isNewTarget = -1;

                                break;
                            case 4:
                                day = "Friday";
                                if (!jobParamList[day][time].Contains(mapNPC) && !jobParamList[day][time].Contains(-1)) if (chance > 10) isNewTarget = -1;

                                break;
                            case 5:
                                day = "Saturday";
                                if (!jobParamList[day][time].Contains(mapNPC) && !jobParamList[day][time].Contains(-1)) if (chance > 10) isNewTarget = -1;

                                break;
                            case 6:
                                day = "Sunday";
                                if (!jobParamList[day][time].Contains(mapNPC) && !jobParamList[day][time].Contains(-1)) if (chance > 10) isNewTarget = -1;

                                break;
                        }
                        //MapLoaderPlugin.Log.LogInfo($"CheckTargetLocation: Day:{day} time:{time} Job: {actor.Job} NPC Map: {mapNPC} result:{isNewTarget}");
                    }
                }
                else MapLoaderPlugin.Log.LogInfo($"jobParamList is Empty");
            }           

            isNewTarget = CheckSoloMap(actor, isNewTarget);

            return isNewTarget;
        }
        public static int CheckSoloMap(Actor _actor, int targetID)
        {
            if (soloMapsList.Count == 0) return targetID;

            var _listAI = SimulationScene._instance.tempAIs;
            int index = 0;
            int indexTarget = 0;
            int indexMain = 0;

            int found = 0;
            foreach (var _ai in _listAI)
            {
                if (_ai._charaData.charasGameParam._Index_k__BackingField == targetID)
                {
                    indexTarget = index;
                    found++;

                    if (found >= 2) break;
                }
                if (_ai._charaData.charasGameParam._Index_k__BackingField == _actor.charasGameParam._Index_k__BackingField)
                {
                    indexMain = index;
                    found++;
                    if (found >= 2) break;
                }
                index++;
            }

            if (found != 2) return targetID;

            if (soloMapsList.Contains(_listAI[indexTarget].BehaviourCtrl.nowMapID))
            {
                if (_listAI[indexMain].BehaviourCtrl.nowMapID != _listAI[indexTarget].BehaviourCtrl.nowMapID) return -1;
                return targetID;
            }
            if (_actor.parameter.sex == 1)
            {
                if (MapManager.Instance.MapListTable.ContainsKey(_listAI[indexTarget].BehaviourCtrl.nowMapID))
                {
                    if (MapManager.Instance.MapListTable[_listAI[indexTarget].BehaviourCtrl.nowMapID].Sex == 0)
                    {
                        if (_actor.gameParameter.individuality.answer.Contains(12) || _actor.gameParameter.individuality.answer.Contains(31))
                        {
                            var _go = _rnd.Next(1, 100);
                            if (_go > 66) return targetID;
                        }
                        return -1;
                    }
                }
            }
            if (_actor.parameter.sex == 0)
            {
                if (MapManager.Instance.MapListTable.ContainsKey(_listAI[indexTarget].BehaviourCtrl.nowMapID))
                {
                    if (MapManager.Instance.MapListTable[_listAI[indexTarget].BehaviourCtrl.nowMapID].Sex == 1)
                    {
                        if (_actor.gameParameter.individuality.answer.Contains(12) || _actor.gameParameter.individuality.answer.Contains(31))
                        {
                            var _go = _rnd.Next(1, 100);
                            if (_go > 66) return targetID;
                        }
                        return -1;
                    }
                }
            }
            return targetID;
        }
        public static bool CheckSoloPublicHLocation(SVThinking _actorAI, out SVThinking _newActorAI)
        {
            _newActorAI = _actorAI;
            if (_newActorAI.CharaCtrl.Target.kind == BehaviourController.TargetInfo.TargetKind.Map) return false;
            if (_newActorAI.CharaCtrl.AI.charaData.CommandNo != 37) return false;

            if (Manager.Game.saveData.Charas.ContainsKey(_newActorAI.CharaCtrl.Target.id))
            {
                var _listAI = SimulationScene._instance.tempAIs;
                foreach (var _ai in _listAI)
                {
                    if (_ai._charaData.charasGameParam._Index_k__BackingField == _newActorAI.CharaCtrl.Target.id)
                    {
                        if (soloMapsList.Contains(_ai.BehaviourCtrl.nowMapID))
                        {
                            _newActorAI.CharaCtrl.AI.charaData.CommandNo = 35;
                            return true;
                        }
                        break;
                    }
                }
            }
            return false;
        }

        //Actions
        public static bool IsCustomaMap(YesNoJudgeManager.YesNoInfo _ynInfo)
        {
            if (_ynInfo.pParam == null) return false;

            if (_ynInfo.passive.charaBase != null)
            {
                if (_ynInfo.passive.charaBase.BehaviourCtrl != null)
                {
                    if (_ynInfo.passive.charaBase.BehaviourCtrl.nowMapID > 15)
                    {
                        return true;
                    }
                }
            }
            /*var _listAI = SimulationScene._instance.tempAIs;
            foreach (var ai in _listAI)
            {
                if (ai._charaData.charasGameParam._Index_k__BackingField == _ynInfo.pParam._Index_k__BackingField)
                {
                    
                    break;
                }
            }*/
            return false;
        }
        public static YesNoJudgeManager.AnswerInfo ActionSuccessRate(YesNoJudgeManager.AnswerInfo _ansInfo, YesNoJudgeManager.YesNoInfo _ynInfo, int _commandID)
        {
            if (_ynInfo.passive == null) return _ansInfo;         
            var isCustomMap = IsCustomaMap(_ynInfo);
            //Check if the Always 100% success rate is on.
            var alwaysTrue = Game.IsUnlockedAdditionalFunction(22);

            switch (_commandID)
            {
                case 35://Sex
                    if (isCustomMap)
                    {
                        if (_ynInfo.passive.gameParameter.LvChastity == 4)
                        {
                            if (_ynInfo.passive.charaBase != null)
                            {
                                if (_ynInfo.passive.charaBase.BehaviourCtrl != null)
                                {
                                    if (!soloMapsList.Contains(_ynInfo.passive.charaBase.BehaviourCtrl.nowMapID))
                                    {
                                        _ansInfo.rate = 0;
                                        _ansInfo.ans = 1;
                                    }
                                }
                            }

                            /*var _listAI = SimulationScene._instance.tempAIs;
                            foreach (var ai in _listAI)
                            {
                                if (ai._charaData.charasGameParam._Index_k__BackingField == _ynInfo.passive.charasGameParam._Index_k__BackingField)
                                {
                                    if (!soloMapsList.Contains(ai.BehaviourCtrl.nowMapID))
                                    {
                                        _ansInfo.rate = 0;
                                        _ansInfo.ans = 1;
                                    }
                                    break;
                                }
                            }*/
                        }
                    }
                    
                    break;
                case 36://Follow Me
                    MapLoaderJobs.JobsFollowMeRestrictions(_ynInfo.active, jobsParamDic);
                    if (isCustomMap)
                    {
                        var newRate = CalcFollowMeSexRoomBaseRate(_ynInfo.passive, _ynInfo.active, _ansInfo.rate);
                        if (alwaysTrue && newRate > 0)
                        {
                            _ansInfo.ans = 0;
                            break;
                        }
                        if (_ansInfo.rate == newRate) break;
                        else _ansInfo.rate = newRate;
                        int chance = _rnd.Next(1, 100);
                        if (chance <= _ansInfo.rate) _ansInfo.ans = 0;
                        else _ansInfo.ans = 1;
                    }                       
                    break;
            }
            return _ansInfo;
        }
        public static float CalcFollowMeSexRoomBaseRate(Actor _actor, Actor askingActor, float oldRate)
        {
            //var listAI = SimulationScene.Instance.tempAIs;
            int askingCharaID = askingActor.charasGameParam.Index;

            bool FtoM = false;
            bool MtoF = false;

            float baseRate = 0;

            //int index = 0;
            //int indexTarget = 0;
            //int indexMain = 0;
            //int found = 0;
            /*foreach (var ai in listAI)
            {
                if (ai.charaData.charasGameParam.Index == askingCharaID)
                {
                    indexTarget = index;
                    found++;

                    if (found >= 2) break;
                }
                if (ai._charaData.charasGameParam.Index == _actor.charasGameParam._Index_k__BackingField)
                {
                    indexMain = index;
                    found++;
                    if (found >= 2) break;
                }
                index++;
            }*/

            if (askingActor.charaBase.BehaviourCtrl.target.IsMap && askingActor.IsPC)
            {
                if (MapManager.Instance.MapListTable[askingActor.charaBase.BehaviourCtrl.target.id].Sex == 0 && _actor.parameter.sex == 1)
                {
                    FtoM = true;
                }
                if (MapManager.Instance.MapListTable[askingActor.charaBase.BehaviourCtrl.target.id].Sex == 1 && _actor.parameter.sex == 0)
                {
                    MtoF = true;
                }
            }
            else if (!askingActor.IsPC)
            {
                var mapIDTarget = askingActor.charasGameParam.thinkingPropertyTemp.beTakenMapNo;
                if (MapManager.Instance.MapListTable.ContainsKey(mapIDTarget))
                {
                    if (MapManager.Instance.MapListTable[mapIDTarget].Sex == 0 && _actor.parameter.sex == 1) FtoM = true;
                    if (MapManager.Instance.MapListTable[mapIDTarget].Sex == 1 && _actor.parameter.sex == 0) MtoF = true;
                }              
            }

            if (FtoM || MtoF)
            {
                switch (_actor.gameParameter.LvChastity)
                {
                    case 2:
                        baseRate = 30;
                        if (_actor.charasGameParam.sensitivity.tableFavorabiliry[askingCharaID].ranks[1] == SensitivityParameter.Rank.MIDDLE) baseRate += 15;

                        break;
                    case 3:
                        baseRate = 20;

                        break;
                    case 4:
                        baseRate = 10;
                        break;
                }

                if (_actor.charasGameParam.sensitivity.tableFavorabiliry[askingCharaID].ranks[0] == SensitivityParameter.Rank.MIDDLE) baseRate += 30;
                if (_actor.charasGameParam.sensitivity.tableFavorabiliry[askingCharaID].ranks[1] == SensitivityParameter.Rank.HIGH || _actor.charasGameParam.sensitivity.tableFavorabiliry[askingCharaID].ranks[1] == SensitivityParameter.Rank.MAX) baseRate = 20;
                if (_actor.charasGameParam.sensitivity.tableFavorabiliry[askingCharaID].ranks[0] == SensitivityParameter.Rank.HIGH || _actor.charasGameParam.sensitivity.tableFavorabiliry[askingCharaID].ranks[0] == SensitivityParameter.Rank.MAX) baseRate = oldRate;

                //Check Mood
                if (_actor.charasGameParam.state._State_k__BackingField == StateParameter.StateKind.UPLIFT) baseRate += 4;
                if (_actor.charasGameParam.state._State_k__BackingField == StateParameter.StateKind.EARNESTNESS) baseRate += 8;
                if (_actor.charasGameParam.state._State_k__BackingField == StateParameter.StateKind.ANGER) baseRate -= 16;
                if (_actor.charasGameParam.state._State_k__BackingField == StateParameter.StateKind.DISAPPOINTMENT) baseRate -= 8;
                if (_actor.charasGameParam.state._State_k__BackingField == StateParameter.StateKind.TENSION) baseRate -= 4;

                //add
                if (_actor.gameParameter.individuality.answer.Contains(7)) baseRate += 5;
                if (_actor.gameParameter.individuality.answer.Contains(38)) baseRate += 10;

                //Multi-Division
                if (_actor.gameParameter.individuality.answer.Contains(2) || _actor.gameParameter.individuality.answer.Contains(3))
                {
                    if (askingActor.parameter.sex == 0) baseRate /= 1.1f;
                    if (askingActor.parameter.sex == 1) baseRate /= 1.1f;
                }
                if (_actor.gameParameter.individuality.answer.Contains(12)) baseRate *= 1.1f;
                if (_actor.gameParameter.individuality.answer.Contains(13)) baseRate /= 1.1f;

                if (baseRate < 0) baseRate = 0;
                if (FtoM) return baseRate;
                if (MtoF) return baseRate;
            }

            return oldRate;
        }
        //
        public static bool GetJobList(string _customMapPath) //Search for jobs files in MapPack Folders
        {
            DirectoryInfo dirInfo = new DirectoryInfo(_customMapPath);
            var mapPacks = dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
            if (mapPacks.Length == 0 || mapPacks == null) return false;

            foreach (var pack in mapPacks)
            {
                var _checkPath = Path.Combine(pack.FullName, "mapinfo");
                if (Directory.Exists(_checkPath))
                {
                    var _jobFilePath = Path.Combine(pack.FullName, "mapinfo\\jobs.json");
                    if (!System.IO.File.Exists(_jobFilePath)) continue;
                    System.Collections.Generic.List<MapLoaderParam.JobsParam> _jobList = new System.Collections.Generic.List<MapLoaderParam.JobsParam>();
                    _jobList = MapLoaderParam.DeserializeJobs(_jobFilePath);
                    if (_jobList == null || _jobList.Count <= 0)
                    {
                        MapLoaderPlugin.Log.LogInfo($"Invalid Jobs Json File");
                        continue;
                    }
                    jobsParamDic.Add(pack.Name, _jobList);
                }
            }
            if (jobsParamDic.Count > 0) return true;
            return false;
        }
        public static void GetJobsADV(string advPath)
        {
            if (advPath != "")
            {
                var jobADVFile = Path.Combine(GetCustomMapPath(), advPath);

                if (!System.IO.File.Exists(jobADVFile)) return;

                System.Collections.Generic.List<MapLoaderParam.JobADV> jobADVList = new System.Collections.Generic.List<MapLoaderParam.JobADV>();
                jobADVList = MapLoaderParam.DeserializeJobADV(jobADVFile);
                if (jobADVList == null || jobADVList.Count <= 0)
                {
                    MapLoaderPlugin.Log.LogInfo($"Invalid Jobs Json File");
                    return;
                }

                string assetName = jobADVList[0].ADVAsset;
                jobADVDic.Add(assetName, jobADVList);
            }     
        }
        public static void SetStartingLocationByJob(MapManager mapManager) //Set the ChangeOfClothesMaps > StartMapID List.
        {
            System.Collections.Generic.List<int[]> _startingMapsIDs = new System.Collections.Generic.List<int[]>();
            System.Collections.Generic.List<int[]> changingRoomCustomJobMapsIDs = new System.Collections.Generic.List<int[]>();

            var _changeOfClothesMaps = ThinkingManager.Instance.changeOfClothesMap;
            var _mapList = mapManager.MapListTable;

            if (_changeOfClothesMaps == null) return;

            int _index = 0;
            foreach (var _jobList in jobsParamDic.Values)
            {
                foreach (var _job in _jobList)
                {
                    if (_startingMapsIDs.Count > 0) _startingMapsIDs.Clear();
                    if (changingRoomCustomJobMapsIDs.Count > 0) changingRoomCustomJobMapsIDs.Clear();

                    if (_job == null || _job.JobID < 0) continue;

                    if (_job.CustomJob) //Set Custom Jobs Starting Map Locations.
                    {
                        if (_addedLocationsToIDs.Contains(_job.JobID)) continue;
                        if (_changeOfClothesMaps.ContainsKey(_job.JobID)) continue;

                        //
                        StartMapIDDataParam startMapIDDataParam = new StartMapIDDataParam();
                        startMapIDDataParam.ID = _job.JobID;

                        List<StartMapIDDataParamMap> mapsRates = new List<StartMapIDDataParamMap>();

                        foreach (var _idList in _job.StartingLocation.Values)
                        {
                            if (_idList.Count != 4)
                            {
                                MapLoaderPlugin.Log.LogInfo($"SKIPPED! Period Count must be 4 For: ID: {_job.JobID} Name: {_job.JobName}");
                                break;
                            }
                            foreach (var _ids in _idList)
                            {
                                _startingMapsIDs.Add(_ids);
                            }
                        }

                        foreach (var changingIdList in _job.ClothesChangingMap.Values)
                        {
                            if (changingIdList.Count != 4)
                            {
                                MapLoaderPlugin.Log.LogInfo($"SKIPPED! Period Count must be 4 For: ID: {_job.JobID} Name: {_job.JobName}");
                                break;
                            }
                            foreach (var _ids in changingIdList)
                            {
                                changingRoomCustomJobMapsIDs.Add(_ids);
                            }
                        }

                        for (int i = 0; i < 28; i++)
                        {
                            StartMapIDDataParamMap mapListParam = new StartMapIDDataParamMap();

                            for (int j = 0; j < _startingMapsIDs[i].Length; j++)
                            {
                                mapListParam.First.Add(_startingMapsIDs[i][j]);
                            }
                            for (int j = 0; j < changingRoomCustomJobMapsIDs[i].Length; j++)
                            {
                                mapListParam.Second.Add(changingRoomCustomJobMapsIDs[i][j]);
                            }
                            mapsRates.Add(mapListParam);
                        }

                        if (mapsRates.Count != 28)
                        {
                            MapLoaderPlugin.Log.LogInfo($"Missing elements or more than accepted for StartingLocation {mapsRates.Count} for: ID: {_job.JobID} Name: {_job.JobName}");
                            continue;
                        }
                        startMapIDDataParam.Rates = mapsRates;
                        _changeOfClothesMaps.Add(_job.JobID, startMapIDDataParam);
                    }
                    else //If it has, set new Starting Locations for Vanilla Jobs.
                    {
                        if (!_changeOfClothesMaps.ContainsKey(_job.JobID)) continue;

                        var _startingMapDic = _job.StartingLocation;
                        if (_startingMapDic == null || _startingMapDic.Count != 7)
                        {
                            MapLoaderPlugin.Log.LogInfo($"SKIPPED! StartingLocation Count is less than 7 For: ID: {_job.JobID} Name: {_job.JobName}");
                            continue;
                        }

                        //Add values to a temp List.
                        foreach (var _idList in _job.StartingLocation.Values)
                        {
                            if (_idList.Count != 4)
                            {
                                MapLoaderPlugin.Log.LogInfo($"SKIPPED! Period Count must be 4 For: ID: {_job.JobID} Name: {_job.JobName}");
                                break;
                            }
                            foreach (var _ids in _idList)
                            {
                                _startingMapsIDs.Add(_ids);
                            }
                        }

                        //add the values from temp List if list count is 28.
                        if (_startingMapsIDs.Count == 28)
                        {
                            //Add new starting locations to the Starting Location List (StartMapIDDataParamMap).
                            _index = 0;
                            foreach (var _daysAndPeriods in _changeOfClothesMaps[_job.JobID].Rates)
                            {
                                if (_startingMapsIDs[_index].Length <= 0) continue;
                                for (int i = 0; i < _startingMapsIDs[_index].Length; i++)
                                {
                                    if (!_mapList.ContainsKey(_startingMapsIDs[_index][i]) && _startingMapsIDs[_index][i] >= 0)
                                    {
                                        MapLoaderPlugin.Log.LogInfo($"Invalid Map ID, Map ID is not listed {_startingMapsIDs[_index][i]}");
                                        continue;
                                    }
                                    if (_startingMapsIDs[_index][0] < -1) break; //<- it will not add maps IDs for the day
                                    if (_startingMapsIDs[_index][i] < 0) continue; //<- it will not add maps IDs for this period
                                    if (!_changeOfClothesMaps[_job.JobID].Rates[_index].First.Contains(_startingMapsIDs[_index][i])) _changeOfClothesMaps[_job.JobID].Rates[_index].First.Add(_startingMapsIDs[_index][i]);
                                }
                                _index++;
                            }
                            ThinkingManager.Instance.changeOfClothesMap[_job.JobID] = _changeOfClothesMaps[_job.JobID];
                            //_addedLocationsToIDs.Add(_job.JobID);
                        }
                        else
                        {
                            MapLoaderPlugin.Log.LogInfo($"Missing elements or more than accepted for StartingLocation {_startingMapDic.Count} for: ID: {_job.JobID} Name: {_job.JobName}");
                            MapLoaderPlugin.Log.LogInfo($"Values were not changed");
                        }
                    }
                }
            }
            MapLoaderPlugin.Log.LogInfo($"Loaded Starting Locations");
        }
        public static void SetChangingClothesLocationByJob(MapManager mapManager) //Set the ChangeOfClothesMaps > StartMapID List.
        {
            System.Collections.Generic.List<int[]> _changingRoomMapsIDs = new System.Collections.Generic.List<int[]>();
            var _changeOfClothesMaps = ThinkingManager.Instance.changeOfClothesMap;
            var _mapList = mapManager.MapListTable;

            if (_changeOfClothesMaps == null) return;

            int _index = 0;
            foreach (var _jobList in jobsParamDic.Values)
            {
                foreach (var _job in _jobList)
                {
                    if (_changingRoomMapsIDs.Count > 0) _changingRoomMapsIDs.Clear();
                    if (_job == null || _job.JobID < 0) continue;

                    if (_job.CustomJob) //Set Custom Jobs Changing Clothes Locations.
                    {
                        if (_addedLocationsToIDs.Contains(_job.JobID)) continue;
                        if (_changeOfClothesMaps.ContainsKey(_job.JobID)) continue;

                        //
                        //Missing Code
                        //
                    }
                    else //If it has, set new Changing Clothes Locations for Vanilla Jobs.
                    {
                        //if (_addedLocationsToIDs.Contains(_job.JobID)) continue;
                        if (!_changeOfClothesMaps.ContainsKey(_job.JobID)) continue;

                        var _changingMapDic = _job.ClothesChangingMap;
                        if (_changingMapDic == null || _changingMapDic.Count != 7)
                        {
                            MapLoaderPlugin.Log.LogInfo($"SKIPPED! ClothesChangingMap Count is less than 7 For: ID: {_job.JobID} Name: {_job.JobName}");
                            continue;
                        }

                        //Add values to a temp List.
                        foreach (var _idList in _job.ClothesChangingMap.Values)
                        {
                            if (_idList.Count != 4)
                            {
                                MapLoaderPlugin.Log.LogInfo($"SKIPPED! Period Count must be 4 For: ID: {_job.JobID} Name: {_job.JobName}");
                                break;
                            }
                            foreach (var _ids in _idList)
                            {
                                _changingRoomMapsIDs.Add(_ids);
                            }
                        }

                        //add the values from temp List if list count is 28.
                        if (_changingRoomMapsIDs.Count == 28)
                        {
                            //Add new Changing Clothes locations to the *** List (***).
                            _index = 0;
                            foreach (var _daysAndPeriods in _changeOfClothesMaps[_job.JobID].Rates)
                            {
                                if (_changingRoomMapsIDs[_index].Length <= 0) continue;
                                for (int i = 0; i < _changingRoomMapsIDs[_index].Length; i++)
                                {
                                    if (!_mapList.ContainsKey(_changingRoomMapsIDs[_index][i]) && _changingRoomMapsIDs[_index][i] >= 0)
                                    {
                                        MapLoaderPlugin.Log.LogInfo($"Invalid Map ID, Map ID is not listed {_changingRoomMapsIDs[_index][i]}");
                                        continue;
                                    }
                                    if (_changingRoomMapsIDs[_index][0] < -1) break; //<- it will not add maps IDs for the day
                                    if (_changingRoomMapsIDs[_index][i] < 0) continue; //<- it will not add maps IDs for this period
                                    if (!_changeOfClothesMaps[_job.JobID].Rates[_index].Second.Contains(_changingRoomMapsIDs[_index][i])) _changeOfClothesMaps[_job.JobID].Rates[_index].Second.Add(_changingRoomMapsIDs[_index][i]);
                                }
                                _index++;
                            }
                            ThinkingManager.Instance.changeOfClothesMap[_job.JobID] = _changeOfClothesMaps[_job.JobID];
                            //_addedLocationsToIDs.Add(_job.JobID);
                        }
                        else
                        {
                            MapLoaderPlugin.Log.LogInfo($"Missing elements or more than accepted for StartingLocation {_changingRoomMapsIDs.Count} for: ID: {_job.JobID} Name: {_job.JobName}");
                            MapLoaderPlugin.Log.LogInfo($"Values were not changed");
                        }
                    }
                }
            }
            MapLoaderPlugin.Log.LogInfo($"Loaded Changing Clothes Locations");
        }
        public static void SetJobClothes()
        {
            bool isMoreOutfit = MapLoaderPlugin.GetMoreOutfitsPlugin(false);
            foreach (var jobList in jobsParamDic.Values)
            {
                foreach (var job in jobList)
                {
                    if (job.JobID > 3)
                    {
                        System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<int[]>> choltesIds = new();
                        int indexDay = 0;
                        foreach (var jobclothesDay in job.ChangeOfClothes.Values)
                        {
                            choltesIds.Add(indexDay, jobclothesDay);
                            indexDay++;
                        }

                        if (choltesIds.Count != 7)
                        {
                            MapLoaderPlugin.Log.LogInfo($"Wrong amount of days for ChangeOfClothes for Job:{job.JobName} ID:{job.JobID}");
                            continue;
                        }

                        var jobChangeOfClothes = ThinkingManager.Instance.changeOfClothes;
                        if (jobChangeOfClothes != null)
                        {
                            int indexDic = 0;
                            foreach (var dayClothes in jobChangeOfClothes)
                            {
                                int per = 0;
                                foreach (var periodsClothes in dayClothes.times)
                                {
                                    if (choltesIds[indexDic][per].Length > 0)
                                    {
                                        if (choltesIds[indexDic][per][0] > 2 && isMoreOutfit) periodsClothes.starts.Add(choltesIds[indexDic][per][0]);
                                        else periodsClothes.starts.Add(1);
                                    } 
                                    else
                                    {
                                        periodsClothes.starts.Add(0);
                                        MapLoaderPlugin.Log.LogInfo($"Missing start value for ChangeOfClothes for Job:{job.JobName} ID:{job.JobID}");
                                    }

                                    if (choltesIds[indexDic][per].Length == 2)
                                    {
                                        if (choltesIds[indexDic][per][1] > 2 && isMoreOutfit) periodsClothes.changes.Add(choltesIds[indexDic][per][1]);
                                        else periodsClothes.changes.Add(1);
                                    } 
                                    else
                                    {
                                        periodsClothes.changes.Add(0);
                                        MapLoaderPlugin.Log.LogInfo($"Missing changes value for ChangeOfClothes for Job:{job.JobName} ID:{job.JobID}");
                                    }
                                    per++;
                                }
                                indexDic++;
                            }
                        }
                    }
                }
            }           
        }
        public static void SetJobPeriodADV()
        {
            var workSchedule = GlobalListLoad.Instance.timezoneTrainingTable;
            if (workSchedule == null || workSchedule.Count == 0 || workSchedule.Count > 4) return;

            foreach (var packJobs in jobsParamDic.Values)
            {
                foreach (var jobSchedule in packJobs) //Per Job
                {
                    if (jobSchedule.JobSchedule == null) continue;
                    if (jobSchedule.JobSchedule.Count == 0) continue;

                    TimezoneTrainingDataParam workTimezone = new();
                    System.Collections.Generic.List<bool> periodsADV = new();
                    
                    foreach (var schedule in jobSchedule.JobSchedule.Values)
                    {
                        foreach (var periodADV in schedule)
                        {
                            periodsADV.Add(periodADV);
                        }
                    }

                    if (periodsADV.Count == 0) continue;

                    workTimezone.IsTrainings = new();
                    foreach (var adv in periodsADV)
                    {
                        workTimezone.IsTrainings.Add(adv);
                    }

                    if (workTimezone.IsTrainings.Count > 0 && !workSchedule.ContainsKey(jobSchedule.JobID)) workSchedule.Add(jobSchedule.JobID, workTimezone);
                }
            }
        }
        public static int CheckChangingRoomSex(Actor _chara, int changingRoomID)
        {
            if (MapManager.Instance.MapListTable[changingRoomID].Sex == -1) return changingRoomID;

            var timeZone = SimulationManager.Instance.Mode;
            int newPeriod = MapLoaderUtils.GetTimeZone();

            if (newPeriod == -1) return changingRoomID;

            int sex = _chara.parameter.sex;
            int weekDay = Manager.Game.saveData.Week;
            var changeList = ThinkingManager.Instance.changeOfClothesMap;

            if (changeList.Count == 0) return changingRoomID;

            switch (weekDay)
            {
                case 1:
                    newPeriod += 4;
                    break;
                case 2:
                    newPeriod += 8;
                    break;
                case 3:
                    newPeriod += 12;
                    break;
                case 4:
                    newPeriod += 16;
                    break;
                case 5:
                    newPeriod += 20;
                    break;
                case 6:
                    newPeriod += 24;
                    break;
            }

            if (sex == 0) //Male
            {
                if (MapManager.Instance.MapListTable[changingRoomID].Sex == 1)
                {
                    if (changeList[_chara.gameParameter.Job].Rates[newPeriod].Second.Count > 1)
                    {
                        System.Collections.Generic.List<int> changeMaps = new();
                        foreach (var mapSex in changeList[_chara.gameParameter.Job].Rates[newPeriod].Second)
                        {
                            if (MapManager.Instance.MapListTable[mapSex].Sex == 0) changeMaps.Add(mapSex);
                        }

                        if (changeMaps.Count > 1)
                        {
                            var id = _rnd.Next(0, changeMaps.Count - 1);
                            return changeMaps[id];
                        }
                        else if (changeMaps.Count == 1)
                        {
                            return changeMaps[0];
                        }
                    }
                }
            }
            else if (sex == 1) //Female
            {
                if (MapManager.Instance.MapListTable[changingRoomID].Sex == 0)
                {                   
                    if (changeList[_chara.gameParameter.Job].Rates[newPeriod].Second.Count > 1)
                    {
                        System.Collections.Generic.List<int> changeMaps = new();
                        foreach (var mapSex in changeList[_chara.gameParameter.Job].Rates[newPeriod].Second)
                        {
                            if (MapManager.Instance.MapListTable[mapSex].Sex == 1) changeMaps.Add(mapSex);
                        }

                        if (changeMaps.Count > 1)
                        {
                            var id = _rnd.Next(0, changeMaps.Count - 1);
                            return changeMaps[id];
                        }
                        else if (changeMaps.Count == 1)
                        {
                            return changeMaps[0];
                        }
                    }
                }
            }
            return changingRoomID;
        }
        public static void SetCustomJobs(MapManager _mapManagerInstance)
        {
            System.Collections.Generic.List<int> mapJobsIds = new();
            if (GlobalListLoad.Instance.jobNameTable.Count > 4)
            {
                foreach (var customJobs in jobsParamDic.Values)
                {
                    foreach (var job in customJobs)
                    {
                        if (GlobalListLoad.Instance.jobNameTable.Count > 4 && job.JobID > 3)
                        {
                            if (_mapManagerInstance.MapListTable.ContainsKey(job.JobMapID))
                            {
                                mapJobsIds.Add(job.JobMapID);
                            }
                        }

                        /*if (GlobalListLoad.Instance.jobNameTable.ContainsKey(job.JobID))
                        {
                            continue;
                        }

                        MapLoaderPlugin.Log.LogInfo($"Check");

                        Il2CppSystem.Collections.Generic.List<string> newNames = new();
                        for (int i = 0; i < job.LocalizationNames.Length; i++)
                        {
                            newNames.Add(job.LocalizationNames[i]);
                        }

                        GlobalListLoad.Instance.jobNameTable.Add(job.JobID, newNames);
                        */
                    }
                }

                //Set Job Map
                Il2CppStructArray<int> newGoJobsArray = new Il2CppStructArray<int>(GlobalListLoad.Instance.jobNameTable.Count);

                int index = 0;
                for (int i = 0; i < newGoJobsArray.Length; i++)
                {
                    if (i < 4) newGoJobsArray[i] = _mapManagerInstance.gojobmaps[i];
                    else if (mapJobsIds.Count > index)
                    {
                        newGoJobsArray[i] = mapJobsIds[index];
                        index++;
                    }
                    else newGoJobsArray[i] = 0;
                }

                _mapManagerInstance.gojobmaps = newGoJobsArray;
            }
        }

        /*public static void MakerCustomJobList(JobSelectView _jobSelectView)
        {
            foreach (var jobNamesList in GlobalListLoad.Instance.jobNameTable)
            {
                if (_jobSelectView._jobNameTable.ContainsKey(jobNamesList.Key)) continue;
                
                System.Collections.Generic.List<string> customlist = new();
                foreach (var Name in jobNamesList.Value) 
                {
                    customlist.Add(Name);
                }
                Il2CppSystem.Collections.Generic.IReadOnlyList<string> nameList = _jobSelectView._jobNameTable[0];
                //Il2CppSystem.Collections.Generic.IReadOnlyList<string> readOnlyList = GlobalListLoad.Instance.jobNameTable[0].AsReadOnly();

                _jobSelectView._jobNameTable.Add(jobNamesList.Key, nameList);
            }
        }*/
        public static void CreateCustomJobsSprites(EntryFileInfoComponent _entry)
        {
            var _jobList = GlobalListLoad.Instance.jobNameTable;

            System.Collections.Generic.List<string> spPath = new();

            foreach (var jobsppath in jobsParamDic.Values)
            {
                for (int i = 0; i < jobsppath.Count; i++)
                {
                    var _jobSpFilePath = Path.Combine(GetCustomMapPath(), jobsppath[i].JobSp);
                    spPath.Add(_jobSpFilePath);
                }         
            }

            for (int i = 0; i < _jobList.Count; i++)
            {
                if (spPath.Count == 0 || i < 4) continue;

                Texture2D tex = new Texture2D(2, 2, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB, 1, TextureCreationFlags.None);
                byte[] fileData;
                string spritePath = "";
                if (i < spPath.Count)
                {
                    spritePath = spPath[i];
                }

                if (File.Exists(spritePath))
                {
                    fileData = File.ReadAllBytes(spritePath);
                    tex.LoadImage(fileData);
                    tex.filterMode = FilterMode.Bilinear;
                    tex.Compress(true);

                    Sprite _sp = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0f, 1f), 100.0f);
                    _entry._spWorkIcons = _entry._spWorkIcons.Append(_sp).ToArray();
                    //MapLoaderPlugin.Log.LogDebug($"Added Custom Job Sprite to Roster Array");
                }
                else
                {
                    Assembly moreoutfits = Assembly.GetExecutingAssembly();
                    string resourceOutfits = "SVS_MapLoader.Resources.workIcon_Error.png";

                    byte[] embeded = GetPngResourceAsByteArray(resourceOutfits);
                    if (embeded.Length > 0)
                    {
                        tex.LoadImage(embeded);
                        tex.filterMode = FilterMode.Bilinear;
                        tex.wrapMode = TextureWrapMode.Clamp;

                        Sprite _sp = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0f, 1f), 100.0f);
                        _entry._spWorkIcons = _entry._spWorkIcons.Append(_sp).ToArray();
                    }
                    //MapLoaderPlugin.Log.LogDebug($"ERROR Adding Custom Job SP to Array, Index:{i}");
                } 
            }
        }
        public static void CreateCustomJobsSpritesOnRoster(ImageSprite _imaSp)
        {
            var _jobList = GlobalListLoad.Instance.jobNameTable;

            System.Collections.Generic.List<string> spPath = new();

            foreach (var jobsppath in jobsParamDic.Values)
            {
                for (int i = 0; i < jobsppath.Count; i++)
                {
                    var _jobSpFilePath = Path.Combine(GetCustomMapPath(), jobsppath[i].JobSp);
                    spPath.Add(_jobSpFilePath);
                }
            }

            for (int i = 0; i < _jobList.Count; i++)
            {
                if (spPath.Count == 0 || i < 4) continue;

                Texture2D tex = new Texture2D(2, 2, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB, 1, TextureCreationFlags.None);
                byte[] fileData;

                string spritePath = "";
                if (i < spPath.Count)
                {
                    spritePath = spPath[i];
                }

                if (File.Exists(spritePath) && spPath[i] != "")
                {
                    fileData = File.ReadAllBytes(spritePath);
                    tex.LoadImage(fileData);
                    tex.filterMode = FilterMode.Bilinear;
                    tex.Compress(true);

                    Sprite _sp = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0f, 1f), 100.0f);
                    _imaSp._sprites = _imaSp._sprites.Append(_sp).ToArray();
                    //MapLoaderPlugin.Log.LogDebug($"Added Custom Job Sprite to Profile Array");
                }
                else
                {
                    Assembly moreoutfits = Assembly.GetExecutingAssembly();
                    string resourceOutfits = "SVS_MapLoader.Resources.workIcon_Error.png";

                    byte[] embeded = GetPngResourceAsByteArray(resourceOutfits);
                    if (embeded.Length > 0)
                    {
                        tex.LoadImage(embeded);
                        tex.filterMode = FilterMode.Bilinear;
                        tex.wrapMode = TextureWrapMode.Clamp;

                        Sprite _sp = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0f, 1f), 100.0f);
                        _imaSp._sprites = _imaSp._sprites.Append(_sp).ToArray();
                    }
                    //MapLoaderPlugin.Log.LogDebug($"ERROR Adding Custom Job SP to Array, Index:{i}");
                } 
            }
        }
        public static void CustomJobADV(OpenData _openData)
        {
            bool isJobADV = false;
            switch (_openData.Asset) 
            {
                case "s_86":
                    isJobADV = true;
                    break;
                case "a_25_1":
                    isJobADV = true;
                    break;
                case "p_25_1":
                    isJobADV = true;
                    break;
                case "a_43_1":
                    isJobADV = true;
                    break;
                case "p_43_1":
                    isJobADV = true;
                    break;
                case "timechange":
                    isJobADV = true;
                    break;
            }

            if (isJobADV)
            {
                MapLoaderUtils.InsertJobADV(_openData, jobADVDic);
            }
        }
        //
        public static System.Collections.Generic.List<MapLoaderParam.JobsParam> GetJobsParams()
        {
            System.Collections.Generic.List<MapLoaderParam.JobsParam> jobsParamList = new();
            foreach (var job in jobsParamDic.Values)
            {
                jobsParamList = job;
            }
            return jobsParamList;
        }
        public static System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<MapLoaderParam.MapActionInfoParam>> GetMapActionsParams()
        {
            return mapActionDic;
        }
    }    
}
