using System.Collections.Generic;
using System.IO;
using BepInEx;
using CharacterCreation;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Manager;
using SaveData;
using SV;
using SV.CoordeSelectScene;
using UnityEngine;
using UnityEngine.UI;
namespace SVS_MoreOutfits
{
    internal static class MoreOutfits
    {
        private static readonly List<Toggle> tglGroup = new();

        private static readonly Il2CppSystem.Collections.Generic.List<ThinkingManager.ChangeClothInfos> changeClothInfosList = new();
        private static Texture2D OutfitSprite { get; set; }
        public static DirectoryInfo[] GetImagesPath()
        {
            string customOutfitPath = Path.Combine(Paths.PluginPath, "SVS_MoreOutfits\\images");
            if (Directory.Exists(customOutfitPath))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(customOutfitPath);
                var imagePacks = dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);

                if (imagePacks.Length == 0) return null;
                return imagePacks;
            }
            return null;
        }
        
        public static void CheckCoordinate(int maxOutfits, bool[] checkOptions)
        {
            int coordCount = maxOutfits - 3; //3 is default coordinate lenght
            int index = 1;
            if (coordCount > 0)
            {
                index += coordCount;
            }

            if (index < checkOptions.Length)
            {
                for (int i = 0; i < checkOptions.Length; i++)
                {
                    if (i >= index) checkOptions[i] = false;
                }
            }
        }
        public static void CreateNewOutfitIcons(List<string> outfitsList, int maxOutfits)
        {
            GameObject coordbg;
            GameObject coordSelect;
            GameObject outfitCasual;

            var humanCusCoordSelect = HumanCustom.Instance;
            var simCoordSelect = CoordeSelect.Instance;
            if (humanCusCoordSelect != null && simCoordSelect != null)
            {
                var coordbgTransform = humanCusCoordSelect.transform.Find("UI/Root/Cvs_Coorde/BG/CoordinateBG");
                coordbg = coordbgTransform.gameObject;

                var coordSelectTransform = humanCusCoordSelect.transform.Find("UI/Root/Cvs_Coorde/BG/CoordinateType");
                coordSelect = coordSelectTransform.gameObject;

                var coordCasualTransform = humanCusCoordSelect.transform.Find("UI/Root/Cvs_Coorde/BG/CoordinateType/01_Plain");
                outfitCasual = coordCasualTransform.gameObject;
            }
            else if (simCoordSelect != null)
            {
                tglGroup.Clear();
                var coordbgTransform = simCoordSelect.transform.Find("Canvas/MainPanel/Selection/CoordinateBG");
                coordbg = coordbgTransform.gameObject;

                var coordSelectTransform = simCoordSelect.transform.FindChild("Canvas/MainPanel/Selection/CoordinateType");
                coordSelect = coordSelectTransform.gameObject;

                var coordCasualTransform = simCoordSelect.transform.FindChild("Canvas/MainPanel/Selection/CoordinateType/01_Plain");
                outfitCasual = coordCasualTransform.gameObject;
            }
            else if (humanCusCoordSelect != null)
            {
                var coordbgTransform = humanCusCoordSelect.transform.Find("UI/Root/Cvs_Coorde/BG/CoordinateBG");
                coordbg = coordbgTransform.gameObject;

                var coordSelectTransform = humanCusCoordSelect.transform.Find("UI/Root/Cvs_Coorde/BG/CoordinateType");
                coordSelect = coordSelectTransform.gameObject;

                var coordCasualTransform = humanCusCoordSelect.transform.Find("UI/Root/Cvs_Coorde/BG/CoordinateType/01_Plain");
                outfitCasual = coordCasualTransform.gameObject;
            }
            else return;

            //Extend the UI so it can fit more icons
            float offset = -50 * (maxOutfits - 3);
            if (coordbg != null)
            {
                var coordCanvas = coordbg.GetComponent<RectTransform>();
                if (coordCanvas != null)
                {
                    var vec = coordCanvas.offsetMin;
                    vec.x += offset;
                    coordCanvas.offsetMin = vec;
                }
            }
            else return;

            var coordinateGroup = coordSelect.GetComponent<HorizontalLayoutGroup>();//Layout group

            int max = maxOutfits - 3;//Check max outfits, ignoring default ones.
            int number = 4;//Use for naming nothing else.
            for (int i = 0; i < max; i++)//<- Create GameObjects and their components for every new outfit.
            {
                //Create a new GameObject and add Components to it
                var customOutfit = new GameObject("0" + number + "_" + outfitsList[i]); //New GameObject to be use for new Outfits icons.
                if (number >= 10) customOutfit = new GameObject(number + "_" + outfitsList[i]);
                number++;
                customOutfit.layer = 5;
                customOutfit.transform.SetParent(coordSelect.transform);//<- Set parent
                customOutfit.AddComponent<RectTransform>();
                customOutfit.AddComponent<CanvasRenderer>();
                customOutfit.AddComponent<Image>();
                customOutfit.AddComponent<Toggle>();

                //Set GameObjects Components
                var rectOutfit = customOutfit.GetComponent<RectTransform>();
                rectOutfit.anchoredPosition = new Vector2(0, 0);
                rectOutfit.sizeDelta = new Vector2(48, 56);
                rectOutfit.pivot = new Vector2(0, 1);
                rectOutfit.anchoredPosition3D = new Vector3(0, 0, 0);

                coordinateGroup.rectChildren.Add(rectOutfit);//<- Add to the outfit select window
                rectOutfit.localScale = new Vector3(1, 1, 1);//Reset Local Scale

                //Create cb_check GameObject. Use for when you click the icon
                GameObject cbOutfit = new GameObject("cb_check");
                cbOutfit.layer = 5;
                cbOutfit.transform.SetParent(customOutfit.transform);//<- Set parent
                cbOutfit.AddComponent<RectTransform>();
                cbOutfit.AddComponent<CanvasRenderer>();
                cbOutfit.AddComponent<Image>();

                //Init Component
                var rectcb = cbOutfit.GetComponent<RectTransform>();
                rectcb.anchorMin = new Vector2(0, 0);
                rectcb.anchorMax = new Vector2(1, 1);
                rectcb.anchoredPosition = new Vector2(0, 0);
                rectcb.sizeDelta = new Vector2(0, 0);
                rectcb.anchoredPosition3D = new Vector3(0, 0, 0);
                rectcb.offsetMin = new Vector2(0, 0);
                rectcb.offsetMax = new Vector2(0, 0);
                rectcb.localScale = new Vector3(1,1,1);//Reset Local scale for cb_check

                //Get Component from Outfit GameObject
                var imageOutfits = customOutfit.GetComponent<Image>();
                var imageOutfitsHover = customOutfit.GetComponent<Toggle>();

                imageOutfitsHover.transition = Selectable.Transition.SpriteSwap;

                //Get Components from cb_check GameObject
                var imageOutfitsSelected = cbOutfit.GetComponent<Image>();
                imageOutfitsSelected.raycastTarget = false;
                imageOutfitsHover.graphic = imageOutfitsSelected;

                var defSprite = outfitCasual.GetComponent<Image>();//Default outfit component
                OutfitSprite = ResourcesLoader.LoadSprite(i,0);
                if (OutfitSprite != null)
                {
                    Sprite newSprite = Sprite.Create(OutfitSprite, new Rect(0.0f, 0.0f, OutfitSprite.width, OutfitSprite.height), new Vector2(0f, 1f), 100.0f);
                    imageOutfits.sprite = newSprite;
                }
                else
                {
                    MoreOutfitsPlugin.Log.LogInfo($"Missing Sprite for Custom Outfit Normal: Case Null");
                    imageOutfits.sprite = defSprite.sprite;
                }

                OutfitSprite = ResourcesLoader.LoadSprite(i, 1);
                if (OutfitSprite != null)
                {
                    Sprite newSprite = Sprite.Create(OutfitSprite, new Rect(0.0f, 0.0f, OutfitSprite.width, OutfitSprite.height), new Vector2(0f, 1f), 100.0f);
                    imageOutfitsHover.targetGraphic = imageOutfits;
                    SpriteState state = new SpriteState();
                    state.highlightedSprite = newSprite;
                    imageOutfitsHover.spriteState = state;
                }
                else
                {
                    MoreOutfitsPlugin.Log.LogInfo($"Missing Sprite for Custom Outfit Hover: Case Null");
                    var defSpriteHover = outfitCasual.GetComponent<Toggle>();
                    imageOutfitsHover.targetGraphic = defSprite;
                    imageOutfitsHover.spriteState.highlightedSprite = defSpriteHover.spriteState.highlightedSprite;
                }

                OutfitSprite = ResourcesLoader.LoadSprite(i, 2);
                if (OutfitSprite != null)
                {
                    Sprite newSprite = Sprite.Create(OutfitSprite, new Rect(0.0f, 0.0f, OutfitSprite.width, OutfitSprite.height), new Vector2(0f, 1f), 100.0f);
                    imageOutfitsSelected.sprite = newSprite;

                }
                else
                {
                    MoreOutfitsPlugin.Log.LogInfo($"Missing Sprite for Custom Outfit Selected: Case Null");
                    var defSpriteHover = outfitCasual.GetComponent<Toggle>();
                    imageOutfitsHover.spriteState.highlightedSprite = defSpriteHover.spriteState.highlightedSprite;
                }

                if (simCoordSelect != null && humanCusCoordSelect == null) tglGroup.Add(imageOutfitsHover);
            }

            if (simCoordSelect != null && humanCusCoordSelect == null && tglGroup.Count > 0)
            {
                int index = 0;
                Il2CppReferenceArray<Toggle> newtglGroup = new Il2CppReferenceArray<Toggle>(maxOutfits);
                for (int i = 0; i < maxOutfits; i++)
                {
                    if (i < 3) newtglGroup[i] = simCoordSelect._tglCoordes[i];
                    else
                    {
                        newtglGroup[i] = tglGroup[index];
                        index++;
                    }
                }
                simCoordSelect._tglCoordes = newtglGroup;
            }
        }
        public static void CreateOldChangeOfClothesList()
        {
            if (changeClothInfosList.Count > 0) return;
            bool[] options = MoreOutfitsPlugin.GetOptions();
            int maxOutfits = MoreOutfitsPlugin.GetMaxOutfits();
            CheckCoordinate(maxOutfits, options); //Check Character coordinate lenght
            var oldChangeOfClothesList = ThinkingManager.Instance.changeOfClothes;   
            
            //int weekDay = 0;
            //int timePeriod = 0;
            //int job = 0;
            foreach (var day in oldChangeOfClothesList)
            {
                ThinkingManager.ChangeClothInfos ccis = new ThinkingManager.ChangeClothInfos();
                //timePeriod = 0;
                foreach (var period in day.times)
                {
                    ThinkingManager.ChangeClothInfo cci = new ThinkingManager.ChangeClothInfo();

                    Il2CppSystem.Collections.Generic.List<int> outfitsStartsValues = new();
                    //job = 0;
                    foreach (var value in period.starts)
                    {
                        outfitsStartsValues.Add(value);
                        //if (!newOptions[5] && value > 2) oldChangeOfClothesList[weekDay].times[timePeriod].starts[job] = 1;
                        //job++;
                    }

                    Il2CppSystem.Collections.Generic.List<int> outfitsChangesValues = new();
                    //job = 0;
                    foreach (var value in period.changes)
                    {
                        outfitsChangesValues.Add(value);
                        //if (!newOptions[5] && value > 2) oldChangeOfClothesList[weekDay].times[timePeriod].changes[job] = 1;
                        //job++;
                    }

                    if (outfitsStartsValues.Count > 0) cci.starts = outfitsStartsValues;
                    if (outfitsChangesValues.Count > 0) cci.changes = outfitsChangesValues;
                    ccis.times.Add(cci);
                    //timePeriod++;
                }
                changeClothInfosList.Add(ccis);
                //weekDay++;
            }
        }
        public static void SetDailyOutfit(Actor _actor, bool _isStart, int _timezone, int maxOutfits)
        {
            if (_timezone >= 0)
            {
                bool[] options = MoreOutfitsPlugin.GetOptions();
                CheckCoordinate(maxOutfits, options); //Check Character coordinate lenght
                //Options
                //bool 0 = isPC
                //bool 1 = Weekend
                //bool 2 = Night
                //bool 3 = Lewd
                //bool 4 = Costume
                //bool 5 = Sports
                //bool 6 = Bath
                //bool 7 = Camping
                //bool 8 = Home
                if (_actor.charasGameParam.isPC && !options[0]) return;
                RestoreOutfitList(_actor, _timezone);
                OutfitCondition_Weekend(_actor, _timezone, options[1]);
                OutfitCondition_Lewd(_actor, _timezone, options[3]);
                OutfitCondition_Sport(_actor, _timezone, options[5]);
                OutfitCondition_Night(_actor, _timezone, options[2]);
                OutfitCondition_Costume(_actor, _timezone, options[4]);

                //Force Change Outfit
                //_actor.chaCtrl.coorde.SetNowCoordinate(chara.chaCtrl.data.Coordinates[4]);
                //chara.chaCtrl.coorde.fileStatus.coordinateType = 4;
                //chara.chaCtrl.ReloadCoordinate();
            }
        }
        private static void RestoreOutfitList(Actor chara, int timeOfDay)
        {
            int weekDay = Game.saveData.Week;
            var dayList = ThinkingManager.Instance.changeOfClothes;

            if (dayList[weekDay].times[timeOfDay].starts[chara.Job] > 2)             
            {
                dayList[weekDay].times[timeOfDay].starts[chara.Job] = changeClothInfosList[weekDay].times[timeOfDay].starts[chara.Job];
            }
            if (dayList[weekDay].times[timeOfDay].changes[chara.Job] > 2)
            {
                dayList[weekDay].times[timeOfDay].changes[chara.Job] = changeClothInfosList[weekDay].times[timeOfDay].changes[chara.Job];
            }
        }

        private static void OutfitCondition_Weekend(Actor chara, int timeOfDay,bool useOutfit)
        {
            if (useOutfit)
            {
                int weekDay = Game.saveData.Week;
                if (weekDay > 4)
                {
                    var dayList = ThinkingManager.Instance.changeOfClothes;

                    int startOutfitID = dayList[weekDay].times[timeOfDay].starts[chara.Job];
                    if ((startOutfitID != 1 && startOutfitID != 2) || (chara.Job == 0 && startOutfitID != 2))
                    {
                        dayList[weekDay].times[timeOfDay].starts[chara.Job] = 3; //Set 03:Weekend Outfit
                    }

                    int changeOutfitID = dayList[weekDay].times[timeOfDay].changes[chara.Job];
                    if ((changeOutfitID != 1 && changeOutfitID != 2 && changeOutfitID != 3) || (chara.Job == 0 && changeOutfitID != 2 && changeOutfitID != 3))
                    {
                        dayList[weekDay].times[timeOfDay].changes[chara.Job] = 3; //Set 03:Weekend Outfit
                    }
                }
            }
        }
        private static void OutfitCondition_Night(Actor chara, int timeOfDay, bool useOutfit)
        {
            if (useOutfit)
            {
                if (timeOfDay >= 2)
                {
                    bool[] extraOptions = MoreOutfitsPlugin.GetExtraOptions();
                    int weekDay = Game.saveData.Week;
                    var dayList = ThinkingManager.Instance.changeOfClothes;

                    if (!extraOptions[0])
                    {
                        if (timeOfDay == 3)
                        {
                            int startOutfitID = dayList[weekDay].times[timeOfDay].starts[chara.Job];
                            if ((startOutfitID != 1 && startOutfitID != 2) || (chara.Job == 0 && startOutfitID != 2))
                            {
                                dayList[weekDay].times[timeOfDay].starts[chara.Job] = 4; //Set 04:Night Outfit
                            }
                        }

                        int changeOutfitID = dayList[weekDay].times[timeOfDay].changes[chara.Job];
                        if ((changeOutfitID != 1 && changeOutfitID != 2 && changeOutfitID != 4) || (chara.Job == 0 && changeOutfitID != 2 && changeOutfitID != 4))
                        {
                            dayList[weekDay].times[timeOfDay].changes[chara.Job] = 4; //Set 04:Night Outfit
                        }
                    }
                    else if (timeOfDay == 3)
                    {
                        int changeOutfitID = dayList[weekDay].times[timeOfDay].changes[chara.Job];
                        if ((changeOutfitID != 1 && changeOutfitID != 2 && changeOutfitID != 4) || (chara.Job == 0 && changeOutfitID != 2 && changeOutfitID != 4))
                        {
                            dayList[weekDay].times[timeOfDay].changes[chara.Job] = 4; //Set 04:Night Outfit
                        }
                    }
                }
            }
        }
        private static void OutfitCondition_Lewd(Actor chara, int timeOfDay, bool useOutfit)
        {
            if (useOutfit)
            {
                if (Game.saveData.dataCount.circularNotice == 1 && Game.saveData.dataCount.isCircularNoticeUse)
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
        }
        private static void OutfitCondition_Costume(Actor chara, int timeOfDay, bool useOutfit)
        {
            if (useOutfit)
            {
                var dayAndPeriod = MoreOutfitsPlugin.GetCostumeDayAndPeriod();
                int weekDay = Game.saveData.Week;
                var dayList = ThinkingManager.Instance.changeOfClothes;

                if (weekDay == dayAndPeriod.Item1)
                {
                    switch (timeOfDay)
                    {
                        case 0:
                            if (dayAndPeriod.Item2[0])
                            {
                                /*int startOutfitID = dayList[weekDay].times[timeOfDay].starts[chara.Job];
                                if ((startOutfitID != 1 && startOutfitID != 2) || (chara.Job == 0 && startOutfitID != 2))
                                {
                                    dayList[weekDay].times[timeOfDay].starts[chara.Job] = 6; //Set 04:Night Outfit
                                }*/

                                int changeOutfitID = dayList[weekDay].times[timeOfDay].changes[chara.Job];
                                if ((changeOutfitID != 1 && changeOutfitID != 2 && changeOutfitID != 6) || (chara.Job == 0 && changeOutfitID != 2 && changeOutfitID != 6))
                                {
                                    dayList[weekDay].times[timeOfDay].changes[chara.Job] = 6; //Set 04:Night Outfit
                                }
                            }
                            break;

                        case 1:
                            if (dayAndPeriod.Item2[1])
                            {
                                if (dayAndPeriod.Item2[0])
                                {
                                    int startOutfitID = dayList[weekDay].times[timeOfDay].starts[chara.Job];
                                    if ((startOutfitID != 1 && startOutfitID != 2) || (chara.Job == 0 && startOutfitID != 2))
                                    {
                                        dayList[weekDay].times[timeOfDay].starts[chara.Job] = 6; //Set 04:Night Outfit
                                    }
                                }

                                int changeOutfitID = dayList[weekDay].times[timeOfDay].changes[chara.Job];
                                if ((changeOutfitID != 1 && changeOutfitID != 2 && changeOutfitID != 6) || (chara.Job == 0 && changeOutfitID != 2 && changeOutfitID != 6))
                                {
                                    dayList[weekDay].times[timeOfDay].changes[chara.Job] = 6; //Set 04:Night Outfit
                                }
                            }
                            break;

                        case 2:
                            if (dayAndPeriod.Item2[2])
                            {
                                if (dayAndPeriod.Item2[1])
                                {
                                    int startOutfitID = dayList[weekDay].times[timeOfDay].starts[chara.Job];
                                    if ((startOutfitID != 1 && startOutfitID != 2) || (chara.Job == 0 && startOutfitID != 2))
                                    {
                                        dayList[weekDay].times[timeOfDay].starts[chara.Job] = 6; //Set 04:Night Outfit
                                    }
                                }

                                int changeOutfitID = dayList[weekDay].times[timeOfDay].changes[chara.Job];
                                if ((changeOutfitID != 1 && changeOutfitID != 2 && changeOutfitID != 6) || (chara.Job == 0 && changeOutfitID != 2 && changeOutfitID != 6))
                                {
                                    dayList[weekDay].times[timeOfDay].changes[chara.Job] = 6; //Set 04:Night Outfit
                                }
                            }
                            break;

                        case 3:
                            if (dayAndPeriod.Item2[3])
                            {
                                if (dayAndPeriod.Item2[2])
                                {
                                    int startOutfitID = dayList[weekDay].times[timeOfDay].starts[chara.Job];
                                    if ((startOutfitID != 1 && startOutfitID != 2) || (chara.Job == 0 && startOutfitID != 2))
                                    {
                                        dayList[weekDay].times[timeOfDay].starts[chara.Job] = 6; //Set 04:Night Outfit
                                    }
                                }

                                int changeOutfitID = dayList[weekDay].times[timeOfDay].changes[chara.Job];
                                if ((changeOutfitID != 1 && changeOutfitID != 2 && changeOutfitID != 6) || (chara.Job == 0 && changeOutfitID != 2 && changeOutfitID != 6))
                                {
                                    dayList[weekDay].times[timeOfDay].changes[chara.Job] = 6; //Set 04:Night Outfit
                                }
                            }
                            break;
                    }
                }
                else if (dayAndPeriod.Item1 == 7 && weekDay > 4)
                {
                    switch (timeOfDay)
                    {
                        case 0:
                            if (dayAndPeriod.Item2[0])
                            {
                                /*int startOutfitID = dayList[weekDay].times[timeOfDay].starts[chara.Job];
                                if ((startOutfitID != 1 && startOutfitID != 2) || (chara.Job == 0 && startOutfitID != 2))
                                {
                                    dayList[weekDay].times[timeOfDay].starts[chara.Job] = 6; //Set 04:Night Outfit
                                }*/

                                int changeOutfitID = dayList[weekDay].times[timeOfDay].changes[chara.Job];
                                if ((changeOutfitID != 1 && changeOutfitID != 2 && changeOutfitID != 6) || (chara.Job == 0 && changeOutfitID != 2 && changeOutfitID != 6))
                                {
                                    dayList[weekDay].times[timeOfDay].changes[chara.Job] = 6; //Set 04:Night Outfit
                                }
                            }
                            break;

                        case 1:
                            if (dayAndPeriod.Item2[1])
                            {
                                if (dayAndPeriod.Item2[0])
                                {
                                    int startOutfitID = dayList[weekDay].times[timeOfDay].starts[chara.Job];
                                    if ((startOutfitID != 1 && startOutfitID != 2) || (chara.Job == 0 && startOutfitID != 2))
                                    {
                                        dayList[weekDay].times[timeOfDay].starts[chara.Job] = 6; //Set 04:Night Outfit
                                    }
                                }

                                int changeOutfitID = dayList[weekDay].times[timeOfDay].changes[chara.Job];
                                if ((changeOutfitID != 1 && changeOutfitID != 2 && changeOutfitID != 6) || (chara.Job == 0 && changeOutfitID != 2 && changeOutfitID != 6))
                                {
                                    dayList[weekDay].times[timeOfDay].changes[chara.Job] = 6; //Set 04:Night Outfit
                                }
                            }
                            break;

                        case 2:
                            if (dayAndPeriod.Item2[2])
                            {
                                if (dayAndPeriod.Item2[1])
                                {
                                    int startOutfitID = dayList[weekDay].times[timeOfDay].starts[chara.Job];
                                    if ((startOutfitID != 1 && startOutfitID != 2) || (chara.Job == 0 && startOutfitID != 2))
                                    {
                                        dayList[weekDay].times[timeOfDay].starts[chara.Job] = 6; //Set 04:Night Outfit
                                    }
                                }

                                int changeOutfitID = dayList[weekDay].times[timeOfDay].changes[chara.Job];
                                if ((changeOutfitID != 1 && changeOutfitID != 2 && changeOutfitID != 6) || (chara.Job == 0 && changeOutfitID != 2 && changeOutfitID != 6))
                                {
                                    dayList[weekDay].times[timeOfDay].changes[chara.Job] = 6; //Set 04:Night Outfit
                                }
                            }
                            break;

                        case 3:
                            if (dayAndPeriod.Item2[3])
                            {
                                if (dayAndPeriod.Item2[2])
                                {
                                    int startOutfitID = dayList[weekDay].times[timeOfDay].starts[chara.Job];
                                    if ((startOutfitID != 1 && startOutfitID != 2) || (chara.Job == 0 && startOutfitID != 2))
                                    {
                                        dayList[weekDay].times[timeOfDay].starts[chara.Job] = 6; //Set 04:Night Outfit
                                    }
                                }

                                int changeOutfitID = dayList[weekDay].times[timeOfDay].changes[chara.Job];
                                if ((changeOutfitID != 1 && changeOutfitID != 2 && changeOutfitID != 6) || (chara.Job == 0 && changeOutfitID != 2 && changeOutfitID != 6))
                                {
                                    dayList[weekDay].times[timeOfDay].changes[chara.Job] = 6; //Set 04:Night Outfit
                                }
                            }
                            break;
                    }
                }
                else if (dayAndPeriod.Item1 == 8)
                {
                    switch (timeOfDay)
                    {
                        case 0:
                            if (dayAndPeriod.Item2[0])
                            {
                                /*int startOutfitID = dayList[weekDay].times[timeOfDay].starts[chara.Job];
                                if ((startOutfitID != 1 && startOutfitID != 2) || (chara.Job == 0 && startOutfitID != 2))
                                {
                                    dayList[weekDay].times[timeOfDay].starts[chara.Job] = 6; //Set 04:Night Outfit
                                }*/

                                int changeOutfitID = dayList[weekDay].times[timeOfDay].changes[chara.Job];
                                if ((changeOutfitID != 1 && changeOutfitID != 2 && changeOutfitID != 6) || (chara.Job == 0 && changeOutfitID != 2 && changeOutfitID != 6))
                                {
                                    dayList[weekDay].times[timeOfDay].changes[chara.Job] = 6; //Set 04:Night Outfit
                                }
                            }
                            break;

                        case 1:
                            if (dayAndPeriod.Item2[1])
                            {
                                if (dayAndPeriod.Item2[0])
                                {
                                    int startOutfitID = dayList[weekDay].times[timeOfDay].starts[chara.Job];
                                    if ((startOutfitID != 1 && startOutfitID != 2) || (chara.Job == 0 && startOutfitID != 2))
                                    {
                                        dayList[weekDay].times[timeOfDay].starts[chara.Job] = 6; //Set 04:Night Outfit
                                    }
                                }

                                int changeOutfitID = dayList[weekDay].times[timeOfDay].changes[chara.Job];
                                if ((changeOutfitID != 1 && changeOutfitID != 2 && changeOutfitID != 6) || (chara.Job == 0 && changeOutfitID != 2 && changeOutfitID != 6))
                                {
                                    dayList[weekDay].times[timeOfDay].changes[chara.Job] = 6; //Set 04:Night Outfit
                                }
                            }
                            break;

                        case 2:
                            if (dayAndPeriod.Item2[2])
                            {
                                if (dayAndPeriod.Item2[1])
                                {
                                    int startOutfitID = dayList[weekDay].times[timeOfDay].starts[chara.Job];
                                    if ((startOutfitID != 1 && startOutfitID != 2) || (chara.Job == 0 && startOutfitID != 2))
                                    {
                                        dayList[weekDay].times[timeOfDay].starts[chara.Job] = 6; //Set 04:Night Outfit
                                    }
                                }

                                int changeOutfitID = dayList[weekDay].times[timeOfDay].changes[chara.Job];
                                if ((changeOutfitID != 1 && changeOutfitID != 2 && changeOutfitID != 6) || (chara.Job == 0 && changeOutfitID != 2 && changeOutfitID != 6))
                                {
                                    dayList[weekDay].times[timeOfDay].changes[chara.Job] = 6; //Set 04:Night Outfit
                                }
                            }
                            break;

                        case 3:
                            if (dayAndPeriod.Item2[3])
                            {
                                if (dayAndPeriod.Item2[2])
                                {
                                    int startOutfitID = dayList[weekDay].times[timeOfDay].starts[chara.Job];
                                    if ((startOutfitID != 1 && startOutfitID != 2) || (chara.Job == 0 && startOutfitID != 2))
                                    {
                                        dayList[weekDay].times[timeOfDay].starts[chara.Job] = 6; //Set 04:Night Outfit
                                    }
                                }

                                int changeOutfitID = dayList[weekDay].times[timeOfDay].changes[chara.Job];
                                if ((changeOutfitID != 1 && changeOutfitID != 2 && changeOutfitID != 6) || (chara.Job == 0 && changeOutfitID != 2 && changeOutfitID != 6))
                                {
                                    dayList[weekDay].times[timeOfDay].changes[chara.Job] = 6; //Set 04:Night Outfit
                                }
                            }
                            break;
                    }
                }
            }
        }
        private static void OutfitCondition_Sport(Actor chara, int timeOfDay, bool useOutfit)
        {
            int weekDay = Game.saveData.Week;
            var dayList = ThinkingManager.Instance.changeOfClothes;

            if (changeClothInfosList[weekDay].times[timeOfDay].starts[chara.Job] == 8)
            {
                if (!useOutfit) dayList[weekDay].times[timeOfDay].starts[chara.Job] = 1;
                else dayList[weekDay].times[timeOfDay].starts[chara.Job] = changeClothInfosList[weekDay].times[timeOfDay].starts[chara.Job];
            }
            if (changeClothInfosList[weekDay].times[timeOfDay].changes[chara.Job] == 8)
            {
                if (!useOutfit) dayList[weekDay].times[timeOfDay].changes[chara.Job] = 1;
                else dayList[weekDay].times[timeOfDay].changes[chara.Job] = changeClothInfosList[weekDay].times[timeOfDay].changes[chara.Job];
            }
        }
    }
}
