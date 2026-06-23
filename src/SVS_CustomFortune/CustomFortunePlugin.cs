using ADV;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Character;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SaveData;
using SV;
using SV.H.UI;
using SV.MyRoomScene;
using SV.Title;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;


namespace SVS_CustomFortune
{
    [BepInPlugin(GUID, DisplayName, Version)]
    public class CustomFortunePlugin : BasePlugin
    {
        public const string DisplayName = Constants.Name;
        public const string GUID = "SVS_CustomFortune";
        public const string Version = Constants.Version;

        internal static new ManualLogSource Log;
        private static Harmony patchedHooks;

        public static string CustomMapListDirectory { get; } = Path.Combine(Paths.GameRootPath, "abdata\\map\\CustomMaps");

        private static ConfigEntry<bool> enableCustomFortunes;
        private static ConfigEntry<bool> enableWackyFortunes;
        private static ConfigEntry<bool> forceFortune;

        private static ConfigEntry<int> fortuneValue;
        public override void Load()
        {
            Log = base.Log;

            enableCustomFortunes = Config.Bind("Custom Fortune", "Use Custom Fortunes", true, new ConfigDescription("Enable this mod, Need restart for this to take effect", null, new ConfigurationManagerAttributes { Order = 10 }));
            //enableWackyFortunes = Config.Bind("Custom Fortune", "Use Wacky Fortunes", true, new ConfigDescription("Enable this mod, Need restart for this to take effect", null, new ConfigurationManagerAttributes { Order = 3 }));
            //forceFortune = Config.Bind("Custom Fortune", "Use Custom Fortunes", true, new ConfigDescription("Enable this mod, Need restart for this to take effect", null, new ConfigurationManagerAttributes { Order = 10 }));

            patchedHooks = Harmony.CreateAndPatchAll(typeof(Hooks));
        }
        public override bool Unload()
        {
            patchedHooks?.UnpatchSelf();
            return true;
        }
        internal static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(TitleScene), nameof(TitleScene.Start))]
            public static void LoadCustomFortunes()
            {
                if (!enableCustomFortunes.Value) return;
                if (GlobalListLoad.Instance._divinationEffectsTable.Count == 9)
                {
                    CustomFortuneFunctions.CustomFortunesInit();
                }
            }

            static int currentFortune = -2;
            [HarmonyPostfix]
            [HarmonyPatch(typeof(MyRoom), nameof(MyRoom.Open))]
            public static void RandomizeFortune()
            {
                if (!enableCustomFortunes.Value) return;
                CustomFortuneFunctions.RandomizeFortuneIDs();
                currentFortune = -2;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ADV.Setup), nameof(ADV.Setup.Open))]
            public static void CustomFortuneScenario(OpenData openData)
            {
                if (!enableCustomFortunes.Value) return;
                //Log.LogInfo($"Setup Open {openData.HasData} - Asset: {openData.Asset} - Bundle{openData.Bundle}");
                if (CustomFortuneFunctions.IsCustomFortune(openData.Asset, out int fortuneID))
                {
                    //Log.LogInfo($"Is Custom Fortune");
                    openData = CustomFortuneFunctions.SetCustomFortuneScenario(openData, fortuneID);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(FortuneUI), nameof(FortuneUI.DoOpen))]
            public static void CustomFortuneSpritesAndText(FortuneUI __instance, bool isCircularNoticeUse, int id)
            {
                if (!enableCustomFortunes.Value) return;
                if (currentFortune != id)
                {
                    CustomFortuneFunctions.SetCustomFortuneSpriteAndText(__instance, id);
                    currentFortune = id;
                }
            }

            [HarmonyPriority(450)]
            [HarmonyPrefix]
            [HarmonyPatch(typeof(SensitivityParameter), nameof(SensitivityParameter.AddFavor))]
            public static void CustomFortuneFavors(SensitivityParameter __instance, MemoryParameter _memory, int _targetCharaID, Il2CppStructArray<int> _favors)
            {
                if (!enableCustomFortunes.Value) return;
                _favors = CustomFortuneFunctions.SetFortuneFavorability(_favors);
            }

            [HarmonyPriority(399)]
            [HarmonyPostfix]
            [HarmonyPatch(typeof(BaseAnswer), nameof(BaseAnswer.Judge))]
            public static void ActionAnswerRate(BaseAnswer __instance, YesNoJudgeManager.AnswerInfo _ansInfo, YesNoJudgeManager.YesNoInfo _ynInfo, int _commandID, int _questionCount, Il2CppStructArray<bool> _calcs)
            {
                if (!enableCustomFortunes.Value) return;
                _ansInfo = CustomFortuneFunctions.FortuneAnswerRate(_ansInfo, _ynInfo, _commandID, _questionCount);
            }

            [HarmonyPriority(399)]
            [HarmonyPrefix]
            [HarmonyPatch(typeof(ConditionManager), nameof(ConditionManager.CharaAdd))]
            public static void CustomFortuneStates(ConditionManager __instance, Actor _actor, Actor _target, Actor _third, bool _isActive, bool _isEveryone, bool _isIndividualityCalc, Il2CppSystem.Collections.Generic.List<int> _adds)
            {
                if (!enableCustomFortunes.Value) return;
                _adds = CustomFortuneFunctions.SetFortuneStatesPoints(_adds);
            }

            /*[HarmonyPostfix]
            [HarmonyPatch(typeof(SimulationScene), nameof(SimulationScene.Start))]
            public static void GetChangeOfClothesList(SimulationScene __instance)
            {
                CustomFortuneFunctions.CreateOldChangeOfClothesList();
            }

            [HarmonyPriority(401)]
            [HarmonyPrefix]
            [HarmonyPatch(typeof(ThinkingManager), nameof(ThinkingManager.GetChangeOfClothesNum))]
            public static void ChangeClothesPerPeriod(Actor _actor, bool _isStart, int _timezone)
            {
                if (_timezone >= 0)
                {
                    CustomFortuneFunctions.ChangeOutfit(_actor, _isStart, _timezone);
                }
            }*/

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ThinkingManager), nameof(ThinkingManager.InterpersonalCommandSelectionTarget))]
            public static int SetCommandTarget(int __result, Actor _actor, int _commandID)
            {
                if (!enableCustomFortunes.Value) return __result;
                if (__result != -1) __result = CustomFortuneFunctions.SetCharaTarget(_actor, _commandID, __result);
                return __result;
            }

        }
    }
}
