using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Manager;
using SaveData;
using SV;
using SV.Chara;
using SV.CharaSelectScene;
using SV.CommonUI.CharaSelect;
using SV.CorrelationDiagramScene;
using SV.EntryScene;
using SVS_ExpandCharacterRoster.Utils;
using UnityEngine.UI;
using CharaListView = SV.EntryScene.CharaListView;

namespace SVS_ExpandCharacterRoster
{
    [BepInPlugin(GUID, DisplayName, Version)]
    public class ExpandCharacterRosterPlugin : BasePlugin
    {
        public const string DisplayName = Constants.Name;
        public const string GUID = "DS27.SVS.ExpandCharacterRoster";
        public const string Version = Constants.Version;

        internal static new ManualLogSource Log;
        private static Harmony patchedHooks;

        private static ConfigEntry<int> charaLimit;
        //private static ConfigEntry<int> absoluteMax;
        private static ConfigEntry<bool> debugLog;

        private static ConfigEntry<bool> reactionFix;

        private static readonly int absoluteMax = 96;
        public override void Load()
        {
            // Plugin startup logic
            Log = base.Log;
            reactionFix = Config.Bind("Expand Character Roster", "Reaction fix", true, new ConfigDescription("Enable this if a character freeze in place, need to restart the game to apply changes", null, new ConfigurationManagerAttributes { Order = 5 }));
            debugLog = Config.Bind("Debug", "Enable Log", false, new ConfigDescription("If you encounter problems with this mod, enable this try to recreate the problem and then send the LogOutput.log to the mod creator", null, new ConfigurationManagerAttributes { Order = 9 }));
            //absoluteMax = Config.Bind("Expand Character Roster", "Chara Absolute Max", 96, new ConfigDescription("Set the absolute character limit. \nNOTE: DO NOT CHANGE THIS!!!", new AcceptableValueRange<int>(24, 96), new ConfigurationManagerAttributes { IsAdvanced = true, Order = 10 }));
            charaLimit = Config.Bind("Expand Character Roster", "Chara Limit", 54, new ConfigDescription("DO NOT USE!! Use the slider that is inside the character roster menu! \nSet a new character limit.", new AcceptableValueRange<int>(24, 96), new ConfigurationManagerAttributes { IsAdvanced = true, Order = 9 }));

            patchedHooks = Harmony.CreateAndPatchAll(typeof(Hooks));
        }
        public override bool Unload()
        {
            patchedHooks?.UnpatchSelf();
            return true;
        }
        public static int GetCharaLimit()
        {
            return charaLimit.Value;
        }
        public static void SetCharaLimit(int value)
        {
            charaLimit.Value = value;
        }
        public static bool IsDebugLog()
        {
            return debugLog.Value;
        }
        internal static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(SimulationScene), nameof(SimulationScene.Start))]
            public static void InitValues()
            {
                ExpandCharacterRoster.ResetValues();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SimulationScene), nameof(SimulationScene.Update))]
            public static void SetNewMaxChara(SimulationScene __instance)
            {
                //Show the new slots and change the roster UI.               
                ExpandCharacterRoster.SetCharaDisplayMenus(__instance);
                //Load characters in the new slot.
                ExpandCharacterRoster.LoadCharasWithNegatives(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CharaEntry), nameof(CharaEntry.Initialize))]
            public static void CreateSlider(CharaEntry __instance)
            {
                ExpandCharacterRoster.CreateCharaNumSlider(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CharaListViewUI<CharaEntry>.Viewer), nameof(CharaListViewUI<CharaEntry>.Viewer.GetMaxNum))]
            public static int NewMaxNum(int __result)
            {
                //Returns a new max when the game calls this method.
                //This value is mostly used when closing the chara roster menu, it checks if the chara ArrayIndex is less than maxNum. If is not, is shows the warning message.
                //ArrayIndex is the position of the character in any of the characters menus (Chara Entry menu, Correlation menu, Chara Select menu), this value is stored in CharasGameParam inside of Actor.
                if (__result == 24) __result = charaLimit.Value;
                return __result;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CharaListView), nameof(CharaListView.Initialize))]
            public static void IncreaseCharaMaxInRosterMenu(CharaListView __instance)
            {
                //Simple way of increasing the character roster limit for the roster menu
                //_fileList is an Il2CppReferenceArray that contains the Actors, it can have nulls for Actors.
                //_indexData is an Il2CppReferenceArray that containt the character portraits and icons that get displayed in the menu.
                if (__instance._viewer != null)
                {
                    if (__instance._viewer._fileList.Count == 24)
                    {
                        __instance._viewer._fileList = new Il2CppReferenceArray<Actor>(absoluteMax);
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CharaListView), nameof(CharaListView.Initialize))]
            public static void ChangeIcons(CharaListView __instance)
            {
                if (__instance._viewer._bg.Length > 0)
                {
                    var bg = ResourcesLoader.GetSprite(0);
                    if (bg != null)
                    {
                        __instance._viewer._bg._sprites[1] = bg;
                    }
                }

                if (__instance._viewer._tglCharaEntryMode != null)
                {
                    var entryMode = __instance._viewer._tglCharaEntryMode.gameObject;
                    if (entryMode != null)
                    {
                        var img12 = entryMode.transform.Find("img12");
                        if (img12 != null)
                        {
                            var icon12Selected = img12.gameObject.GetComponent<Image>();
                            if (icon12Selected != null)
                            {
                                icon12Selected.sprite = ResourcesLoader.GetSprite(1);
                            }
                            var img12_h = img12.transform.Find("12_h");
                            if (img12_h != null)
                            {
                                var icon12Hover = img12_h.gameObject.GetComponent<Image>();

                                icon12Hover.sprite = ResourcesLoader.GetSprite(2);
                            }
                        }

                        var img24 = entryMode.transform.Find("img24");
                        if (img24 != null)
                        {
                            var iconMaxSelected = img24.gameObject.GetComponent<Image>();
                            if (iconMaxSelected != null)
                            {
                                iconMaxSelected.sprite = ResourcesLoader.GetSprite(3);
                            }
                            var img24_h = img24.transform.Find("24_h");
                            if (img24_h != null)
                            {
                                var iconMaxHover = img24_h.gameObject.GetComponent<Image>();
                                if (iconMaxHover != null)
                                {
                                    iconMaxHover.sprite = ResourcesLoader.GetSprite(4);
                                }
                            }                            
                        }
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CharaListView.Viewer), nameof(CharaListView.Viewer.Initialize), typeof(SexualTargetUI.IndexData<SexualTargetActorUI>))]
            public static void AddCharaMaxInActionMenu(CharaListView.Viewer __instance, SexualTargetUI.IndexData<SexualTargetActorUI> indexData)
            {
                //increase the _indexData if it was done before for roster menu
                if (charaLimit.Value > 24)
                {
                    if (__instance._indexData.Count == 24) __instance._indexData = new Il2CppReferenceArray<SexualTargetUI.IndexData<SexualTargetActorUI>>(absoluteMax);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CharaSelect), nameof(CharaSelect.Initialize))]
            public static void IncreaseCharaMaxInActionMenu(CharaSelect __instance)
            {
                if (__instance._charaListView._viewer._fileList.Count == 24) __instance._charaListView._viewer._fileList = new Il2CppReferenceArray<Actor>(absoluteMax);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SV.CharaSelectScene.CharaListView.Viewer), nameof(SV.CharaSelectScene.CharaListView.Viewer.Initialize), typeof(SexualTargetUI.IndexData<SexualTargetActorUI>))]
            public static void AddCharaMaxInActionMenu(SV.CharaSelectScene.CharaListView.Viewer __instance, SexualTargetUI.IndexData<SexualTargetActorUI> indexData)
            {
                if (charaLimit.Value > 24)
                {
                    if (__instance._indexData.Count == 24) __instance._indexData = new Il2CppReferenceArray<SexualTargetUI.IndexData<SexualTargetActorUI>>(absoluteMax);
                } 
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SV.CorrelationDiagramScene.CharaListView.Viewer), nameof(SV.CorrelationDiagramScene.CharaListView.Viewer.Initialize), typeof(CharaListViewUI<CorrelationDiagram>))]
            public static void IncreaseCharaMaxInCorrelationMenu(SV.CorrelationDiagramScene.CharaListView.Viewer __instance, CharaListViewUI<CorrelationDiagram> listViewUI)
            {
                if (__instance._fileList.Count == 24) __instance._fileList = new Il2CppReferenceArray<Actor>(absoluteMax);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SV.CorrelationDiagramScene.CharaListView.Viewer), nameof(SV.CorrelationDiagramScene.CharaListView.Viewer.Initialize), typeof(SexualTargetUI.IndexData<SexualTargetActorUI>))]
            public static void AddCharaMaxInCorrelationMenu(SV.CorrelationDiagramScene.CharaListView.Viewer __instance, SexualTargetUI.IndexData<SexualTargetActorUI> indexData)
            {
                if (charaLimit.Value > 24)
                {
                    if (__instance._indexData.Count == 24) __instance._indexData = new Il2CppReferenceArray<SexualTargetUI.IndexData<SexualTargetActorUI>>(absoluteMax);
                    if (!indexData.Data.IsActive()) indexData.Data.SetActive(true);
                }
            }

            private static int seat;
            private static int randomSeat;
            private static int seatWith;
            private static int randomSeatWith;

            [HarmonyWrapSafe]
            [HarmonyPrefix]
            [HarmonyPatch(typeof(MapManager), nameof(MapManager.GetClassRoomIDPoint))]
            public static void PreStudyMapHandle(MapManager __instance, int job, int charaIndex) //Handles the study seats for characters with ID above 23
            {
                if (charaIndex > 23 && __instance != null)
                {
                    int currentSeat = charaIndex % 24;                   
                    seat = __instance.PointInfoTable[4].pointList.urouroTable[1].points[currentSeat].urouroDetails[0].ClassroomNo;
                    randomSeat = __instance.PointInfoTable[4].pointList.urouroTable[1].randoms[currentSeat].urouroDetails[0].ClassroomNo;
                    __instance.PointInfoTable[4].pointList.urouroTable[1].points[currentSeat].urouroDetails[0].ClassroomNo = charaIndex;
                    __instance.PointInfoTable[4].pointList.urouroTable[1].randoms[currentSeat].urouroDetails[0].ClassroomNo = charaIndex;

                    seatWith = __instance.PointInfoTable[4].pointList.withTable[1].points[currentSeat].withDetails[0].ClassroomNo;
                    randomSeatWith = __instance.PointInfoTable[4].pointList.withTable[1].randoms[currentSeat].withDetails[0].ClassroomNo;
                    __instance.PointInfoTable[4].pointList.withTable[1].points[currentSeat].withDetails[0].ClassroomNo = charaIndex;
                    __instance.PointInfoTable[4].pointList.withTable[1].randoms[currentSeat].withDetails[0].ClassroomNo = charaIndex;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(MapManager), nameof(MapManager.GetClassRoomIDPoint))]
            public static void PostStudyMapHandle(MapManager __instance, int job, int charaIndex) //Reverse the changes made on the Prefix
            {
                if (charaIndex > 23 && __instance != null)
                {
                    int currentSeat = charaIndex % 24;
                    __instance.PointInfoTable[4].pointList.urouroTable[1].points[currentSeat].urouroDetails[0].ClassroomNo = seat;
                    __instance.PointInfoTable[4].pointList.urouroTable[1].randoms[currentSeat].urouroDetails[0].ClassroomNo = randomSeat;

                    __instance.PointInfoTable[4].pointList.withTable[1].points[currentSeat].withDetails[0].ClassroomNo = seatWith;
                    __instance.PointInfoTable[4].pointList.withTable[1].randoms[currentSeat].withDetails[0].ClassroomNo = randomSeatWith;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ReactionManager), nameof(ReactionManager.Initialize))]
            public static void Test00(ReactionManager __instance)
            {
                if (__instance != null)
                {
                    if (IsDebugLog()) Log.LogInfo($"CalcAI: {__instance.calcAI.Count}");
                    if (reactionFix.Value)
                    {
                        if (__instance.calcAI.Count != 0) __instance.calcAI = new Il2CppReferenceArray<AI>(absoluteMax);
                    } 
                }
            }

            /*[HarmonyPrefix]
            [HarmonyPatch(typeof(ReactionManager), nameof(ReactionManager.ReactionProc))]
            public static void Test01(ReactionManager __instance, AI _ai)
            {
                if (_ai != null)
                {
                    if (IsDebugLog())
                    {
                        Log.LogInfo($"ReactionProc AI ID: {_ai.charaData.charasGameParam.Index}");
                        Log.LogInfo($"ReactionProc AI ArrayIndex: {_ai.charaData.charasGameParam.ArrayIndex}");
                        Log.LogInfo($"ReactionProc AI isReaction: {_ai.charaData.charasGameParam.isReaction}");
                        if (__instance != null)
                        {
                            Log.LogInfo($"ReactionProc tempAi count: {__instance.tempAIs.Count}");
                            if (__instance.tempAIs.Count > 0)
                            {
                                foreach (var tempais in __instance.tempAIs)
                                {
                                    Log.LogInfo($"ReactionProc AI ID: {tempais.charaData.charasGameParam.Index}");
                                    Log.LogInfo($"ReactionProc AI ArrayIndex: {tempais.charaData.charasGameParam.ArrayIndex}");
                                }
                            }
                            Log.LogInfo($"ReactionProc CalcAI: {__instance.calcAI.Count}");
                        }
                    } 
                }
            }*/
        }
    }
}
