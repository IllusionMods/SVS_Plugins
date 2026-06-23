using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using SaveData;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem;
using Manager;
using SV.CorrelationDiagramScene;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using SV;
using Character;

namespace FavorabiltyGainController
{
    [BepInPlugin(GUID, DisplayName, Version)]
    public class FavorabilityGainPlugin : BasePlugin
    {
        public const string DisplayName = "SVS_FavorabilityGainController";
        public const string GUID = "SVS_FavorabilityGainController";
        public const string Version = "1.3.1";

        internal static new ManualLogSource Log;
        private static Harmony patchedHooks;

        private static ConfigEntry<SensitivityType> _sensitivityMin;
        private static ConfigEntry<SensitivityType> _sensitivityMax;
        private static ConfigEntry<FavorModes> _favorMode;

        private static ConfigEntry<bool> _enableConfig;
        private static ConfigEntry<bool> _indiFavorEnable;
        private static ConfigEntry<bool> _devLog;
        private static ConfigEntry<bool> _enableShowValues;
        private static ConfigEntry<bool> _hideShowValuesdesc;
        private static ConfigEntry<bool> _removeFirstImpression;
        private static ConfigEntry<bool> _heteroHomoFriendGain;

        private static ConfigEntry<int> _sliderAll;
        private static ConfigEntry<int> _sliderLove;
        private static ConfigEntry<int> _sliderFriend;
        private static ConfigEntry<int> _sliderDistant;
        private static ConfigEntry<int> _sliderHate;

        private static ConfigEntry<int> _absoluteMin;
        private static ConfigEntry<int> _absoluteMax;

        private static System.Random _rnd = new System.Random();
        private static GameObject FavorCanvasObject;
        private static CorrelationDiagram DiagramSceneInstance;
        private static WorldData _gameInstance;

        private static readonly string _desHateSlider = "Set how many Hate SubPoints the characters gain from interactions (in %). Value 0 the characters won't get any points in Hate. Value 100 is the default Game value";
        private static readonly string _desDistantSlider = "Set how many Distant SubPoints the characters gain from interactions (in %). Value 0 the characters won't get any points in Distant. Value 100 is the default Game value";
        private static readonly string _desFriendSlider = "Set how many Friend SubPoints the characters gain from interactions (in %). Value 0 the characters won't get any points in Friend. Value 100 is the default Game value";
        private static readonly string _desLoveSlider = "Set how many Love SubPoints the characters gain from interactions (in %). Value 0 the characters won't get any points in Love. Value 100 is the default Game value";
        private static readonly string _desEnableIndiFavor = "If Enable, it will use the values from the Love, Friend, Distant, Hate sliders to Set how many points the characters get for each Category. (The Slider [Set % gain to all Categories] will be ignored)";
        private static readonly string _desAllSlider = "Set how many SubPoints the characters gain from interactions (in %). Value 0 disable the Favorability Gain system (Characters get 0 points). Value 100 is the default/vanilla Game value";

        private static float _allMod = 0f;
        private static float _loveMod = 0f;
        private static float _friendMod = 0f;
        private static float _distantMod = 0f;
        private static float _hateMod = 0f;
        private static float _roundedValue = 30f;

        private static int _tempL = 0;
        private static int _tempF = 0;
        private static int _tempD = 0;
        private static int _tempH = 0;
        private static int _randomTotal = 0;
        private static int _unforseen = 0;
        private static int _currentSelected = -1;
        //private static int _charaList;

        public override void Load()
        {
            Log = base.Log;

            _enableConfig = Config.Bind("General", "Favorability Gain Controller", true, new ConfigDescription("Reload the game to Enable/Disable this mod", null, new ConfigurationManagerAttributes { Order = 4 }));           
            _devLog = Config.Bind("General", "Enable Log", false, new ConfigDescription("Sends information to the LogOutput.log", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));
            _hideShowValuesdesc = Config.Bind("General", "Hide Description", false, new ConfigDescription("Hide the description of the favoarbility values", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }));
            _enableShowValues = Config.Bind("General", "Show Favorability Points", true, new ConfigDescription("Show the Favorability Points at the jizo statue. Select a character at the jizo statue to see the exact amount of points a character has toward the selected one", null, new ConfigurationManagerAttributes { Order = 1 }));
            _heteroHomoFriendGain = Config.Bind("General", "Enables Hetero/Homo friend point gain", false, new ConfigDescription("Allows Hetero and Homo characters to gain friend points toward Opposite and Same sex characters", null, new ConfigurationManagerAttributes { Order = 0 }));

