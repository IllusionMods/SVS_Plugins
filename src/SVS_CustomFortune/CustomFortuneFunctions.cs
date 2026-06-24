using System;
using System.Collections.Generic;
using System.IO;
using ADV;
using BepInEx;
using BepInEx.Logging;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Manager;
using SaveData;
using SV;
using SV.H.UI;
using UnityEngine;
using Random = System.Random;

namespace SVS_CustomFortune
{
    internal class CustomFortuneFunctions
    {
        private static readonly Dictionary<int, List<CustomFortuneParam.FortuneParam>> fortunePackDic = new();
        private static readonly Dictionary<int, CustomFortuneParam.FortuneParam> customFortuneDic = new();
        private static readonly Il2CppSystem.Collections.Generic.List<ThinkingManager.ChangeClothInfos> changeClothInfosList = new();
        private static readonly List<int> fortuneIDs = new();
        private static bool loadedDics;
        private static bool loadedEffectsDic;
        //private static int currentFortune = -2;
        private static readonly Random _rnd = new();

        public static void CustomFortunesInit()
        {
            string path = GetCustomFortunePath();
            if (!Directory.Exists(path))
            {
                CustomFortunePlugin.Log.LogInfo($"Could not find CustomFortunes folder at:abdata\\mods\\CustomFortunes");
                return;
            }

            if (fortunePackDic.Count == 0) loadedDics = LoadFortuneFile(path);
            if (!loadedDics)
            {
                CustomFortunePlugin.Log.LogInfo($"Could not load CustomFortunes");
                return;
            }

            if (!loadedEffectsDic)
            {
                SetFortuneEffectsTable();
                loadedEffectsDic = true;
            }
        }

