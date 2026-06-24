using System;
using System.Collections.Generic;
using System.Linq;
using Manager;
using SaveData;
using SV;

namespace MapLoader
{
    internal class MapLoaderJobs
    {
        public static Random rnd = new();
        public static void JobsFollowMeRestrictions(Actor askingChara,Dictionary<string, List<MapLoaderParam.JobsParam>> customJobs)
        {
            if (askingChara.IsPC) return;           
            Dictionary<string, List<int[]>> jobParamList = new Dictionary<string, List<int[]>>();
            foreach (var jobParam in customJobs.Values)
            {
                if (askingChara.Job < jobParam.Count)
                {
                    if (jobParam[askingChara.Job].ExplorationArea != null) jobParamList = jobParam[askingChara.Job].ExplorationArea;
                    else MapLoaderPlugin.Log.LogInfo($"ExplorationArea is Null for job {jobParam[askingChara.Job].JobID}");
                    break;
                } 
            }

            int chance = rnd.Next(0, 100);
            string day = MapLoaderUtils.GetWeekDay();
            int time = MapLoaderUtils.GetTimeZone();
            int mapID = askingChara.charasGameParam.thinkingPropertyTemp.beTakenMapNo;

            if (MapLoaderPlugin.IsLogEnable()) MapLoaderPlugin.Log.LogInfo($"JobsFollowMeRestrictions -> destination: [{mapID}] from charaID: [{askingChara.charasGameParam.Index}] rng value: [{chance}]");

            if (!jobParamList[day][time].Contains(mapID) && !jobParamList[day][time].Contains(-1))
            {
                if (chance > 1)
                {
                    var newMapID = rnd.Next(0, jobParamList[day][time].Length);
                    askingChara.charasGameParam.thinkingPropertyTemp.beTakenMapNo = jobParamList[day][time][newMapID];
                }
            }
            if (MapLoaderPlugin.IsLogEnable()) MapLoaderPlugin.Log.LogInfo($"JobsFollowMeRestrictions -> New destination: [{askingChara.charasGameParam.thinkingPropertyTemp.beTakenMapNo}]");
        }