            _favorMode = Config.Bind("Game Mode", "Select Game Mode", FavorModes.Normal,
            new ConfigDescription("Select a Game Mode, the Modifiers will apply on any mode. | Normal = Default Game System. | FullPoint = Ignores the subPoints and every action will give Full points instead (a Full point is 30 subPoints). | Reverse = The Points gained are reverse (Loves becomes Hate, Friend becomes Distant and vice versa), | Random = Gain a random amount of subPoints from interaction. | Unforeseen = Has a chance of giving 300 subPoints of any type after an interaction", null, new ConfigurationManagerAttributes { Order = 1 }));

            _removeFirstImpression = Config.Bind("Set Favorability Modifier", "Removes Bonus Points Gain", false, new ConfigDescription("Characters can gain a massive amount of points during the first couple of interactions so they can build relationships faster, enabling this remove those bonus points and the characters will build relationships at a normal pace", null, new ConfigurationManagerAttributes { Order = 7 }));
            _sliderAll = Config.Bind("Set Favorability Modifier", "Set % gain to all Categories", 100, new ConfigDescription(_desAllSlider, new AcceptableValueRange<int>(0, 200), new ConfigurationManagerAttributes { Order = 6 }));

            _indiFavorEnable = Config.Bind("Set Favorability Modifier", "Modify Values Independently", false, new ConfigDescription(_desEnableIndiFavor, null, new ConfigurationManagerAttributes { Order = 4 }));

            _sliderLove = Config.Bind("Set Favorability Modifier", "Set Love Points % Gain", 100, new ConfigDescription(_desLoveSlider, new AcceptableValueRange<int>(0, 200), new ConfigurationManagerAttributes { Order = 3 }));
            _sliderFriend = Config.Bind("Set Favorability Modifier", "Set Friend Point % Gain", 100, new ConfigDescription(_desFriendSlider, new AcceptableValueRange<int>(0, 200), new ConfigurationManagerAttributes { Order = 2 }));
            _sliderDistant = Config.Bind("Set Favorability Modifier", "Set Distant Point % Gain", 100, new ConfigDescription(_desDistantSlider, new AcceptableValueRange<int>(0, 200), new ConfigurationManagerAttributes { Order = 1 }));
            _sliderHate = Config.Bind("Set Favorability Modifier", "Set Hate Point % Gain", 100, new ConfigDescription(_desHateSlider, new AcceptableValueRange<int>(0, 200), new ConfigurationManagerAttributes { Order = 0 }));

            _sensitivityMin = Config.Bind("Set limits", "Minimum Limit for Favorability", SensitivityType.Love | SensitivityType.Friend | SensitivityType.Distant | SensitivityType.Hate,
            new ConfigDescription("Enables which Favorability Type has a Minimun limit for gaining subPoints (By default are all enables)", null, new ConfigurationManagerAttributes { Order = 3 }));
            _absoluteMin = Config.Bind("Set limits", "Set Minimum Gain Value", 1, new ConfigDescription("Set the Minimum possible value a character can gain from interactions (1 is to prevent getting 0 subPoints from interactions unless the slider is set as 0)", new AcceptableValueRange<int>(1, 30), new ConfigurationManagerAttributes { Order = 2 }));

