using Manager;
using SV;
using SV.CharaSelectScene;
using SV.CorrelationDiagramScene;
using SV.EntryScene;
using SVS_ExpandCharacterRoster.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SVS_ExpandCharacterRoster
{
    internal class ExpandCharacterRoster
    {
        private static GameObject expandObj;
        private static GameObject charaNumText;

        private static int currentMax = 0;

        private static bool setPlayer = false;

        private static bool restoreCharaRoster = false;
        private static bool restoreCharaSelect = false;
        private static bool restoreCharaCorre = false;

        private static bool setWhenOpen = true;

        private static bool setCharaSelectMenu = false;
        private static bool setCorrelationMenu = false;

        private static bool isSimRoomToMorning = false;
        private static bool isSimMorning = false;
        public static void ResetValues()
        {
            currentMax = 0;
            restoreCharaRoster = false;
            setCharaSelectMenu = false;
            setCorrelationMenu = false;
            isSimRoomToMorning = false;
            isSimMorning = false;
            setWhenOpen = true;
        }
        public static void SetCharaDisplayMenus(SimulationScene simulation)
        {
            if (simulation == null) return;
            if (CharaEntry.Instance != null) //Set Home Menu
            {
                if (CharaEntry.Instance._isOpen)
                {
                    if (Game.saveData == null) return;
                    var charaLimit = ExpandCharacterRosterPlugin.GetCharaLimit();
                    if (Game.saveData.CharaEntryMode > 0 && charaLimit >= 24)
                    {
                        if (expandObj != null) expandObj.SetActive(true);
                        if (currentMax != charaLimit || setWhenOpen)
                        {
                            if (ExpandCharacterRosterPlugin.IsDebugLog()) ExpandCharacterRosterPlugin.Log.LogInfo($"Current Max: {charaLimit}");
                            setWhenOpen = false;
                            restoreCharaRoster = true;
                            setCharaSelectMenu = false;
                            setCorrelationMenu = false;
                            currentMax = charaLimit;
                            if (charaNumText != null)
                            {
                                var numTxt = charaNumText.GetComponent<TextMeshProUGUI>();
                                if (numTxt != null)
                                {
                                    numTxt.text = charaLimit.ToString();
                                }
                            } 

                            var contentGameObj = simulation.transform.FindChild("CharaEntry(Clone)/Canvas/MainPanel/CharaList/Window/BG/Content");

                            if (contentGameObj != null)
                            {
                                SetGridLayout(contentGameObj, charaLimit);
                            }
                            else ExpandCharacterRosterPlugin.Log.LogInfo("transform not found for CharaEntry");

                            int charaIndex = 0;
                            foreach (var index in CharaEntry.Instance.CharaListView._viewer._indexData)
                            {
                                if (!index.Data.IsActive() && charaIndex > 23 && charaIndex < charaLimit) index.Data.SetActive(true);
                                else if (charaIndex > 23 && index.Data.IsActive() && charaIndex >= charaLimit)
                                {
                                    if (index.Data.HasChara())
                                    {
                                        if (index.Data.Actor.charasGameParam.isPC)
                                        {
                                            index.Data.Actor.charasGameParam.isPC = false;
                                            index.Data.SetPlayer(false);
                                            setPlayer = true;
                                        } 
                                    }
                                    index.Data.SetActive(false);
                                } 
                                charaIndex++;
                            }

                            if (setPlayer)
                            {
                                charaIndex = 0;
                                setPlayer = false;
                                foreach (var index in CharaEntry.Instance.CharaListView._viewer._indexData)
                                {
                                    if (index.Data.IsActive() && index.Data.HasChara() && charaIndex < charaLimit)
                                    {
                                        index.Data.Actor.charasGameParam.isPC = true;
                                        index.Data.SetPlayer(true);
                                        SV.Sound.Play(SystemSE.ok);
                                        break;
                                    }
                                    charaIndex++;
                                }
                            }
                        }
                    }
                    else if (restoreCharaRoster)
                    {
                        if (expandObj != null) expandObj.SetActive(false);
                        restoreCharaRoster = false;
                        setWhenOpen = true;
                        var contentGameObj = simulation.transform.FindChild("CharaEntry(Clone)/Canvas/MainPanel/CharaList/Window/BG/Content");
                        if (contentGameObj != null)
                        {
                            var gridGroup = contentGameObj.gameObject.GetComponent<GridLayoutGroup>();
                            gridGroup.constraint = GridLayoutGroup.Constraint.Flexible;
                            gridGroup.constraintCount = 2;
                            var rect = contentGameObj.gameObject.GetComponent<RectTransform>();
                            rect.localScale = new Vector3(1.0f, 1.0f, 1f);
                        }
                        else ExpandCharacterRosterPlugin.Log.LogInfo("transform not found for CharaEntry");
                    }
                }
                else setWhenOpen = true;
            }

            if (CharaSelect.Instance != null)
            {
                if (CharaSelect.Instance._isOpen)
                {
                    if (Game.saveData == null) return;
                    var charaLimit = ExpandCharacterRosterPlugin.GetCharaLimit();
                    if (Game.saveData.CharaEntryMode > 0 && charaLimit >= 24)
                    {
                        if (!setCharaSelectMenu)
                        {
                            setCharaSelectMenu = true;
                            restoreCharaSelect = true;
                            currentMax = charaLimit;

                            var contentGameObj = simulation.transform.FindChild("CharaSelect(Clone)/Canvas/MainPanel/CharaList/Window/BG/Content");
                            if (contentGameObj != null)
                            {
                                var correbg = contentGameObj.gameObject.GetComponentInParent<Image>();
                                if (correbg != null) correbg.enabled = false;
                                SetGridLayout(contentGameObj, charaLimit);
                            }
                            else ExpandCharacterRosterPlugin.Log.LogInfo("transform not found for CharaSelect");
                           

                            int charaIndex = 0;
                            foreach (var index in CharaSelect.Instance._charaListView._viewer._indexData)
                            {
                                if (!index.Data.IsActive() && charaIndex > 23 && charaIndex < charaLimit) index.Data.SetActive(true);
                                else if (charaIndex > 23 && index.Data.IsActive() && charaIndex >= charaLimit) index.Data.SetActive(false);
                                charaIndex++;
                            }
                        }
                    }
                    else if (restoreCharaSelect)
                    {
                        restoreCharaSelect = false;
                        var contentGameObj = simulation.transform.FindChild("CharaSelect(Clone)/Canvas/MainPanel/CharaList/Window/BG/Content");
                        if (contentGameObj != null)
                        {
                            var gridGroup = contentGameObj.gameObject.GetComponent<GridLayoutGroup>();
                            gridGroup.constraint = GridLayoutGroup.Constraint.Flexible;
                            gridGroup.constraintCount = 2;
                            var rect = contentGameObj.gameObject.GetComponent<RectTransform>();
                            rect.localScale = new Vector3(1.0f, 1.0f, 1f);
                        }
                        else ExpandCharacterRosterPlugin.Log.LogInfo("transform not found for CharaSelect");
                    }
                }
            }

            if (CorrelationDiagram.Instance != null)
            {
                if (CorrelationDiagram.Instance._isOpen)
                {
                    if (Game.saveData == null) return;
                    var charaLimit = ExpandCharacterRosterPlugin.GetCharaLimit();
                    if (Game.saveData.CharaEntryMode > 0 && charaLimit >= 24)
                    {
                        if (!setCorrelationMenu)
                        {
                            setCorrelationMenu = true;
                            restoreCharaCorre = true;
                            currentMax = charaLimit;

                            var contentGameObj = simulation.transform.Find("CorrelationDiagram(Clone)/Canvas/MainPanel/CharaList/Window/BG/Content");
                            if (contentGameObj != null)
                            {
                                var correbg = contentGameObj.gameObject.GetComponentInParent<Image>();
                                if (correbg != null) correbg.enabled = false;
                                SetGridLayout(contentGameObj, charaLimit);
                            }
                            else ExpandCharacterRosterPlugin.Log.LogInfo("transform not found for CorrelationDiagram");

                            int charaIndex = 0;
                            foreach (var index in CorrelationDiagram.Instance._charaListView._viewer._indexData)
                            {
                                if (!index.Data.IsActive() && charaIndex > 23 && charaIndex < charaLimit) index.Data.SetActive(true);
                                else if (charaIndex > 23 && index.Data.IsActive() && charaIndex >= charaLimit) index.Data.SetActive(false);
                                charaIndex++;
                            }
                        }
                    }
                    else if (restoreCharaCorre)
                    {
                        restoreCharaCorre = false;
                        var contentGameObj = simulation.transform.Find("CorrelationDiagram(Clone)/Canvas/MainPanel/CharaList/Window/BG/Content");
                        if (contentGameObj != null)
                        {                            
                            var gridGroup = contentGameObj.gameObject.GetComponent<GridLayoutGroup>();
                            gridGroup.constraint = GridLayoutGroup.Constraint.Flexible;
                            gridGroup.constraintCount = 2;
                            var rect = contentGameObj.gameObject.GetComponent<RectTransform>();
                            rect.localScale = new Vector3(1.0f, 1.0f, 1f);
                        }
                        else ExpandCharacterRosterPlugin.Log.LogInfo("transform not found for CorrelationDiagram");
                    }
                }
            }
        }
        public static void CreateCharaNumSlider(CharaEntry CharaEntryInstance)
        {
            if (CharaEntryInstance == null) return;
            var rightPanel = CharaEntryInstance.transform.Find("Canvas/MainPanel/CharaList/Window/Right");
            if (rightPanel != null)
            {
                var expandPanel = CharaEntryInstance.transform.Find("Canvas/MainPanel/CharaList/Window/Right/Expand");
                if (expandPanel != null) return;

                Sprite sliderSprite;

                var imgSprite = rightPanel.GetComponentInChildren<Image>();
                if (imgSprite != null)
                {
                    sliderSprite = imgSprite.sprite;
                }
                else sliderSprite = new Sprite();
                var textFont = rightPanel.GetComponentInChildren<TextMeshProUGUI>();

                expandObj = new GameObject("Expand");
                expandObj.layer = 5;
                expandObj.AddComponent<RectTransform>();
                expandObj.AddComponent<CanvasRenderer>();
                expandObj.AddComponent<Image>();
                expandObj.transform.SetParent(rightPanel.transform);

                var rectTrans = expandObj.GetComponent<RectTransform>();
                rectTrans.anchoredPosition = new Vector2(0f, 0f);
                rectTrans.localScale = new Vector3(1, 1, 1);//Reset Local scale
                rectTrans.sizeDelta = new Vector2(350, 128);
                rectTrans.localPosition = new Vector3(220f, 350, 0);

                var imgComp = expandObj.GetComponentInChildren<Image>();
                imgComp.sprite = sliderSprite;
                imgComp.type = Image.Type.Sliced;

                GameObject expandTextObj = new GameObject("expandTxt");
                expandTextObj.layer = 5;
                expandTextObj.AddComponent<RectTransform>();
                expandTextObj.AddComponent<CanvasRenderer>();
                expandTextObj.AddComponent<TextMeshProUGUI>();
                expandTextObj.transform.SetParent(expandObj.transform);
                
                var textRect = expandTextObj.GetComponent<RectTransform>();
                textRect.anchoredPosition = new Vector2(0f, 0f);
                textRect.localScale = new Vector3(1, 1, 1);//Reset Local scale
                textRect.sizeDelta = new Vector2(120, 50);
                textRect.localPosition = new Vector3(0, 35f, 0);

                var expandTxt = expandTextObj.GetComponentInChildren<TextMeshProUGUI>();
                if (textFont != null)
                {
                    expandTxt.font = textFont.font;
                    expandTxt.fontSharedMaterial = textFont.fontSharedMaterial;
                }
                expandTxt.text = "Set Max";
                expandTxt.fontSize = 24;
                expandTxt.alignment = TextAlignmentOptions.Center;
                expandTxt.color = new Color(0.9882353f, 0.95686275f, 0.9137255f);

                CreateSlider(expandObj);
                
                GameObject charaNum = new GameObject("charaNum");
                charaNum.layer = 5;
                charaNum.AddComponent<RectTransform>();
                charaNum.AddComponent<CanvasRenderer>();
                charaNum.AddComponent<Image>();
                charaNum.transform.SetParent(expandObj.transform);

                var charaNumTrans = charaNum.GetComponent<RectTransform>();
                charaNumTrans.anchoredPosition = new Vector2(0f, 0f);
                charaNumTrans.localScale = new Vector3(1, 1, 1);//Reset Local scale
                charaNumTrans.sizeDelta = new Vector2(120, 120);
                charaNumTrans.localPosition = new Vector3(110, -70, 0);

                var charaNumImg = charaNum.GetComponent<Image>();
                charaNumImg.sprite = sliderSprite;
                charaNumImg.type = Image.Type.Sliced;

                charaNumText = new GameObject("charaNumTxt");
                charaNumText.layer = 5;
                charaNumText.AddComponent<RectTransform>();
                charaNumText.AddComponent<CanvasRenderer>();
                charaNumText.AddComponent<TextMeshProUGUI>();
                charaNumText.transform.SetParent(charaNum.transform);

                var charaNumTextRect = charaNumText.GetComponent<RectTransform>();
                charaNumTextRect.anchoredPosition = new Vector2(0f, 0f);
                charaNumTextRect.localScale = new Vector3(1, 1, 1);//Reset Local scale
                charaNumTextRect.sizeDelta = new Vector2(60f, 60f);
                charaNumTextRect.localPosition = new Vector3(0f, 0f, 0f);

                var charaNumTMPComp = charaNumText.GetComponent<TextMeshProUGUI>();
                if (textFont != null)
                {
                    charaNumTMPComp.font = textFont.font;
                    charaNumTMPComp.fontSharedMaterial = textFont.fontSharedMaterial;
                }
                charaNumTMPComp.text = ExpandCharacterRosterPlugin.GetCharaLimit().ToString();
                charaNumTMPComp.fontSize = 36;
                charaNumTMPComp.alignment = TextAlignmentOptions.Center;
                charaNumTMPComp.color = new Color(0.9882353f, 0.95686275f, 0.9137255f);

                if (Game.saveData != null)
                {
                    if (Game.saveData.CharaEntryMode == 0) expandObj.SetActive(false); 
                }
            }
        }
        private static void CreateSlider(GameObject gameObj)
        {
            if (gameObj == null) return;
            GameObject sliderObj = new GameObject("slider");
            sliderObj.AddComponent<RectTransform>();
            sliderObj.AddComponent<Slider>();
            sliderObj.transform.SetParent(gameObj.transform);

            var sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchoredPosition = new Vector2(0f, 0f);
            sliderRect.localScale = new Vector3(1, 1, 1);//Reset Local scale
            sliderRect.sizeDelta = new Vector2(270, 20);
            sliderRect.localPosition = new Vector3(0, 0, 0);

            var sliderComp = sliderObj.GetComponent<Slider>();
            sliderComp.wholeNumbers = true;
            sliderComp.minValue = 24;
            sliderComp.maxValue = 96;
            sliderComp.value = ExpandCharacterRosterPlugin.GetCharaLimit();

            GameObject backgroundObj = new GameObject("Background");
            GameObject fillAreaObj = new GameObject("Fill Area");
            GameObject handleSlideAreaObj = new GameObject("Handle Slide Area");
            GameObject fillObj = new GameObject("Fill");
            GameObject handleObj = new GameObject("Handle");

            backgroundObj.layer = 5;
            fillAreaObj.layer = 5;
            handleSlideAreaObj.layer = 5;
            fillObj.layer = 5;
            handleObj.layer = 5;

            backgroundObj.transform.SetParent(sliderObj.transform);
            fillAreaObj.transform.SetParent(sliderObj.transform);
            handleSlideAreaObj.transform.SetParent(sliderObj.transform);
            fillObj.transform.SetParent(fillAreaObj.transform);
            handleObj.transform.SetParent(handleSlideAreaObj.transform);
            
            //Background
            backgroundObj.AddComponent<RectTransform>();
            backgroundObj.AddComponent<CanvasRenderer>();
            backgroundObj.AddComponent<Image>();

            var bacgroundRect = backgroundObj.GetComponent<RectTransform>();
            bacgroundRect.anchoredPosition = Vector2.zero;
            bacgroundRect.localScale = Vector3.one;
            bacgroundRect.localPosition = new Vector3(0, 0, 0);
            bacgroundRect.anchorMin = new Vector2(0f,0.5f);
            bacgroundRect.anchorMax = new Vector2(1f,0.5f);
            bacgroundRect.pivot = new Vector2(0.5f, 0.5f);
            bacgroundRect.sizeDelta = new Vector2(0f, 30f); 

            var backgroundImgComp = backgroundObj.GetComponent<Image>();
            backgroundImgComp.sprite = ResourcesLoader.GetSpriteWithCustomBorders(5);
            backgroundImgComp.type = Image.Type.Sliced;
            
            //Fill Area
            fillAreaObj.AddComponent<RectTransform>();
            var fillAreaObjRect = fillAreaObj.GetComponent<RectTransform>();
            fillAreaObjRect.anchoredPosition = Vector2.zero;
            fillAreaObjRect.localScale = Vector3.one;
            fillAreaObjRect.localPosition = new Vector3(0, 0, 0);
            fillAreaObjRect.anchorMin = new Vector2(0f, 0.5f);
            fillAreaObjRect.anchorMax = new Vector2(1f, 0.5f);
            fillAreaObjRect.pivot = new Vector2(0.5f, 0.5f);
            fillAreaObjRect.sizeDelta = new Vector2(0f, 0f);

            //Fill
            fillObj.AddComponent<RectTransform>();
            fillObj.AddComponent<CanvasRenderer>();
            fillObj.AddComponent<Image>();

            var fillObjRect = fillObj.GetComponent<RectTransform>();
            fillObjRect.anchoredPosition = Vector2.zero;
            fillObjRect.localScale = Vector3.one;
            fillObjRect.localPosition = new Vector3(0, 0, 0);
            fillObjRect.anchorMin = new Vector2(0f, 0.5f);
            fillObjRect.anchorMax = new Vector2(1f, 0.5f);
            fillObjRect.pivot = new Vector2(0.5f, 0.5f);
            fillObjRect.sizeDelta = new Vector2(20f, 30f);

            var fillObjImg = fillObj.GetComponent<Image>();
            fillObjImg.sprite = ResourcesLoader.GetSpriteWithCustomBorders(6);
            fillObjImg.type = Image.Type.Sliced;

            //Handle Slide Area
            handleSlideAreaObj.AddComponent<RectTransform>();
            var handleSlideAreaObjRect = handleSlideAreaObj.GetComponent<RectTransform>();
            handleSlideAreaObjRect.anchoredPosition = Vector2.zero;
            handleSlideAreaObjRect.localScale = Vector3.one;
            handleSlideAreaObjRect.localPosition = new Vector3(0, 0, 0);
            handleSlideAreaObjRect.anchorMin = new Vector2(0f, 0.5f);
            handleSlideAreaObjRect.anchorMax = new Vector2(1f, 0.5f);
            handleSlideAreaObjRect.pivot = new Vector2(0.5f, 0.5f);
            handleSlideAreaObjRect.sizeDelta = new Vector2(0f, 0f);

            //Handle
            handleObj.AddComponent<RectTransform>();
            handleObj.AddComponent<CanvasRenderer>();
            handleObj.AddComponent<Image>();

            var handleObjRect = handleObj.GetComponent<RectTransform>();
            handleObjRect.anchoredPosition = Vector2.zero;
            handleObjRect.localScale = Vector3.one;
            handleObjRect.localPosition = new Vector3(0, 0, 0);
            handleObjRect.anchorMin = new Vector2(0f, 0.5f);
            handleObjRect.anchorMax = new Vector2(1f, 0.5f);
            handleObjRect.pivot = new Vector2(0.5f, 0.5f);
            handleObjRect.sizeDelta = new Vector2(24f, 24f);

            var handleObjImg = handleObj.GetComponent<Image>();
            handleObjImg.sprite = ResourcesLoader.GetSpriteWithCustomBorders(7);
            handleObjImg.type = Image.Type.Sliced;

            //Setting Slider
            var sliderObjComp = sliderObj.GetComponent<Slider>();
            sliderObjComp.targetGraphic = backgroundImgComp;
            sliderObjComp.fillRect = fillObjRect;
            sliderObjComp.handleRect = handleObjRect;

            UnityAction<float> act = new System.Action<float>((float value) => { SetCharaNumText(sliderComp.value); });
            sliderComp.onValueChanged.AddListener(act);
        }
        public static void LoadCharasWithNegatives(SimulationScene simulation)
        {
            if (simulation == null) return;
            if (SimulationManager.Instance != null)
            {
                if (SimulationManager.Instance.Mode == SimulationManager.SimulationMode.RoomMorning_SimMorning && Game.saveData.CharaEntryMode > 0)
                {
                    if (!isSimRoomToMorning)
                    {
                        isSimRoomToMorning = true;
                        isSimMorning = false;
                        if (Game.Charas != null)
                        {
                            if (ExpandCharacterRosterPlugin.IsDebugLog()) ExpandCharacterRosterPlugin.Log.LogInfo($"Save Data chara count: {Game.Charas.Count}");

                            int charaMax = ExpandCharacterRosterPlugin.GetCharaLimit();
                            foreach (var chara in Game.Charas.Values)
                            {
                                if (ExpandCharacterRosterPlugin.IsDebugLog()) ExpandCharacterRosterPlugin.Log.LogInfo($"chara ArrayIndex {chara.charasGameParam.ArrayIndex}");
                                if (chara.charasGameParam.ArrayIndex > 23 && chara.charasGameParam.ArrayIndex < charaMax)
                                {
                                    chara.charasGameParam.ArrayIndex *= -1;
                                }
                            }
                        }                      
                    }
                }
                if (SimulationManager.Instance.Mode == SimulationManager.SimulationMode.SimMorning && Game.saveData.CharaEntryMode > 0)
                {
                    if (!isSimMorning)
                    {
                        isSimRoomToMorning = false;
                        isSimMorning = true;
                        if (Game.Charas != null)
                        {
                            foreach (var chara in Game.Charas.Values)
                            {
                                if (chara.charasGameParam.ArrayIndex < 0) chara.charasGameParam.ArrayIndex *= -1;
                            }
                        }
                        if (simulation.tempAIs != null)
                        {
                            foreach (var AI in simulation.tempAIs)
                            {
                                if (AI.charaData.charasGameParam.ArrayIndex < 0) AI.charaData.charasGameParam.ArrayIndex *= -1;
                            }
                            ExpandCharacterRosterPlugin.Log.LogInfo($"{simulation.tempAIs.Count} Characters Loaded thanks to negatives values!");
                        }
                    }
                }
            }
        }
        private static void SetGridLayout(Transform gridTransform, int charas)
        {
            var gridGroup = gridTransform.gameObject.GetComponent<GridLayoutGroup>();
            if (gridGroup == null) return;
            if (charas > 24) gridGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            else
            {
                gridGroup.constraint = GridLayoutGroup.Constraint.Flexible;
                gridGroup.constraintCount = 2;
                var rect = gridTransform.gameObject.GetComponent<RectTransform>();
                rect.localScale = new Vector3(1.0f, 1.0f, 1f);
            }

            if (charas > 24 && charas < 29)
            {
                gridGroup.constraintCount = 7;
                var rect = gridTransform.gameObject.GetComponent<RectTransform>();
                rect.localScale = new Vector3(0.85f, 0.85f, 1f);
            }
            else if (charas > 28 && charas < 41)
            {
                gridGroup.constraintCount = 8;
                var rect = gridTransform.gameObject.GetComponent<RectTransform>();
                rect.localScale = new Vector3(0.75f, 0.75f, 1f);
            }
            else if (charas > 40 && charas < 55)
            {
                gridGroup.constraintCount = 9;
                var rect = gridTransform.gameObject.GetComponent<RectTransform>();
                rect.localScale = new Vector3(0.65f, 0.65f, 1f);
            }
            else if (charas > 54 && charas < 61)
            {
                gridGroup.constraintCount = 10;
                var rect = gridTransform.gameObject.GetComponent<RectTransform>();
                rect.localScale = new Vector3(0.6f, 0.6f, 1f);
            }
            else if (charas > 60)
            {
                gridGroup.constraintCount = 12;
                var rect = gridTransform.gameObject.GetComponent<RectTransform>();
                rect.localScale = new Vector3(0.5f, 0.5f, 1f);
            }
        }
        public static void SetCharaNumText(float value)
        {
            if (charaNumText != null)
            {
                var textComp = charaNumText.GetComponent<TextMeshProUGUI>();
                if (textComp != null)
                {
                    if (value < 24) value = 24;
                    if (value > 96) value = 96;
                    ExpandCharacterRosterPlugin.SetCharaLimit((int)value);
                    textComp.text = ExpandCharacterRosterPlugin.GetCharaLimit().ToString();
                }
            }
        }
    }
}