        public static void JobCharaAction(BehaviourController charaBehaviour, MapManager.MapTargetInfo mapTarget)
        {
            if (charaBehaviour == null) return;
            if (charaBehaviour.target == null) return;
            if (!MapLoaderPlugin.GetJobInteractableArea()) return;
            if (MapManager.Instance == null) return;
            if (MapManager.Instance.PointInfoTable == null) return;
            
            if (MapLoaderPlugin.IsLogEnable()) MapLoaderPlugin.Log.LogInfo($"Personal Action: targetID {charaBehaviour.target.id} - kind:{charaBehaviour.target.kind.ToString()} - type:{charaBehaviour.target.type} - job:{charaBehaviour.target.job}");
            switch (charaBehaviour.target.kind)
            {
                case BehaviourController.TargetInfo.TargetKind.Map:
                    if (MapLoaderPlugin.IsLogEnable()) MapLoaderPlugin.Log.LogInfo($"Chara Map Action {charaBehaviour.BaseAction.ToString()}");
                    if (charaBehaviour.IsPC && charaBehaviour.target.job == -1) return;
                    var mapID = charaBehaviour.target.id;
                    if (MapManager.Instance != null)
                    {
                        if (MapManager.Instance.MapListTable.ContainsKey(mapID))
                        {
                            if (MapManager.Instance.MapListTable[mapID].Kind == 1) return;
                        }
                    }
                    SetMapAction(charaBehaviour, mapTarget);
                    break;
                case BehaviourController.TargetInfo.TargetKind.Chara:
                    if (MapLoaderPlugin.IsLogEnable()) MapLoaderPlugin.Log.LogInfo($"Chara chara Action {charaBehaviour.BaseAction.ToString()}");
                    SetCharaAction(charaBehaviour);
                    break;
            }          
        }
        private static void SetMapAction(BehaviourController charaBehaviour, MapManager.MapTargetInfo mapTarget)
        {
            if (charaBehaviour.AI.charaData.Job < 4 && (charaBehaviour.target.job == 0 || charaBehaviour.target.job == 1 || charaBehaviour.target.job == 2)) return;
            if (charaBehaviour.AI.chaCtrl.coorde.fileStatus.coordinateType == 2 && charaBehaviour.AI.charaData.CommandNo == 36)
            {
                if (MapManager.Instance.pointInfoTable.ContainsKey(2) && charaBehaviour.target.job == -1)
                {
                    if (MapManager.Instance.PointInfoTable[2].pointList.urouroTable.ContainsKey(-1))
                    {
                        var mapPoints = MapManager.Instance.PointInfoTable[2].pointList.urouroTable[-1].randoms;
                        int newPoint = rnd.Next(0, mapPoints.Count);
                        charaBehaviour.target.SetMap(mapPoints[newPoint], 2, 0, -1);
                    } 
                }
            }

            var explorableAreasDic = new Dictionary<string, List<int[]>>();
            var mapActionList = MapLoader.GetMapActionList();
            var jobParamList = MapLoader.GetJobsParams();

            string day = MapLoaderUtils.GetWeekDay();
            int time = MapLoaderUtils.GetTimeZone();
            //int chance = rnd.Next(0, 100);
            //int targetMapID = charaBehaviour.target.id;
            int typeNum;
            int jobNum = -1;

            foreach (var jobParam in jobParamList)
            {
                if (charaBehaviour.AI.charaData.Job == jobParam.JobID)
                {
                    explorableAreasDic = jobParam.ExplorationArea;
                    break;
                } 
            }
            if (explorableAreasDic[day][time].Contains(charaBehaviour.target.id) || explorableAreasDic[day][time].Contains(-1)) return;

            switch (charaBehaviour.target.type)
            {
                case 0://Urouro
                    if (MapLoaderPlugin.IsLogEnable()) MapLoaderPlugin.Log.LogInfo($"Chara Urouro");
                    typeNum = 0;
                    switch (charaBehaviour.target.job)
                    {
                        case -1://None
                            jobNum = -1;
                            if (MapLoaderPlugin.IsLogEnable()) MapLoaderPlugin.Log.LogInfo($"Urouro Job {jobNum}");
                            int newMapID = rnd.Next(0, explorableAreasDic[day][time].Length);
                            if (MapManager.Instance.pointInfoTable.ContainsKey(explorableAreasDic[day][time][newMapID]))
                            {
                                var mapPoints = MapManager.Instance.PointInfoTable[explorableAreasDic[day][time][newMapID]].pointList.urouroTable[jobNum].randoms;
                                int newPoint = rnd.Next(0, mapPoints.Count);
                                charaBehaviour.target.SetMap(mapPoints[newPoint], explorableAreasDic[day][time][newMapID], typeNum, jobNum);
                            }
                            break;
                        case 0://Meal
                            jobNum = 0;
                            if (MapLoaderPlugin.IsLogEnable()) MapLoaderPlugin.Log.LogInfo($"Urouro Job {jobNum}");
                            foreach (var mealMap in mapActionList)
                            {
                                if (mealMap.EatPlace && explorableAreasDic[day][time].Contains(mealMap.ID))
                                {
                                    if (MapManager.Instance.pointInfoTable.ContainsKey(mealMap.ID))
                                    {
                                        var mapPoints = MapManager.Instance.PointInfoTable[mealMap.ID].pointList.urouroTable[jobNum].randoms;
                                        int newPoint = rnd.Next(0, mapPoints.Count);
                                        charaBehaviour.target.SetMap(mapPoints[newPoint], mealMap.ID, typeNum, jobNum);
                                    }
                                    break;
                                }
                            }
                            break;
                        case 1://Study
                            jobNum = 1;
                            if (MapLoaderPlugin.IsLogEnable()) MapLoaderPlugin.Log.LogInfo($"Urouro Job {jobNum}");
                            /*foreach (var studyMap in mapActionList)
                            {
                                if (studyMap.StudyPlace && explorableAreasDic[day][time].Contains(studyMap.ID))
                                {
                                    if (MapManager.Instance.pointInfoTable.ContainsKey(studyMap.ID))
                                    {
                                        var mapPoints = MapManager.Instance.PointInfoTable[studyMap.ID].pointList.urouroTable[jobNum].randoms;
                                        int newPoint = rnd.Next(0, mapPoints.Count);
                                        charaBehaviour.target.SetMap(mapPoints[newPoint], studyMap.ID, typeNum, jobNum);
                                    }
                                    break;
                                }
                            }*/
                            break;
                        case 2://Motion
                            jobNum = 2;
                            if (MapLoaderPlugin.IsLogEnable()) MapLoaderPlugin.Log.LogInfo($"Urouro Job {jobNum}");
                            /*foreach (var motionMap in mapActionList)
                            {
                                if (motionMap.StudyPlace && explorableAreasDic[day][time].Contains(motionMap.ID))
                                {
                                    if (MapManager.Instance.pointInfoTable.ContainsKey(motionMap.ID))
                                    {
                                        var mapPoints = MapManager.Instance.PointInfoTable[motionMap.ID].pointList.urouroTable[jobNum].randoms;
                                        int newPoint = rnd.Next(0, mapPoints.Count);
                                        charaBehaviour.target.SetMap(mapPoints[newPoint], motionMap.ID, typeNum, jobNum);
                                    }
                                    break;
                                }
                            }*/
                            break;
                        case 4://Bath
                            jobNum = 4;
                            break;
                    }
                    break;
                case 1://Solo
                    typeNum = 1;
                    if (charaBehaviour.target.job == 0) MapLoaderPlugin.Log.LogInfo($"personal type: Solo Eat");
                    if (charaBehaviour.target.job == 1) MapLoaderPlugin.Log.LogInfo($"personal type: Solo Study");
                    if (charaBehaviour.target.job == 2) MapLoaderPlugin.Log.LogInfo($"personal type: Solo Motion");
                    if (charaBehaviour.target.job == 3) MapLoaderPlugin.Log.LogInfo($"personal type: Solo Job");
                    break;
                case 2://With 
                    if (MapLoaderPlugin.IsLogEnable()) MapLoaderPlugin.Log.LogInfo($"Chara With");
                    typeNum = 2;
                    switch (charaBehaviour.target.job)
                    {//To do: Check when charas have different meal locations
                        case -1:
                            break;
                        case 0://Meal
                            jobNum = 0;
                            if (MapLoaderPlugin.IsLogEnable()) MapLoaderPlugin.Log.LogInfo($"With Job {jobNum}");
                            if (charaBehaviour.AI.charaData.CommandNo == 22)
                            {
                                foreach (var mealMap in mapActionList)
                                {
                                    if (mealMap.EatPlace && explorableAreasDic[day][time].Contains(mealMap.ID))
                                    {
                                        if (MapManager.Instance.pointInfoTable.ContainsKey(mealMap.ID))
                                        {
                                            var mapPoints = MapManager.Instance.PointInfoTable[mealMap.ID].pointList.withTable[jobNum].randoms;
                                            int newPoint = rnd.Next(0, mapPoints.Count);
                                            charaBehaviour.target.SetMap(mapPoints[newPoint], mealMap.ID, typeNum, jobNum);
                                            if (mapTarget != null)
                                            {
                                                mapTarget.pInfo = mapPoints[newPoint];
                                                mapTarget.map = mealMap.ID;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }                           
                            break;
                        case 1://Study
                            if (MapLoaderPlugin.IsLogEnable()) MapLoaderPlugin.Log.LogInfo($"With Job {jobNum}");

                            break;
                        case 2://Motion
                            if (MapLoaderPlugin.IsLogEnable()) MapLoaderPlugin.Log.LogInfo($"With Job {jobNum}");

                            break;
                    }
                    if (charaBehaviour.target.job == 0) MapLoaderPlugin.Log.LogInfo($"personal type: With Eat");
                    if (charaBehaviour.target.job == 1) MapLoaderPlugin.Log.LogInfo($"personal type: With Study");
                    if (charaBehaviour.target.job == 2) MapLoaderPlugin.Log.LogInfo($"personal type: With Motion");
                    if (charaBehaviour.target.job == 3) MapLoaderPlugin.Log.LogInfo($"personal type: With Job");
                    break;
                case 3://Everyone
                    if (charaBehaviour.target.job == 0) MapLoaderPlugin.Log.LogInfo($"personal type: Everyone Eat");
                    if (charaBehaviour.target.job == 1) MapLoaderPlugin.Log.LogInfo($"personal type: Everyone Study");
                    if (charaBehaviour.target.job == 2) MapLoaderPlugin.Log.LogInfo($"personal type: Everyone Motion");
                    if (charaBehaviour.target.job == 3) MapLoaderPlugin.Log.LogInfo($"personal type: Everyone Job");
                    break;
                case 4://PC
                    MapLoaderPlugin.Log.LogInfo($"personal type: PC");
                    break;
            }
            if (MapLoaderPlugin.IsLogEnable()) MapLoaderPlugin.Log.LogInfo($"JobPersonalAction -> new Personal Action: target map {charaBehaviour.target.id}");
    
            //if (MapLoaderPlugin.IsLogEnable()) MapLoaderPlugin.Log.LogInfo($"JobPersonalAction -> old Personal Action: target map {charaBehaviour.target.id}");

        }
        private static void SetCharaAction(BehaviourController charaBehaviour)
        {

        }

        public static void JobAnswerConditions(YesNoJudgeManager.AnswerInfo oldAnswerInfo, YesNoJudgeManager.YesNoInfo yesNoInfo, int commandID, int questionCount)
        {

        }
    }
}