        private static void SetFortuneIDList()
        {
            fortuneIDs.Clear();
            var gameFortunes = GlobalListLoad.Instance._divinationEffectsTable;
            foreach (var gameFortune in gameFortunes.Values)
            {
                if (customFortuneDic.ContainsKey(gameFortune.ID))
                {
                    if (customFortuneDic[gameFortune.ID].Enable)
                    {
                        fortuneIDs.Add(gameFortune.ID);
                        if (customFortuneDic[gameFortune.ID].FortuneRate > 1)
                        {
                            for (int i = 1; i < customFortuneDic[gameFortune.ID].FortuneRate; i++)
                            {
                                if (i > 100) break;
                                fortuneIDs.Add(gameFortune.ID);
                            }
                        }
                    }
                }
                else fortuneIDs.Add(gameFortune.ID);
            }

            //foreach (var customF in customFortuneDic.Values)
            //{
            //    if (!customF.Enable) continue;
            //
            //}
        }
        public static bool IsCustomFortune(string asset, out int fortuneID)
        {
            var trimedText = asset.Replace("board", "");
            fortuneID = -1;
            if (int.TryParse(trimedText, out int validInt))
            {
                if (validInt > 7)
                {
                    fortuneID = validInt;
                    return true;
                }
            }
            return false;
        }
        public static void SetFortuneEffectsTable()
        {
            var fortuneEffectTable = GlobalListLoad.Instance._divinationEffectsTable;
            foreach (var fortunes in fortunePackDic.Values)
            {
                foreach (var fotune in fortunes)
                {
                    if (!fortuneEffectTable.ContainsKey(fotune.ID))
                    {
                        if (!fotune.Enable) continue;
                        var customFortuneEffect = new DivinationEffectsInfoParam();
                        customFortuneEffect.ID = fotune.ID;
                        Il2CppSystem.Collections.Generic.List<float> favorPoints = new();
                        for (int i = 0; i < 4; i++)
                        {
                            favorPoints.Add(fotune.FavorPoints[i]);
                        }
                        customFortuneEffect.Favourables = favorPoints;

                        Il2CppSystem.Collections.Generic.List<int> statesPoints = new();
                        for (int i = 0; i < 10; i++)
                        {
                            statesPoints.Add(fotune.StatesPoints[i]);
                        }
                        customFortuneEffect.States = statesPoints;
                        customFortuneEffect.AddSuccessPoint = 10;
                        GlobalListLoad.Instance._divinationEffectsTable.Add(fotune.ID, customFortuneEffect);
                    }
                }
            }
        }
        public static void SetCustomFortuneScenario(OpenData openData, int _fortuneID)
        {
            if (_fortuneID <= 7) return;
            if (customFortuneDic.Count == 0)
            {
                CustomFortunePlugin.Log.LogInfo($"Custom List Empty");
                return;
            }

            if (customFortuneDic.TryGetValue(_fortuneID, out var customFortune))
            {
                CustomFortunePlugin.Log.LogInfo($"Found Fortune! {customFortune.Name}");

                var scenarioData = new ScenarioData();
                scenarioData._list = new Il2CppReferenceArray<ScenarioCommand>(customFortune.ScenarioParams.Count);
                //----------------------------------------------------------------------------------------------------------
                int index = 0;
                foreach (var param in customFortune.ScenarioParams)
                {
                    //CustomFortunePlugin.Log.LogInfo($"Set Scenario {index}");
                    scenarioData._list[index] = new ScenarioCommand();
                    scenarioData._list[index]._version = 0;
                    scenarioData._list[index]._multi = false;
                    scenarioData._list[index]._command = (Command)Enum.Parse(typeof(Command), param.Command);
                    scenarioData._list[index]._args = new Il2CppStringArray(param.Args.Length);
                    for (int i = 0; i < param.Args.Length; i++)
                    {
                        scenarioData._list[index]._args[i] = param.Args[i];
                    }

                    scenarioData._list[index].Hash = scenarioData._list[index].GetHashCode();
                    index++;
                }
                //----------------------------------------------------------------------------------------------------------
                /*scenarioData._list[0] = new ScenarioCommand();
                scenarioData._list[0]._version = 0;
                scenarioData._list[0]._multi = false;
                scenarioData._list[0]._command = Command.OpenFortuneUI;
                scenarioData._list[0]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(4);
                scenarioData._list[0]._args[0] = "8";
                scenarioData._list[0]._args[1] = "2";
                scenarioData._list[0]._args[2] = "1";
                scenarioData._list[0]._args[3] = "0.5";
                scenarioData._list[0].Hash = scenarioData._list[0].GetHashCode();
                //----------------------------------------------------------------------------------------------------------
                scenarioData._list[1] = new ScenarioCommand();
                scenarioData._list[1]._version = 0;
                scenarioData._list[1]._multi = false;
                scenarioData._list[1]._command = Command.Switch;
                scenarioData._list[1]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(4);
                scenarioData._list[1]._args[0] = "fortuneResult";
                scenarioData._list[1]._args[1] = "0,見る";
                scenarioData._list[1]._args[2] = "1,見ない";
                scenarioData._list[1]._args[3] = "見ない";
                scenarioData._list[1].Hash = scenarioData._list[1].GetHashCode();
                //----------------------------------------------------------------------------------------------------------
                scenarioData._list[2] = new ScenarioCommand();
                scenarioData._list[2]._version = 0;
                scenarioData._list[2]._multi = false;
                scenarioData._list[2]._command = Command.Tag;
                scenarioData._list[2]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(1);
                scenarioData._list[2]._args[0] = "見る";
                scenarioData._list[2].Hash = scenarioData._list[2].GetHashCode();
                //----------------------------------------------------------------------------------------------------------
                scenarioData._list[3] = new ScenarioCommand();
                scenarioData._list[3]._version = 0;
                scenarioData._list[3]._multi = false;
                scenarioData._list[3]._command = Command.EventCGSetting;
                scenarioData._list[3]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(2);
                scenarioData._list[3]._args[0] = "";
                scenarioData._list[3]._args[1] = "EventCG_ea_00";
                scenarioData._list[3].Hash = scenarioData._list[3].GetHashCode();
                //----------------------------------------------------------------------------------------------------------
                scenarioData._list[4] = new ScenarioCommand();

                scenarioData._list[4]._version = 0;
                scenarioData._list[4]._multi = false;
                scenarioData._list[4]._command = Command.Text;
                scenarioData._list[4]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(5);
                scenarioData._list[4]._args[0] = "";
                scenarioData._list[4]._args[1] = "『本日の占い』\r\n～～～恐れずにチャレンジすることが大切。きっと上手くいくはず～～～";
                scenarioData._list[4]._args[2] = "[Today's Fortune]\r\n～～～You will die of ligma.～～～";
                scenarioData._list[4]._args[3] = "『今日的占卜』\r\n～～～拿出勇气挑战是很重要的。肯定会顺利～～～";
                scenarioData._list[4]._args[4] = "『本日運勢』\r\n～～～無所畏懼接受挑戰很重要。凡事勢必都會順利～～～";
                scenarioData._list[4].Hash = scenarioData._list[4].GetHashCode();
                //----------------------------------------------------------------------------------------------------------
                scenarioData._list[5] = new ScenarioCommand();

                scenarioData._list[5]._version = 0;
                scenarioData._list[5]._multi = false;
                scenarioData._list[5]._command = Command.CloseFortuneUI;
                scenarioData._list[5]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(0);
                scenarioData._list[5].Hash = scenarioData._list[5].GetHashCode();
                //----------------------------------------------------------------------------------------------------------
                scenarioData._list[6] = new ScenarioCommand();

                scenarioData._list[6]._version = 0;
                scenarioData._list[6]._multi = false;
                scenarioData._list[6]._command = Command.Jump;
                scenarioData._list[6]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(1);
                scenarioData._list[6]._args[0] = "END";
                scenarioData._list[6].Hash = scenarioData._list[6].GetHashCode();
                //----------------------------------------------------------------------------------------------------------
                scenarioData._list[7] = new ScenarioCommand();

                scenarioData._list[7]._version = 0;
                scenarioData._list[7]._multi = false;
                scenarioData._list[7]._command = Command.Tag;
                scenarioData._list[7]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(1);
                scenarioData._list[7]._args[0] = "見ない";
                scenarioData._list[7].Hash = scenarioData._list[7].GetHashCode();
                //----------------------------------------------------------------------------------------------------------
                scenarioData._list[8] = new ScenarioCommand();

                scenarioData._list[8]._version = 0;
                scenarioData._list[8]._multi = false;
                scenarioData._list[8]._command = Command.CloseFortuneUI;
                scenarioData._list[8]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(0);
                scenarioData._list[8].Hash = scenarioData._list[8].GetHashCode();
                //----------------------------------------------------------------------------------------------------------
                scenarioData._list[9] = new ScenarioCommand();

                scenarioData._list[9]._version = 0;
                scenarioData._list[9]._multi = false;
                scenarioData._list[9]._command = Command.Jump;
                scenarioData._list[9]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(1);
                scenarioData._list[9]._args[0] = "END";
                scenarioData._list[9].Hash = scenarioData._list[9].GetHashCode();
                //----------------------------------------------------------------------------------------------------------
                scenarioData._list[10] = new ScenarioCommand();

                scenarioData._list[10]._version = 0;
                scenarioData._list[10]._multi = false;
                scenarioData._list[10]._command = Command.Tag;
                scenarioData._list[10]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(1);
                scenarioData._list[10]._args[0] = "END";
                scenarioData._list[10].Hash = scenarioData._list[10].GetHashCode();
                //----------------------------------------------------------------------------------------------------------
                scenarioData._list[11] = new ScenarioCommand();

                scenarioData._list[11]._version = 0;
                scenarioData._list[11]._multi = false;
                scenarioData._list[11]._command = Command.Close;
                scenarioData._list[11]._args = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(0);
                scenarioData._list[11].Hash = scenarioData._list[11].GetHashCode();*/
                //----------------------------------------------------------------------------------------------------------
                openData._data = scenarioData;
            }
        }
        private static string GetCustomFortunePath()
        {
            string customFortunePath = Path.Combine(Paths.GameRootPath, "abdata\\mods\\CustomFortunes");
            if (!Directory.Exists(customFortunePath)) CustomFortunePlugin.Log.Log(LogLevel.Message, $"Missing mod or custom fortune folder");

            return customFortunePath;
        }
        private static bool LoadFortuneFile(string customFortunePath) //Search for fortune file in fortunePack folders
        {
            DirectoryInfo dirInfo = new DirectoryInfo(customFortunePath);
            var fortunePacks = dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
            if (fortunePacks.Length == 0) return false;

            int index = 0;
            foreach (var fortunePack in fortunePacks)
            {
                var fortuneFilePath = Path.Combine(fortunePack.FullName, "fortunes.json");
                if (!File.Exists(fortuneFilePath)) continue;
                var fortuneList = CustomFortuneParam.DeserializeFortunes(fortuneFilePath);
                if (fortuneList == null || fortuneList.Count <= 0)
                {
                    CustomFortunePlugin.Log.LogInfo($"Invalid fortune json file");
                    continue;
                }
                var textID = fortunePack.Name.Split('_');
                int dicID = index;
                if (int.TryParse(textID[0], out int validID))
                {
                    dicID = validID;
                }
                fortunePackDic.Add(dicID, fortuneList);
                index++;
            }

            if (customFortuneDic.Count == 0)
            {
                foreach (var dic in fortunePackDic.Values)
                {
                    foreach (var item in dic)
                    {
                        if (!item.Enable) continue;
                        if (item.ID <= 7 || !customFortuneDic.TryAdd(item.ID, item))
                        {
                            CustomFortunePlugin.Log.LogInfo($"Repeated ID for Custom Fortunes: {customFortuneDic[item.ID].Name} and {item.Name}");
                        }
                    }
                }
            }

            if (fortunePackDic.Count > 0 && customFortuneDic.Count > 0) return true;
            return false;
        }