            _sensitivityMax = Config.Bind("Set limits", "Max Limit for Favorability", SensitivityType.Love | SensitivityType.Friend | SensitivityType.Distant | SensitivityType.Hate,
            new ConfigDescription("Enables which Favorability Type has a Max limit for gaining subPoint (By default are all enables)", null, new ConfigurationManagerAttributes { Order = 1 }));
            _absoluteMax = Config.Bind("Set limits", "Set Max Gain Value", 1800, new ConfigDescription("Set the Max possible value a character can gain from interactions (900 is 30 Points for a Favorability Type)", new AcceptableValueRange<int>(30, 1800), new ConfigurationManagerAttributes { Order = 0 }));

            if (_enableConfig.Value)
            {
                patchedHooks = Harmony.CreateAndPatchAll(typeof(Hooks));
            }

            ClassInjector.RegisterTypeInIl2Cpp<FavorabilityCanvas>();
        }
        public override bool Unload()
        {
            patchedHooks?.UnpatchSelf();
            return true;
        }

        public enum FavorModes
        {
            Normal = 0,
            FullPoint = 1,
            Reverse = 2,
            Random = 3,
            Unforeseen = 4,
        }

        [System.Flags]
        public enum SensitivityType
        {
            None = 0,
            Love = 1 << 0,
            Friend = 1 << 1,
            Distant = 1 << 2,
            Hate = 1 << 3,
        }

        /*public enum Sensitivity
        {
            LOVE = 0,
            FRIEND = 1,
            INDIFFERENT = 2,
            DISLIKE = 3,
            MAX = 4
        }*/

        public static bool GetConfigValues()
        {
            return _enableShowValues.Value;
        }

        //public static void SetModifiers(float _allMod, float _loveMod,float _friendMod, float _distantMod, float _hateMod)
        public static void SetModifiers()
        {
            if (_sliderAll.Value != 0) _allMod = (float)_sliderAll.Value / 100;
            else _allMod = 0;
            if (_sliderLove.Value != 0) _loveMod = (float)_sliderLove.Value / 100;
            else _loveMod = 0;
            if (_sliderFriend.Value != 0) _friendMod = (float)_sliderFriend.Value / 100;
            else _friendMod = 0;
            if (_sliderDistant.Value != 0) _distantMod = (float)_sliderDistant.Value / 100;
            else _distantMod = 0;
            if (_sliderHate.Value != 0) _hateMod = (float)_sliderHate.Value / 100;
            else _hateMod = 0;
        }

        public static void MakeFavorabilityCanvas(UnityEngine.SceneManagement.Scene scene)
        {                
            // Creating Canvas object
            FavorCanvasObject = new GameObject("FavorabilityCanvas");
            SceneManager.MoveGameObjectToScene(FavorCanvasObject, scene);
            FavorCanvasObject.AddComponent<FavorabilityCanvas>();

            if (_devLog.Value) Log.LogInfo($"Favorability Canvas Created");           
        }

        public class FavorabilityCanvas : MonoBehaviour
        {
            // Constructor needed to use Start, Update, etc...
            public FavorabilityCanvas(System.IntPtr handle) : base(handle) { }

            private static GameObject favorabilityObject;
            private static TextMeshProUGUI favorInfo;

            private static T GetResource<T>(string name) where T : UnityEngine.Object
            {
                var objs = Resources.FindObjectsOfTypeAll(Il2CppInterop.Runtime.Il2CppType.Of<T>());
                for (var i = objs.Length - 1; i >= 0; --i)
                {
                    var obj = objs[i];
                    if (obj.name == name)
                    {
                        var ret = obj.TryCast<T>();
                        return ret;
                    }
                }
                return null;
            }
            void Start()
            {
                // Setting canvas attributes
                var canvasScaler = FavorCanvasObject.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);

                Canvas canvas = FavorCanvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 520;
                FavorCanvasObject.AddComponent<CanvasGroup>().blocksRaycasts = false;

                // Setting subtitle object
                favorabilityObject = new GameObject("XUAIGNORE FavorabilityText");
                favorabilityObject.transform.SetParent(FavorCanvasObject.transform);

                int fontSize = (int)(Screen.height / 60.0f);

