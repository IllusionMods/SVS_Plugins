using ADV.Commands.Game.LowChara;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SV;
using SV.Chara;
using SV.Talk;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SVS_SixthSense
{
    [BepInPlugin(GUID, DisplayName, Version)]
    public class SixthSensePlugin : BasePlugin
    {
        public const string DisplayName = "SVS_SixthSense";
        public const string GUID = "SVS_SixthSense";
        public const string Version = "1.2.2";

        internal static new ManualLogSource Log;
        private static Harmony patchedHooks;

        private static ConfigEntry<int> textTimer;

        private static ConfigEntry<bool> debugLog;

        private static ConfigEntry<bool> enableSisxthSense;
        private static ConfigEntry<bool> enableSexSense;
        private static ConfigEntry<bool> enableblackmailSense;
        private static ConfigEntry<bool> enableMasturbationSense;
        private static ConfigEntry<bool> enableFightingSense;
        private static ConfigEntry<bool> isAmbiguous;
        private static ConfigEntry<bool> enableMoodSense;
        private static ConfigEntry<Color> textColor { get; set; }
        private static ConfigEntry<Color> underlayColor { get; set; }

        private static GameObject CustomCanvasObject;

        public override void Load()
        {
            Log = base.Log;

            var colorConverter = new TypeConverter
            {
                ConvertToString = (obj, type) => ColorUtility.ToHtmlStringRGBA((Color)obj),
                ConvertToObject = (str, type) =>
                {
                    if (!ColorUtility.TryParseHtmlString("#" + str.Trim('#', ' '), out var c))
                        throw new FormatException("Invalid color string, expected hex #RRGGBBAA");
                    return c;
                }
            };
            TomlTypeConverter.AddConverter(typeof(Color), colorConverter);

            enableSisxthSense = Config.Bind("General", "Enable Sixth Sense", true, new ConfigDescription("Enable or disable this mod", null, new ConfigurationManagerAttributes { Order = 10 }));
            debugLog = Config.Bind("General", "Enable Log", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 10 }));

            textTimer = Config.Bind("Sixth Sense", "Text Timer", 10, new ConfigDescription("Set how much time the text will be displayed in seconds before disappearing", new AcceptableValueRange<int>(5, 60), new ConfigurationManagerAttributes { Order = 9 }));

            enableSexSense = Config.Bind("Sixth Sense", "Enable Sex Sensor", true, new ConfigDescription("If enable, you will know when a NPC is having sex and where", null, new ConfigurationManagerAttributes { Order = 8 }));
            enableblackmailSense = Config.Bind("Sixth Sense", "Blackmail Sensor", true, new ConfigDescription("If enable, you will know when a NPC is getting blackmailed", null, new ConfigurationManagerAttributes { Order = 7 }));
            enableMasturbationSense = Config.Bind("Sixth Sense", "Masturbation Sensor", true, new ConfigDescription("If enable, you will know when a NPC is masturbating and where", null, new ConfigurationManagerAttributes { Order = 6 }));
            enableFightingSense = Config.Bind("Sixth Sense", "Fight Sensor", true, new ConfigDescription("If enable, you will know when a NPC are fighting and where", null, new ConfigurationManagerAttributes { Order = 6 }));

            enableMoodSense = Config.Bind("Mood Sense", "Mood Sensor", true, new ConfigDescription("If enable, you will know the mood of the character you are selecting in the overworld and during interactions \nIf your playable character knowns the NPC well", null, new ConfigurationManagerAttributes { Order = 10 }));

            isAmbiguous = Config.Bind("Sixth Sense", "Ambiguity", false, new ConfigDescription("If enable, the character name will be hidden", null, new ConfigurationManagerAttributes { Order = 2 }));
            textColor = Config.Bind("Sixth Sense", "Text Color", new Color(1, 1, 1, 1f), new ConfigDescription("Color of text", null, new ConfigurationManagerAttributes { Order = 1 }));
            underlayColor = Config.Bind("Sixth Sense", "Underlay Color", new Color(0, 0, 0, 1f), new ConfigDescription("Color of text", null, new ConfigurationManagerAttributes { Order = 0 }));

            patchedHooks = Harmony.CreateAndPatchAll(typeof(Hooks));
        }
        public override bool Unload()
        {
            patchedHooks?.UnpatchSelf();
            return true;
        }

        public static bool[] GetSettings()
        {
            return new bool [] { isAmbiguous.Value };
        }

        public static bool IsDebugLog()
        {
            return debugLog.Value;
        }
        public static List<Color> GetTextColor()
        {
            List<Color> colorsList = new List<Color>();
            colorsList.Add(textColor.Value);
            colorsList.Add(underlayColor.Value);
            return colorsList;
        }
        internal static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(SimulationScene), nameof(SimulationScene.Start))]
            public static void CreateSixthSenseObject(SimulationScene __instance)
            {
                if (!CustomCanvasObject) SixthSense.MakeCanvas(SceneManager.GetActiveScene(), CustomCanvasObject, __instance);
                SixthSense.CreateCharaMoodImage(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CommandUI), nameof(CommandUI.Start))]
            public static void CreateSixthSenseObjectInADV(CommandUI __instance)
            {
                SixthSense.CreateCharaMoodImageInADV(__instance);
            }

            static bool startCounter = false;
            static float counter = 0;
            static bool isCharaName = false;
            static string currentName = "";
            static bool isMoodVisible = true;
            [HarmonyPostfix]
            [HarmonyPatch(typeof(SimulationScene), nameof(SimulationScene.Update))]
            public static void HideCounter(SimulationScene __instance)
            {
                if (startCounter)
                {
                    if (counter > textTimer.Value)
                    {
                        SixthSense.HideText();
                        startCounter = false;
                        return;
                    }
                    counter += Time.deltaTime;
                }
                if (!enableMoodSense.Value && isMoodVisible)
                {
                    isMoodVisible = false;
                    SixthSense.ShowMood(enableMoodSense.Value);
                }
                else if (!isMoodVisible && enableMoodSense.Value)
                {
                    isMoodVisible = true;
                    SixthSense.ShowMood(enableMoodSense.Value);
                }
                if (isCharaName && enableMoodSense.Value)
                {
                    SixthSense.ShowCharacterMood(currentName);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SimulationUICtrl), nameof(SimulationUICtrl.SetTargetCharaName))]
            public static void GetSelectedCharaName(SimulationUICtrl __instance, string _targetName)
            {
                if (currentName != _targetName) currentName = _targetName;
                if (_targetName != "") isCharaName = true;
                else isCharaName = false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(LowPolyHMotionPlay), nameof(LowPolyHMotionPlay.Do))]
            public static void CheckNPCSexAnim(LowPolyHMotionPlay __instance)
            {
                if (enableSexSense.Value && enableSisxthSense.Value)
                {
                    var tempChara = __instance.LowPolyChara;
                    if (tempChara != null)
                    {
                        SixthSense.ShowHavingSexText(tempChara);                    
                        counter = 0;
                        startCounter = true;
                    }
                }            
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(LowPolyFightMotion), nameof(LowPolyFightMotion.Do))]
            public static void CheckNPCFightAnim(LowPolyFightMotion __instance)
            {
                if (enableFightingSense.Value && enableSisxthSense.Value)
                {
                    var tempChara = __instance.LowPolyChara;
                    if (tempChara != null)
                    {
                        SixthSense.ShowFightingText(tempChara);
                        counter = 0;
                        startCounter = true;
                    }
                }
            }

            /*[HarmonyPostfix]
            [HarmonyPatch(typeof(SVIdleSetMotion), nameof(SVIdleSetMotion.OnUpdate))]
            public static void IdleAnimationCheck(SVIdleSetMotion __instance)
            {
                if (__instance != null) Log.LogInfo($"OnUpdate! POST {__instance._charaCtrl.nowAnimPtn}");               
            }*/

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SVIdleSetMotion), nameof(SVIdleSetMotion.IsMasturbation))]
            public static void MasturbationCheck(SVIdleSetMotion __instance, bool __result)
            {
                if (__result)
                {
                    if (enableMasturbationSense.Value && enableSisxthSense.Value)
                    {
                        SixthSense.ShowNPCMasturbatingText(__instance.CharaCtrl);
                        counter = 0;
                        startCounter = true;
                    }                      
                } 
            }

            [HarmonyPriority(100)]
            [HarmonyPostfix]
            [HarmonyPatch(typeof(BaseAnswer), nameof(BaseAnswer.Judge))]
            public static void ActionAnswerRate(bool __result, YesNoJudgeManager.AnswerInfo _ansInfo, YesNoJudgeManager.YesNoInfo _ynInfo, int _commandID, int _questionCount, Il2CppStructArray<bool> _calcs)
            {
                if (__result)
                {
                    if (enableblackmailSense.Value && enableSisxthSense.Value)
                    {
                        if (_ynInfo.active.charasGameParam.isPC || _ynInfo.passive.charasGameParam.isPC) return;
                        if (_commandID == 78 || _commandID == 79 || _commandID == 80 || _commandID == 81)
                        {
                            SixthSense.ShowCommandText(_ynInfo.passive, _ynInfo.active, _commandID);
                            counter = 0;
                            startCounter = true;
                        }
                    }
                }                          
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TalkTaskBase), nameof(TalkTaskBase.ADVStartInPlayer))]
            public static void ShowMoodInADV(TalkTaskBase __instance, string _advAsset, int _charaID, int _category, int _playerAction, AI _player,AI _npc, AI _third, AI _fourth, AI _fifth)
            {
                if (IsDebugLog()) Log.LogInfo("Starting ADV!");
                if (_npc != null && enableMoodSense.Value)
                {
                    if (IsDebugLog()) Log.LogInfo($"Setting mood icon for CharaID: {_npc.charaData.charasGameParam.Index}");
                    SixthSense.ShowCharaMoodInADV(_npc);
                }
            }
        }
    }
}