        private static Texture2D LoadPNG(string path)
        {
            Texture2D tex = new Texture2D(2, 2);

            string packPath = GetCustomFortunePath();
            string spritePath = Path.Combine(packPath, path);

            if (File.Exists(spritePath))
            {
                var fileData = File.ReadAllBytes(spritePath);
                tex.LoadImage(fileData);
                tex.filterMode = FilterMode.Bilinear;
                tex.Compress(true);
                return tex;
            }
            CustomFortunePlugin.Log.LogInfo($"Could not load sprite at: {path}");

            return null;
        }
        public static void SetCustomFortuneSpriteAndText(FortuneUI _fortune, int _fortuneID)
        {
            var sheetTran = _fortune.transform.Find("Panel/imgResult/anm2dIcon");
            if (sheetTran == null) return;

            var sheetAnimTwoD = sheetTran.GetComponent<Animation2D>();
            if (_fortuneID < 8)
            {
                if (sheetAnimTwoD.FPS != 1) sheetAnimTwoD.FPS = 1;
                return;
            }

            //var sheetImage = sheetTran.GetComponent<Image>();
            _fortune._txtLuck.text = customFortuneDic[_fortuneID].ShortMessage;
            if (sheetAnimTwoD != null)
            {
                sheetAnimTwoD.Sheet = new Il2CppReferenceArray<Sprite>(customFortuneDic[_fortuneID].SpritesPath.Length);

                int index = 0;
                foreach (var spritePath in customFortuneDic[_fortuneID].SpritesPath)
                {
                    var spriteTexture = LoadPNG(spritePath);
                    if (spriteTexture != null)
                    {
                        Sprite sprite = Sprite.Create(spriteTexture, new Rect(0.0f, 0.0f, spriteTexture.width, spriteTexture.height), new Vector2(0f, 1f), 100.0f);
                        sheetAnimTwoD.Sheet[index] = sprite;
                    }
                    else CustomFortunePlugin.Log.LogInfo($"Failed to set sprite, file is missing or wrong path");
                    index++;
                }

                if (customFortuneDic[_fortuneID].SpriteAnimFPS > 1)
                {
                    sheetAnimTwoD.FPS = customFortuneDic[_fortuneID].SpriteAnimFPS;
                }
                else if (sheetAnimTwoD.FPS != 1) sheetAnimTwoD.FPS = 1;
            }
        }
        public static void RandomizeFortuneIDs()
        {
            SetFortuneIDList();
            if (fortuneIDs.Count == 0) return;
            int select = _rnd.Next(0, fortuneIDs.Count - 1);
            Game.saveData.dataCount.circularNotice = fortuneIDs[select];
            CustomFortunePlugin.Log.LogInfo($"Fortune randomizer result: {Game.saveData.dataCount.circularNotice}");
        }
        public static void SetFortuneFavorability(Il2CppStructArray<int> favors)
        {
            var isFortuneActive = Game.saveData.dataCount.isCircularNoticeUse;
            if (isFortuneActive)
            {
                var circularID = Game.saveData.dataCount.circularNotice;
                if (circularID > 7)
                {
                    if (!customFortuneDic[circularID].Enable) return;
                    favors[0] = (int)(favors[0] * customFortuneDic[circularID].FavorPoints[0]);
                    favors[1] = (int)(favors[1] * customFortuneDic[circularID].FavorPoints[1]);
                    favors[2] = (int)(favors[2] * customFortuneDic[circularID].FavorPoints[2]);
                    favors[3] = (int)(favors[3] * customFortuneDic[circularID].FavorPoints[3]);
                }
            }
        }
        public static void SetFortuneStatesPoints(Il2CppSystem.Collections.Generic.List<int> states)
        {
            var isFortuneActive = Game.saveData.dataCount.isCircularNoticeUse;
            if (isFortuneActive)
            {
                var circularID = Game.saveData.dataCount.circularNotice;
                if (circularID > 7)
                {
                    if (!customFortuneDic[circularID].Enable) return;

                    for (var i = 0; i < customFortuneDic[circularID].StatesPoints.Count; i++)
                    {
                        states[i] += customFortuneDic[circularID].StatesPoints[i];
                        if (states[i] < 0) states[i] = 0;
                    }
                }
            }
        }
        //public static void SetFortuneSuccessPoints()
        //{
        //    var isFortuneActive = Manager.Game.saveData.dataCount.isCircularNoticeUse;
        //    if (isFortuneActive)
        //    {
        //        var circularID = Manager.Game.saveData.dataCount.circularNotice;
        //        if (circularID > 7)
        //        {
        //            if (!customFortuneDic[circularID].Enable) return;
        //        }
        //    }
        //}