                RectTransform favorRect = favorabilityObject.AddComponent<RectTransform>();
                favorRect.anchoredPosition = new Vector2(10, Screen.height/-8);
                favorRect.anchorMax = new Vector2(0, 1f);
                favorRect.anchorMin = new Vector2(0, 1f);
                favorRect.pivot = new Vector2(0, 1);
                favorRect.sizeDelta = new Vector2(Screen.width, Screen.height);
                //favorRect.sizeDelta = new Vector2(Screen.width * 0.990f, fontSize + (fontSize * 0.05f));

                favorInfo = favorabilityObject.AddComponent<TextMeshProUGUI>();
                favorInfo.font = GetResource<TMP_FontAsset>("tmp_sv_default");
                favorInfo.fontSharedMaterial = GetResource<Material>("tmp_sv_default Material");

                favorInfo.fontSize = fontSize;
                favorInfo.alignment = TextAlignmentOptions.TopLeft;
                favorInfo.overflowMode = TextOverflowModes.Overflow;
                favorInfo.enableWordWrapping = true;
                favorInfo.color = Color.white;
                favorInfo.text = "";
            }

            // Using Update because coroutines, onDestroy and onDisable are not working as intended
            private void Update()
            {
                if (_enableShowValues.Value)
                {
                    if (DiagramSceneInstance._isOpen)
                    {
                        favorabilityObject.active = true;

                        int _charaSelected = DiagramSceneInstance._charaListView._nowSelect.Value;

                        if (_charaSelected != _currentSelected)
                        {
                            _currentSelected = _charaSelected;

                            var favorText = "";

                            //_charaList = _gameInstance.Charas.Count;                 

                            var _charaInfo = DiagramSceneInstance._charaStatusView._actor;
                            //var _actorID = HelperFunctions.TryGetActorId(_charaInfo);
                            var _actorID = _charaInfo.charasGameParam._Index_k__BackingField;

                            TranslationHelper.TryTranslate(_charaInfo.Name, out string translatedName);
                            var _name = translatedName;

                            if (_name == null)
                            {
                                _name = _charaInfo.Name;
                            }

                            if (_devLog.Value) Log.LogInfo($"Reading Favorability for: " + _actorID + " Name: " + _name);

                            if (_actorID < 10) favorText = "Selected: " + "0"+_actorID + " " + _name + "\n" + "Favorability: Love/Friend/Distant/Hate \n";
                            else favorText = "Selected: " + _actorID + " " + _name + "\n" + "Favorability: Love/Friend/Distant/Hate \n";

                            foreach (var chara in _gameInstance.Charas)
                            {
                                if (_actorID != chara.Key)
                                {
                                    _charaInfo = _gameInstance.Charas[chara.Key];
                                    if (chara.Key < 10)
                                    {
                                        TranslationHelper.TryTranslate(_charaInfo.Name, out string translatedCharaName);
                                        _name = translatedCharaName;

                                        if (_name == null)
                                        {
                                            _name = _charaInfo.Name;
                                        }

                                        favorText = favorText + "0" + chara.Key + ": " +
                                            _charaInfo.charasGameParam.sensitivity.tableFavorabiliry[_actorID].longSensitivityCounts[0] + "/" +
                                            _charaInfo.charasGameParam.sensitivity.tableFavorabiliry[_actorID].longSensitivityCounts[1] + "/" +
                                            _charaInfo.charasGameParam.sensitivity.tableFavorabiliry[_actorID].longSensitivityCounts[2] + "/" +
                                            _charaInfo.charasGameParam.sensitivity.tableFavorabiliry[_actorID].longSensitivityCounts[3] + " | " + _name + "\n";
                                    }
                                    else
                                    {
                                        TranslationHelper.TryTranslate(_charaInfo.Name, out string translatedCharaName);
                                        _name = translatedCharaName;

                                        if (_name == null) _name = _charaInfo.Name;                   

                                        favorText = favorText + chara.Key + ": " +
                                            _charaInfo.charasGameParam.sensitivity.tableFavorabiliry[_actorID].longSensitivityCounts[0] + "/" +
                                            _charaInfo.charasGameParam.sensitivity.tableFavorabiliry[_actorID].longSensitivityCounts[1] + "/" +
                                            _charaInfo.charasGameParam.sensitivity.tableFavorabiliry[_actorID].longSensitivityCounts[2] + "/" +
                                            _charaInfo.charasGameParam.sensitivity.tableFavorabiliry[_actorID].longSensitivityCounts[3] + " | " + _name + "\n";
                                    }
                                }
                            }
                            if (!_hideShowValuesdesc.Value)
                            {
                                favorText = favorText + "\n[The max amount of points is 30 Total]\r\nFor more information check the\r\nSummer Vacation! Scramble wiki";
                            }
                            favorInfo.text = favorText;
                        }
                    }
                    else
                    {
                        favorabilityObject.active = false;
                        _currentSelected = -1;
                    }
                }
                else if (favorabilityObject.active) favorabilityObject.active = false;
            }
        }
        internal static class Hooks
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(FavourableImpressionManager), nameof(FavourableImpressionManager.IndividualityCorrection))]
            public static void OverrideRates(FavourableImpressionManager __instance, bool _isActive, bool _isOneWay, HumanData _myCharaData, CharactersGameParameter _myGameParam, HumanData _targetCharaData, CharactersGameParameter _targetGameParam)
            {
                if (_devLog.Value)
                {
                    Log.LogInfo($"New Rate " + __instance.addRates[0]);
                    Log.LogInfo($"New Rate " + __instance.addRates[1]);
                    Log.LogInfo($"New Rate " + __instance.addRates[2]);
                    Log.LogInfo($"New Rate " + __instance.addRates[3]);

                    Log.LogInfo($"--------------------------------");
                }

                if (_removeFirstImpression.Value)
                {
                    __instance = FavorabilityMod.RemoveFirstImpression(__instance, _myCharaData, _myGameParam, _targetCharaData, _targetGameParam);
                    /*if (_myGameParam.sensitivity.tableFavorabiliry.ContainsKey(_targetGameParam._Index_k__BackingField))
                    {
                        if (_myGameParam.sensitivity.tableFavorabiliry[_targetGameParam._Index_k__BackingField].longStocks.Count < 21)
                        {
                            __instance.addRates[0] = 1f;
                            __instance.addRates[1] = 1f;
                            __instance.addRates[2] = 1f;
                            __instance.addRates[3] = 1f;

                            switch (_myCharaData.GameParameter.sexualTarget)
                            {
                                case 0: //Hetero
                                    if (_myCharaData.Parameter.sex != _targetCharaData.Parameter.sex)
                                    {
                                        __instance.addRates[1] = 0;
                                    }
                                    else
                                    {
                                        __instance.addRates[0] = 0;
                                    }
                                    break;

                                case 1: //Lean hetero
                                    if (_myCharaData.Parameter.sex != _targetCharaData.Parameter.sex)
                                    {
                                        __instance.addRates[1] = 0.5f;
                                    }
                                    else
                                    {
                                        __instance.addRates[0] = 0.5f;
                                    }
                                    break;
                                case 3: //Lean Homo
                                    if (_myCharaData.Parameter.sex == _targetCharaData.Parameter.sex)
                                    {
                                        __instance.addRates[1] = 0.5f;
                                    }
                                    else
                                    {
                                        __instance.addRates[0] = 0.5f;
                                    }
                                    break;

                                case 4://Homo
                                    if (_myCharaData.Parameter.sex == _targetCharaData.Parameter.sex)
                                    {
                                        __instance.addRates[1] = 0;
                                    }
                                    else
                                    {
                                        __instance.addRates[0] = 0f;
                                    }
                                    break;
                            }
                        }
                    }*/
                }

                if (_heteroHomoFriendGain.Value)
                {
                    __instance = FavorabilityMod.SetHeteroHomoFriendGain(__instance, _myCharaData, _targetCharaData);
                    /*if (_myCharaData.GameParameter.sexualTarget == 0)
                    {
                        if (_myCharaData.Parameter.sex != _targetCharaData.Parameter.sex)
                        {
                            __instance.addRates[1] = 0.5f;
                        }
                    }

                    if (_myCharaData.GameParameter.sexualTarget == 4)
                    {
                        if (_myCharaData.Parameter.sex == _targetCharaData.Parameter.sex)
                        {
                            __instance.addRates[1] = 0.5f;
                        }
                    }*/
                }

                if (_devLog.Value)
                {
                    Log.LogInfo($"New Rate " + __instance.addRates[0]);
                    Log.LogInfo($"New Rate " + __instance.addRates[1]);
                    Log.LogInfo($"New Rate " + __instance.addRates[2]);
                    Log.LogInfo($"New Rate " + __instance.addRates[3]);

                    Log.LogInfo($"--------------------------------");
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SensitivityParameter), nameof(SensitivityParameter.AddFavor))]
            public static void AddFavorOverridePointsGain(SensitivityParameter __instance, MemoryParameter _memory, int _targetCharaID, Il2CppStructArray<int> _favors)
            {
                var _CharaInfo = Game._saveData_k__BackingField.Charas[_targetCharaID];
                int _length = _CharaInfo.gameParameter.individuality.answer.Length;
                if (_devLog.Value)
                {
                    Log.LogInfo($"*********************************");
                    Log.LogInfo($"Chara Name: " + _CharaInfo.Name);
                    Log.LogInfo($"Target Chara ID:" + _targetCharaID);
                    //Log.LogInfo($"Points Gained" + _favors.Length);
                    Log.LogInfo($"Love gain: " + _favors[0]);
                    Log.LogInfo($"Friend gain: " + _favors[1]);
                    Log.LogInfo($"Distant gain: " + _favors[2]);
                    Log.LogInfo($"Hate gain: " + _favors[3]);
                    Log.LogInfo($"---------------------------------");
                }

                SetModifiers();
                if (_favors.Length > 3)
                {
                    if (_indiFavorEnable.Value)
                    {
                        if (_favors[0] != 0)
                        {
                            _favors[0] = (int)Math.Round(_favors[0] * _loveMod);
                            if ((_sensitivityMin.Value & SensitivityType.Love) != 0)
                            {
                                if (_favors[0] < _absoluteMin.Value && _loveMod != 0) _favors[0] = _absoluteMin.Value;
                            }

                            if ((_sensitivityMax.Value & SensitivityType.Love) != 0)
                            {
                                if (_favors[0] > _absoluteMax.Value) _favors[0] = _absoluteMax.Value;
                            }
                        }

                        if (_favors[1] != 0)
                        {
                            _favors[1] = (int)Math.Round(_favors[1] * _friendMod);
                            if ((_sensitivityMin.Value & SensitivityType.Friend) != 0)
                            {
                                if (_favors[1] < _absoluteMin.Value && _friendMod != 0) _favors[1] = _absoluteMin.Value;
                            }

                            if ((_sensitivityMax.Value & SensitivityType.Friend) != 0)
                            {
                                if (_favors[1] > _absoluteMax.Value) _favors[1] = _absoluteMax.Value;
                            }
                        }

                        if (_favors[2] != 0)
                        {
                            _favors[2] = (int)Math.Round(_favors[2] * _distantMod);
                            if ((_sensitivityMin.Value & SensitivityType.Distant) != 0)
                            {
                                if (_favors[2] < _absoluteMin.Value && _distantMod != 0) _favors[2] = _absoluteMin.Value;
                            }

                            if ((_sensitivityMax.Value & SensitivityType.Distant) != 0)
                            {
                                if (_favors[2] > _absoluteMax.Value) _favors[2] = _absoluteMax.Value;
                            }
                        }

                        if (_favors[3] != 0)
                        {
                            _favors[3] = (int)Math.Round(_favors[3] * _hateMod);
                            if ((_sensitivityMin.Value & SensitivityType.Hate) != 0)
                            {
                                if (_favors[3] < _absoluteMin.Value && _hateMod != 0) _favors[3] = _absoluteMin.Value;
                            }

                            if ((_sensitivityMax.Value & SensitivityType.Hate) != 0)
                            {
                                if (_favors[3] > _absoluteMax.Value) _favors[3] = _absoluteMax.Value;
                            }
                        }
                    }
                    else
                    {
                        if (_favors[0] != 0)
                        {
                            _favors[0] = (int)Math.Round(_favors[0] * _allMod);
                            if ((_sensitivityMin.Value & SensitivityType.Love) != 0)
                            {
                                if (_favors[0] < _absoluteMin.Value && _allMod != 0) _favors[0] = _absoluteMin.Value;
                            }

                            if ((_sensitivityMax.Value & SensitivityType.Love) != 0)
                            {
                                if (_favors[0] > _absoluteMax.Value) _favors[0] = _absoluteMax.Value;
                            }
                        }

                        if (_favors[1] != 0)
                        {
                            _favors[1] = (int)Math.Round(_favors[1] * _allMod);
                            if ((_sensitivityMin.Value & SensitivityType.Friend) != 0)
                            {
                                if (_favors[1] < _absoluteMin.Value && _allMod != 0) _favors[1] = _absoluteMin.Value;
                            }

                            if ((_sensitivityMax.Value & SensitivityType.Friend) != 0)
                            {
                                if (_favors[1] > _absoluteMax.Value) _favors[1] = _absoluteMax.Value;
                            }
                        }

                        if (_favors[2] != 0)
                        {
                            _favors[2] = (int)Math.Round(_favors[2] * _allMod);
                            if ((_sensitivityMin.Value & SensitivityType.Distant) != 0)
                            {
                                if (_favors[2] < _absoluteMin.Value && _allMod != 0) _favors[2] = _absoluteMin.Value;
                            }

                            if ((_sensitivityMax.Value & SensitivityType.Distant) != 0)
                            {
                                if (_favors[2] > _absoluteMax.Value) _favors[2] = _absoluteMax.Value;
                            }
                        }

                        if (_favors[3] != 0)
                        {
                            _favors[3] = (int)Math.Round(_favors[3] * _allMod);
                            if ((_sensitivityMin.Value & SensitivityType.Hate) != 0)
                            {
                                if (_favors[3] < _absoluteMin.Value && _allMod != 0) _favors[3] = _absoluteMin.Value;
                            }

                            if ((_sensitivityMax.Value & SensitivityType.Hate) != 0)
                            {
                                if (_favors[3] > _absoluteMax.Value) _favors[3] = _absoluteMax.Value;
                            }
                        }
                    }

                    if (_devLog.Value)
                    {
                        if (_indiFavorEnable.Value)
                        {
                            Log.LogInfo($"Set Values Independently");
                            Log.LogInfo($"---------------------------------");
                            Log.LogInfo($"Modifiers");
                            Log.LogInfo($"ModLove: " + _loveMod);
                            Log.LogInfo($"ModFriend: " + _friendMod);
                            Log.LogInfo($"ModDistant: " + _distantMod);
                            Log.LogInfo($"ModHate: " + _hateMod);
                        }
                        else
                        {
                            Log.LogInfo($"Set All Values");
                            Log.LogInfo($"---------------------------------");
                            Log.LogInfo($"Modifier");
                            Log.LogInfo($"ModAll: " + _allMod);
                        }

                        Log.LogInfo($"---------------------------------");
                        Log.LogInfo($"Edited Values");
                        Log.LogInfo($"Love gain: " + _favors[0]);
                        Log.LogInfo($"Friend gain: " + _favors[1]);
                        Log.LogInfo($"Distant gain: " + _favors[2]);
                        Log.LogInfo($"Hate gain: " + _favors[3]);
                        Log.LogInfo($"*********************************");
                    }
                }

                switch (_favorMode.Value)
                {
                    case FavorModes.Normal:
                        break;

                    case FavorModes.FullPoint:
                        {
                            //Love
                            _roundedValue = (float)_favors[0] / 30;
                            _favors[0] = (int)(Math.Ceiling(_roundedValue) * 30);
                            //Friend
                            _roundedValue = (float)_favors[1] / 30;
                            _favors[1] = (int)(Math.Ceiling(_roundedValue) * 30);
                            //Distant
                            _roundedValue = (float)_favors[2] / 30;
                            _favors[2] = (int)(Math.Ceiling(_roundedValue) * 30);
                            //Hate
                            _roundedValue = (float)_favors[3] / 30;
                            _favors[3] = (int)(Math.Ceiling(_roundedValue) * 30);

                            if (_devLog.Value)
                            {
                                Log.LogInfo($"/////////////////////////////////");
                                Log.LogInfo($"FavorModes.FullPoint");

                                Log.LogInfo($"Love gain: " + _favors[0]);
                                Log.LogInfo($"Friend gain: " + _favors[1]);
                                Log.LogInfo($"Distant gain: " + _favors[2]);
                                Log.LogInfo($"Hate gain: " + _favors[3]);
                                Log.LogInfo($"/////////////////////////////////");
                            }

                        }
                        break;

                    case FavorModes.Reverse:
                        {
                            _tempL = _favors[0];
                            _tempF = _favors[1];
                            _tempD = _favors[2];
                            _tempH = _favors[3];

                            _favors[3] = _tempL;
                            _favors[2] = _tempF;
                            _favors[1] = _tempD;
                            _favors[0] = _tempH;

                            if (_devLog.Value)
                            {
                                Log.LogInfo($"/////////////////////////////////");
                                Log.LogInfo($"FavorModes.Reverse");

                                Log.LogInfo($"Love gain: " + _favors[0]);
                                Log.LogInfo($"Friend gain: " + _favors[1]);
                                Log.LogInfo($"Distant gain: " + _favors[2]);
                                Log.LogInfo($"Hate gain: " + _favors[3]);
                                Log.LogInfo($"/////////////////////////////////");
                            }

                        }
                        break;

                    case FavorModes.Random:
                        {
                            _randomTotal = _favors[0] + _favors[1] + _favors[2] + _favors[3];

                            if (_randomTotal != 0)
                            {
                                if (_favors[0] != 0) _favors[0] = _rnd.Next(0, _randomTotal);
                                if (_favors[1] != 0) _favors[1] = _rnd.Next(0, _randomTotal);
                                if (_favors[2] != 0) _favors[2] = _rnd.Next(0, _randomTotal);
                                if (_favors[3] != 0) _favors[3] = _rnd.Next(0, _randomTotal);

                                if (_devLog.Value)
                                {
                                    Log.LogInfo($"/////////////////////////////////");
                                    Log.LogInfo($"FavorModes.Random");
                                    Log.LogInfo($"Love gain: " + _favors[0]);
                                    Log.LogInfo($"Friend gain: " + _favors[1]);
                                    Log.LogInfo($"Distant gain: " + _favors[2]);
                                    Log.LogInfo($"Hate gain: " + _favors[3]);
                                    Log.LogInfo($"/////////////////////////////////");
                                }
                            }
                        }
                        break;

                    case FavorModes.Unforeseen:
                        {
                            _unforseen = _rnd.Next(0, 100);
                            if (_unforseen < 10)
                            {
                                _unforseen = _rnd.Next(0, 3);
                                _favors[_unforseen] = 300;
                            }
                            break;
                        }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CorrelationDiagram), nameof(CorrelationDiagram.SetOpenCloseEvent))]
            public static void ShowFavorabilityPoints(CorrelationDiagram __instance, bool open)
            {
                if (_enableShowValues.Value)
                {
                    if (!FavorCanvasObject) MakeFavorabilityCanvas(SceneManager.GetActiveScene());

                    if (open)
                    {
                        _gameInstance = Game._saveData_k__BackingField;
                        DiagramSceneInstance = __instance;
                        //FavorabilityCanvas.GetValues();
                    }
                }
            }
        }
    }
}
