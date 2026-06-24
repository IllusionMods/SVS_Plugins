using ADV;
using Il2CppSystem.Collections.Generic;
using Manager;
using SaveData;
using SV;
using SV.Chara;
using SV.H;
using SV.MyRoomScene;
using SVS_SixthSense.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Scene = UnityEngine.SceneManagement.Scene;
namespace SVS_SixthSense
{
    public class SixthSense
    {
        public static GameObject sixthSenseObj;
        public static GameObject moodSensorObj;
        private static GameObject advMoodSensorObj;
        private static Material SixthSenseMat;

        private static AI currentCharaAI;

        private static StateParameter.StateKind currentMood = StateParameter.StateKind.TOTAL;

        private static string currentName = "";
        private static bool charaFound;
        public static void MakeCanvas(Scene scene, SimulationScene _instance)
        {
            // Creating Canvas object
            var customGameObj = new GameObject("SixthSense");
            SceneManager.MoveGameObjectToScene(customGameObj, scene);
            var canvasScaler = customGameObj.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            Canvas canvas = customGameObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 51;

            sixthSenseObj = new GameObject("senseText");
            sixthSenseObj.transform.SetParent(customGameObj.transform);

            int fontSize = 20;

            var favorRect = sixthSenseObj.AddComponent<RectTransform>();
            favorRect.anchoredPosition = new Vector2(-540, 300);
            favorRect.anchorMax = new Vector2(0.5f, 0.5f);
            favorRect.anchorMin = new Vector2(0.5f, 0.5f);
            favorRect.pivot = new Vector2(0.5f, 0.5f);
            favorRect.sizeDelta = new Vector2(800, 200);

            var favorInfo = sixthSenseObj.AddComponent<TextMeshProUGUI>();
            favorInfo.font = SixthSenseLoader.GetResource<TMP_FontAsset>("tmp_sv_default");

            var colorList = SixthSensePlugin.GetTextColor();

            var tempMat = SixthSenseLoader.GetResource<Material>("tmp_sv_default Material");
            if (SixthSenseMat == null)
            {
                SixthSenseMat = new Material(tempMat);
                SixthSenseMat.name = "tmp_sv_default SixthSenseMat";
                SixthSenseMat.SetColor("_UnderlayColor", colorList[1]);
                SixthSenseMat.SetFloat("_UnderlayDilate", 0.7f);
                SixthSenseMat.EnableKeyword("UNDERLAY_ON");
            }
            favorInfo.fontMaterial = SixthSenseMat;

            favorInfo.fontSize = fontSize;
            favorInfo.alignment = TextAlignmentOptions.TopLeft;
            favorInfo.overflowMode = TextOverflowModes.Overflow;
            favorInfo.enableWordWrapping = true;
            favorInfo.color = colorList[0];
            favorInfo.text = "";

            customGameObj.transform.SetParent(_instance.transform);
            SixthSensePlugin.Log.LogInfo($"SixthSense Canvas Created");
        }
        public static void CreateCharaMoodImage(SimulationScene simInstance)
        {
            var charaInfo = simInstance.transform.Find("Canvas/MainCanvas/GameCanvas/CharaInfo");
            if (charaInfo != null)
            {
                var charaMoodTransform = simInstance.transform.Find("Canvas/MainCanvas/GameCanvas/CharaInfo/CharaMood");
                if (charaMoodTransform == null)
                {
                    moodSensorObj = new GameObject("CharaMood");
                    moodSensorObj.layer = 5;
                    moodSensorObj.AddComponent<RectTransform>();
                    moodSensorObj.AddComponent<CanvasRenderer>();
                    moodSensorObj.AddComponent<Image>();

                    moodSensorObj.transform.SetParent(charaInfo.transform);

                    var charaMoodRect = moodSensorObj.GetComponent<RectTransform>();
                    charaMoodRect.anchoredPosition = new Vector2(0f, 0f);
                    charaMoodRect.localScale = new Vector3(1, 1, 1);//Reset Local scale
                    charaMoodRect.sizeDelta = new Vector2(55, 80);
                    charaMoodRect.position = new Vector3(45f, 980f, 0);
                    charaMoodRect.pivot = new Vector2(0.5f, 0.5f);
                    charaMoodRect.localPosition = new Vector3(50f, -100f, 0);

                    var charaMoodImage = moodSensorObj.GetComponent<Image>();
                    charaMoodImage.sprite = SixthSenseLoader.GetMoodSprites(-1);
                }              
            }
            else SixthSensePlugin.Log.LogInfo($"Transform CharaInfo not found, failed to create CharaMood gameObject");
        }
        public static void CreateCharaMoodImageInADV(CommandUI commandUIInstance)
        {           
            var charaADVCanvas = commandUIInstance.transform.Find("Canvas");
            if (charaADVCanvas != null)
            {
                var charaMoodTransform = commandUIInstance.transform.Find("Canvas/CharaMood");
                if (charaMoodTransform != null) return;
                advMoodSensorObj = new GameObject("CharaMood");
                advMoodSensorObj.layer = 5;
                advMoodSensorObj.AddComponent<RectTransform>();
                advMoodSensorObj.AddComponent<CanvasRenderer>();
                advMoodSensorObj.AddComponent<Image>();

                advMoodSensorObj.transform.SetParent(charaADVCanvas.transform);

                var charaMoodRect = advMoodSensorObj.GetComponent<RectTransform>();
                charaMoodRect.anchoredPosition = new Vector2(0f, 0f);
                charaMoodRect.localScale = new Vector3(1, 1, 1);//Reset Local scale
                charaMoodRect.sizeDelta = new Vector2(69, 100);
                charaMoodRect.position = new Vector3(90f, 1005f, 0);
                charaMoodRect.pivot = new Vector2(0.5f, 0.5f);
                charaMoodRect.localPosition = new Vector3(-870f, 465f, 0);

                var charaMoodImage = moodSensorObj.GetComponent<Image>();
                charaMoodImage.sprite = SixthSenseLoader.GetMoodSprites(-1);
            }
            else SixthSensePlugin.Log.LogInfo($"Transform CommandUI(Clone)/Canvas not found, failed to create CharaMood gameObject");
        }
        private static bool CheckDisplay()
        {
            var isHScene = ADVManager._instance.IsHScene || (HScene._instance != null);
            var isOpen = isHScene || ADVManager._instance.IsADV
                || (MyRoom._instance != null && MyRoom._instance.IsOpen());
            if (isOpen) return false;
            return true;
        }
        public static void ShowHavingSexText(IReadOnlyList<Actor> readOnlyList)
        {
            if (readOnlyList == null) return;
            if (!CheckDisplay()) return;
            readOnlyList.TryGet(0, out Actor actorOne);
            readOnlyList.TryGet(1, out Actor actorTwo);
            var charaOne = actorOne;
            var charaTwo = actorTwo;
            int mapID = -1;
            if (charaOne != null && charaTwo != null)
            {
                if (charaOne.charasGameParam.isPC || charaTwo.charasGameParam.isPC) return;
                if (charaOne.charaBase != null)
                {
                    if (charaOne.charaBase.BehaviourCtrl != null) mapID = charaOne.charaBase.BehaviourCtrl.nowMapID;
                }               
                string mapName = "Unknown Location";
                if (MapManager.Instance.MapListTable.ContainsKey(mapID))
                {
                    mapName = MapManager.Instance.MapListTable[mapID].Name;
                }
                var tmpText = sixthSenseObj.GetComponent<TMP_Text>();
                var currentColor = SixthSensePlugin.GetTextColor();
                tmpText.color = currentColor[0];
                tmpText.fontSharedMaterial.SetColor("_UnderlayColor", currentColor[1]);
                var options = SixthSensePlugin.GetSettings();
                if (options[0])
                {
                    if (tmpText.text != "") tmpText.text += "\nSomeone is having sex in " + mapName;
                    else tmpText.text = "Someone is having sex in " + mapName;
                }
                else
                {
                    if (tmpText.text != "") tmpText.text += "\n" + actorOne.Name + " and " + actorTwo.Name + "\n" + "are having sex in " + mapName;
                    else tmpText.text = actorOne.Name + " and " + actorTwo.Name + "\n" + "are having sex in " + mapName;
                }
                tmpText.gameObject.SetActive(true);

                /*var tempAiList = SimulationScene.Instance.tempAIs;
                if (tempAiList != null)
                {
                    foreach (var chara in tempAiList)
                    {
                        if (chara.charaData.charasGameParam.Index == charaOne.charasGameParam.Index)
                        {
                            mapID = chara.BehaviourCtrl.NowMapID;
                            break;
                        }
                    }
                    
                }*/
            }
            else SixthSensePlugin.Log.LogInfo("Chara not found");
        }
        public static void ShowFightingText(IReadOnlyList<Actor> readOnlyList)
        {
            if (readOnlyList == null) return;
            if (!CheckDisplay()) return;
            readOnlyList.TryGet(0, out Actor actorOne);
            readOnlyList.TryGet(1, out Actor actorTwo);
            var charaOne = actorOne;
            var charaTwo = actorTwo;
            int mapID = -1;
            if (charaOne != null && charaTwo != null)
            {
                if (charaOne.charasGameParam.isPC || charaTwo.charasGameParam.isPC) return;
                if (charaOne.charaBase != null)
                {
                    if (charaOne.charaBase.BehaviourCtrl != null) mapID = charaOne.charaBase.BehaviourCtrl.nowMapID;
                }
                string mapName = "Unknown Location";
                if (MapManager.Instance.MapListTable.ContainsKey(mapID))
                {
                    mapName = MapManager.Instance.MapListTable[mapID].Name;
                }
                var tmpText = sixthSenseObj.GetComponent<TMP_Text>();
                var currentColor = SixthSensePlugin.GetTextColor();
                tmpText.color = currentColor[0];
                tmpText.fontSharedMaterial.SetColor("_UnderlayColor", currentColor[1]);
                var options = SixthSensePlugin.GetSettings();
                if (options[0])
                {
                    if (tmpText.text != "") tmpText.text += "\nThere is a fight happening at " + mapName;
                    else tmpText.text = "There is a fight happening at " + mapName;
                }
                else
                {
                    if (tmpText.text != "") tmpText.text += "\n" + actorOne.Name + " and " + actorTwo.Name + "\n" + "are fighting in " + mapName;
                    else tmpText.text = actorOne.Name + " and " + actorTwo.Name + "\n" + "are fighting in " + mapName;
                }
                tmpText.gameObject.SetActive(true);

                /*var tempAiList = SimulationScene.Instance.tempAIs;
                if (tempAiList != null)
                {
                    foreach (var chara in tempAiList)
                    {
                        if (chara.charaData.charasGameParam.Index == charaOne.charasGameParam.Index)
                        {
                            mapID = chara.BehaviourCtrl.NowMapID;
                            break;
                        }
                    }
                    
                }*/
            }
            else SixthSensePlugin.Log.LogInfo("Chara not found");
        }
        public static void ShowCommandText(Actor passive, Actor active, int commandID)
        {
            if (!CheckDisplay()) return;
            if (passive != null && active != null)
            {
                if (commandID == 78 || commandID == 79 || commandID == 80 || commandID == 81)
                {
                    var tmpText = sixthSenseObj.GetComponent<TMP_Text>();
                    var currentColor = SixthSensePlugin.GetTextColor();
                    tmpText.color = currentColor[0];
                    tmpText.fontSharedMaterial.SetColor("_UnderlayColor", currentColor[1]);
                    var options = SixthSensePlugin.GetSettings();
                    if (options[0])
                    {
                        if (tmpText.text != "") tmpText.text += "\nSomeone is getting blackmailed!";
                        else tmpText.text = "Someone is getting blackmailed!";
                    }
                    else 
                    {
                        if (tmpText.text != "") tmpText.text += "\n" + passive.Name + " is getting blackmailed by " + active.Name;
                        else tmpText.text = passive.Name + " is getting blackmailed by " + active.Name;
                    }
                    
                    tmpText.gameObject.SetActive(true);
                }
            }
        }
        public static void HideText()
        {
            var tmpText = sixthSenseObj.GetComponent<TMP_Text>();
            tmpText.text = "";
            tmpText.gameObject.SetActive(false);
        }
        public static void ShowNPCMasturbatingText(BehaviourController charaCtrl)
        {
            if (!CheckDisplay()) return;
            if (charaCtrl != null)
            {
                string mapName = "Unknown Location";
                int mapID = charaCtrl.nowMapID;
                if (MapManager.Instance.MapListTable.ContainsKey(mapID))
                {
                    mapName = MapManager.Instance.MapListTable[mapID].Name;
                }
                Actor chara = charaCtrl.ai?.charaData;
                if (chara != null)
                {
                    var tmpText = sixthSenseObj.GetComponent<TMP_Text>();
                    var currentColor = SixthSensePlugin.GetTextColor();
                    tmpText.color = currentColor[0];
                    tmpText.fontSharedMaterial.SetColor("_UnderlayColor", currentColor[1]);

                    var options = SixthSensePlugin.GetSettings();
                    if (options[0])
                    {
                        if (tmpText.text != "") tmpText.text += "\nSomeone is masturbating in " + mapName;
                        else tmpText.text = "Someone is masturbating in " + mapName;
                    }
                    else
                    {
                        if (tmpText.text != "") tmpText.text += "\n" + chara.Name + " is masturbating in " + mapName;
                        else tmpText.text = chara.Name + " is masturbating in " + mapName;
                    }
                    tmpText.gameObject.SetActive(true);
                }
            }
            else SixthSensePlugin.Log.LogInfo("Chara controller not found");
        }