        public static void FortuneAnswerRate(YesNoJudgeManager.AnswerInfo answerInfo, YesNoJudgeManager.YesNoInfo yesNoInfo, int _commandID, int _questionCount)
        {
            if (answerInfo.ans > 1) return;
            var isFortuneActive = Game.saveData.dataCount.isCircularNoticeUse;
            if (isFortuneActive)
            {
                var circularID = Game.saveData.dataCount.circularNotice;
                if (circularID > 7)
                {
                    if (!customFortuneDic[circularID].Enable || customFortuneDic[circularID].AddSuccessPoint == 0) return;
                    if (customFortuneDic[circularID].ActionsCommands.CharaActionsAnswers.Contains(_commandID) || customFortuneDic[circularID].ActionsCommands.CharaActionsAnswers.Count == 0)
                    {
                        if (answerInfo.rate > 0)
                        {
                            float rate = answerInfo.rate;
                            int successPoint = customFortuneDic[circularID].AddSuccessPoint;
                            if (successPoint < -100) successPoint = -100;
                            if (successPoint > 100) successPoint = 100;

                            answerInfo.rate = rate * (1 + ((float)successPoint / 100));
                            if (answerInfo.rate >= 100) answerInfo.ans = 0;
                            else if (answerInfo.rate == 0) answerInfo.ans = 1;
                            else
                            {
                                var chance = _rnd.Next(1, 100);
                                answerInfo.ans = chance <= answerInfo.rate ? 0 : 1;
                            }
                        }
                    }
                }
            }
        }

        public static void CreateOldChangeOfClothesList()
        {
            if (changeClothInfosList.Count > 0) return;
            var oldChangeOfClothesList = ThinkingManager.Instance.changeOfClothes;
            foreach (var day in oldChangeOfClothesList)
            {
                ThinkingManager.ChangeClothInfos ccis = new ThinkingManager.ChangeClothInfos();
                foreach (var period in day.times)
                {
                    ThinkingManager.ChangeClothInfo cci = new ThinkingManager.ChangeClothInfo();

                    Il2CppSystem.Collections.Generic.List<int> outfitsStartsValues = new();
                    foreach (var value in period.starts)
                    {
                        outfitsStartsValues.Add(value);
                    }

                    Il2CppSystem.Collections.Generic.List<int> outfitsChangesValues = new();
                    foreach (var value in period.changes)
                    {
                        outfitsChangesValues.Add(value);
                    }

                    if (outfitsStartsValues.Count > 0) cci.starts = outfitsStartsValues;
                    if (outfitsChangesValues.Count > 0) cci.changes = outfitsChangesValues;
                    ccis.times.Add(cci);
                }
                changeClothInfosList.Add(ccis);
            }
        }
        public static void ChangeOutfit(Actor chara, bool _isStart, int timeOfDay)
        {
            if (timeOfDay >= 0)
            {
                if (Game.saveData.dataCount.circularNotice > 7 && Game.saveData.dataCount.isCircularNoticeUse)
                {
                    var fortuneID = Game.saveData.dataCount.circularNotice;
                    if (customFortuneDic[fortuneID].Outfits.UseOutfit)
                    {
                        int weekDay = Game.saveData.Week;
                        var dayList = ThinkingManager.Instance.changeOfClothes;

                        int startOutfitID = dayList[weekDay].times[timeOfDay].starts[chara.Job];
                        if ((startOutfitID != 1 && startOutfitID != 2) || (chara.Job == 0 && startOutfitID != 2))
                        {
                            dayList[weekDay].times[timeOfDay].starts[chara.Job] = 5;//Set 05:Lewd Outfit
                        }

                        int changeOutfitID = dayList[weekDay].times[timeOfDay].changes[chara.Job];
                        if ((changeOutfitID != 1 && changeOutfitID != 2 && changeOutfitID != 5) || (chara.Job == 0 && changeOutfitID != 2 && changeOutfitID != 5))
                        {
                            dayList[weekDay].times[timeOfDay].changes[chara.Job] = 5;//Set 05:Lewd Outfit Later
                        }
                    }
                }
                //Force Change Outfit
                //chara.chaCtrl.coorde.SetNowCoordinate(chara.chaCtrl.data.Coordinates[4]);
                //chara.chaCtrl.coorde.fileStatus.coordinateType = 4;
                //chara.chaCtrl.ReloadCoordinate();
            }
        }

        public static int SetCharaTarget(Actor actor, int commandID, int targetID)
        {
            var circularID = Game.saveData.dataCount.circularNotice;
            if (customFortuneDic.ContainsKey(circularID))
            {
                if (customFortuneDic[circularID].ActionsCommands.ReduceActionCommandRate == null) return targetID;

                if (customFortuneDic[circularID].ActionsCommands.ReduceActionCommandRate.ContainsKey(commandID))
                {
                    int value = customFortuneDic[circularID].ActionsCommands.ReduceActionCommandRate[commandID];
                    var chance = _rnd.Next(0, 100);
                    if (chance <= value) return -1;
                }
            }
            return targetID;
        }
    }
}