        public static void ShowCharacterMood(string charaName)
        {
            if (moodSensorObj != null)
            {
                if (currentCharaAI == null)
                {
                    if (SimulationScene.Instance != null)
                    {
                        if (SimulationScene.Instance.tempAIs != null)
                        {                            
                            foreach (var tempAI in SimulationScene.Instance.tempAIs)
                            {
                                if (!tempAI.charaData.IsPC && tempAI.objCircle.activeSelf)
                                {
                                    currentCharaAI = tempAI;
                                    currentMood = currentCharaAI.charaData.charasGameParam.state.State;
                                    //if (charaName != currentCharaAI.charaData.parameter.fullname)
                                    //{
                                    //    var aiName = currentCharaAI.charaData.parameter.fullname;
                                    //}
                                    SetMoodSprite(moodSensorObj, currentCharaAI.charaData);
                                    if (SixthSensePlugin.IsDebugLog()) SixthSensePlugin.Log.LogInfo("Setting Chara Mood. First Time");
                                    break;
                                }
                            }
                        }
                    }
                }

                if (currentCharaAI != null)
                {
                    if (!currentCharaAI.objCircle.activeSelf)
                    {                       
                        if (SimulationScene.Instance != null)
                        {
                            if (SimulationScene.Instance.tempAIs != null)
                            {
                                if (SimulationScene.Instance.tempAIs.Count == 0) currentCharaAI = null;
                                if (charaName != currentName)
                                {
                                    currentName = charaName;
                                    charaFound = false;
                                    foreach (var tempAI in SimulationScene.Instance.tempAIs)
                                    {
                                        if (!tempAI.charaData.IsPC && tempAI.objCircle.activeSelf)
                                        {
                                            currentCharaAI = tempAI;
                                            currentMood = currentCharaAI.charaData.charasGameParam.state.State;
                                            SetMoodSprite(moodSensorObj, currentCharaAI.charaData);
                                            charaFound = true;
                                            if (SixthSensePlugin.IsDebugLog()) SixthSensePlugin.Log.LogInfo("Changing AI and Chara Mood on Map");
                                            break;
                                        }
                                    }
                                    if (!charaFound)
                                    {
                                        currentMood = StateParameter.StateKind.TOTAL;
                                        SetMoodSprite(moodSensorObj, currentCharaAI?.charaData);
                                        if (SixthSensePlugin.IsDebugLog()) SixthSensePlugin.Log.LogInfo("Current AI not found on Map and not selected AI");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (charaName != currentCharaAI.charaData.parameter.fullname)
                        {
                            if (currentName != charaName)
                            {
                                currentName = charaName;
                                //var aiName = currentCharaAI.charaData.parameter.fullname;
                                currentMood = StateParameter.StateKind.TOTAL;
                                SetMoodSprite(moodSensorObj, currentCharaAI.charaData);
                                if (SixthSensePlugin.IsDebugLog()) SixthSensePlugin.Log.LogInfo("Mouse Hover, Changing Mood to Unknown");

                            }
                        }
                        else if (currentCharaAI.charaData.charasGameParam.state.State != currentMood)
                        {
                            currentMood = currentCharaAI.charaData.charasGameParam.state.State;
                            currentName = currentCharaAI.charaData.parameter.fullname;
                            SetMoodSprite(moodSensorObj, currentCharaAI.charaData);
                            if (SixthSensePlugin.IsDebugLog()) SixthSensePlugin.Log.LogInfo("Updating Mood");
                        }
                    }
                }
            }
        }

        public static void ShowCharaMoodInADV(AI npc)
        {
            currentMood = npc.charaData.charasGameParam.state.State;
            SetMoodSprite(advMoodSensorObj, npc.charaData);
        }
        private static void SetMoodSprite(GameObject moodSensorObject, Actor currentActor)
        {
            if (moodSensorObject != null)
            {
                var moodSp = moodSensorObject.GetComponent<Image>();
                if (moodSp != null)
                {
                    if (CheckMoodConditions(currentActor))
                    {
                        if (SixthSensePlugin.IsDebugLog()) SixthSensePlugin.Log.LogInfo($"Conditions met. Loading Mood Sprite for MoodType: {currentMood.ToString()}");
                        switch (currentMood)
                        {
                            case StateParameter.StateKind.UPLIFT:
                                moodSp.sprite = SixthSenseLoader.GetMoodSprites(0);
                                break;
                            case StateParameter.StateKind.SHYNESS:
                                moodSp.sprite = SixthSenseLoader.GetMoodSprites(1);
                                break;
                            case StateParameter.StateKind.JEALOUSY:
                                moodSp.sprite = SixthSenseLoader.GetMoodSprites(2);
                                break;
                            case StateParameter.StateKind.ANGER:
                                moodSp.sprite = SixthSenseLoader.GetMoodSprites(3);
                                break;
                            case StateParameter.StateKind.DISAPPOINTMENT:
                                moodSp.sprite = SixthSenseLoader.GetMoodSprites(4);
                                break;
                            case StateParameter.StateKind.PEACEOFMIND:
                                moodSp.sprite = SixthSenseLoader.GetMoodSprites(5);
                                break;
                            case StateParameter.StateKind.RUT:
                                moodSp.sprite = SixthSenseLoader.GetMoodSprites(6);
                                break;
                            case StateParameter.StateKind.EARNESTNESS:
                                moodSp.sprite = SixthSenseLoader.GetMoodSprites(7);
                                break;
                            case StateParameter.StateKind.TENSION:
                                moodSp.sprite = SixthSenseLoader.GetMoodSprites(8);
                                break;
                            case StateParameter.StateKind.NORMAL:
                                moodSp.sprite = SixthSenseLoader.GetMoodSprites(9);
                                break;
                            default:
                                moodSp.sprite = SixthSenseLoader.GetMoodSprites(-1);
                                break;
                        }
                    }
                    else
                    {
                        if (SixthSensePlugin.IsDebugLog()) SixthSensePlugin.Log.LogInfo($"Conditions not met, setting Mood to Unknown");
                        moodSp.sprite = SixthSenseLoader.GetMoodSprites(-1);
                    }
                }
            }
        }
        private static bool CheckMoodConditions(Actor currentActor)
        {
            if (currentActor != null && GameChara.Player != null)
            {
                if (GameChara.Player.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(currentActor.charasGameParam.Index))
                {
                    var charaSenTable = GameChara.Player.charasGameParam.sensitivity.tableFavorabiliry[currentActor.charasGameParam.Index];
                    if (charaSenTable.longSensitivityCounts[0] > 10 || charaSenTable.longSensitivityCounts[1] > 10) return true;
                }
            }
            return false;
        }
        public static void ShowMood(bool show)
        {
            moodSensorObj.SetActive(show);
            advMoodSensorObj.SetActive(show);        
        }

        private static void Test()
        {
        }
    }
}
