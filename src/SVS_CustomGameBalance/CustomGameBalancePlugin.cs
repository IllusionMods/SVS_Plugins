using ADV;
using ADV.Commands.Game.LowChara;
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
using SV.MyRoomScene;
using SV.Title;
using System;
using UnityEngine;

namespace SVS_CustomGameBalance
{
    [BepInPlugin(GUID, DisplayName, Version)]
    public class CustomGameBalancePlugin : BasePlugin
    {
        public const string DisplayName = Constants.Name;
        public const string GUID = "DS27.SVS.CustomGameBalance";
        public const string Version = Constants.Version;

        internal static new ManualLogSource Log;
        private static Harmony patchedHooks;

        internal static ConfigEntry<KeyCode> _toggleKey_AutoPC { get; set; }
        internal static ConfigEntry<KeyCode> _toggleKey_SwitchPCCharacter { get; set; }
        internal static ConfigEntry<KeyCode> _toggleKey_AutoCharaVoice { get; set; }

        private static ConfigEntry<AllowHugging> _allowHugSkinship;
        private static ConfigEntry<CharaType> _interruptPCNPC;
        private static ConfigEntry<CharaType> _actionLowestRatePCNPC;
        private static ConfigEntry<HighVirtueBBQ> _bbqAnswerType;
        private static ConfigEntry<NPCSex> _NPCLowPolySex;
        private static ConfigEntry<DisplayPercentage> _actionPorcentageDisplay;

        private static ConfigEntry<bool> _enableStatsReduction;
        private static ConfigEntry<bool> _disablePCReduction;
        private static ConfigEntry<bool> _enableCheaterEnhancer;
        private static ConfigEntry<bool> _byEndPeriod;
        private static ConfigEntry<bool> _disablePCCheatEnhancer;
        private static ConfigEntry<bool> _forceThreesome;
        private static ConfigEntry<bool> _forceThreesomeNPC;
        private static ConfigEntry<bool> _forceHAction;
        private static ConfigEntry<bool> _enableReactionManager;
        //private static ConfigEntry<bool> _accuratedProbability;
        //private static ConfigEntry<bool> _hideProbability;
        private static ConfigEntry<bool> _enableActionLowestRate;
        private static ConfigEntry<bool> _applyGameFixes;
        private static ConfigEntry<bool> _applyAreYouDatingFix;
        private static ConfigEntry<bool> _doNotSkipNPC;
        private static ConfigEntry<bool> _makePCNonInteractable;
        private static ConfigEntry<bool> _removeActionLimit;
        //private static ConfigEntry<bool> _escapeVoiceFix;
        private static ConfigEntry<bool> _fortuneSuccessFix;
        private static ConfigEntry<bool> increaseNightEvent;
        private static ConfigEntry<bool> forceNightEvent;
        private static ConfigEntry<bool> forceBreakUp;
        private static ConfigEntry<bool> hugLovePointsSwitch;
        //private static ConfigEntry<bool> sexSensor;

        private static ConfigEntry<int> _basePoint;
        private static ConfigEntry<int> _pointAditive;
        private static ConfigEntry<int> _baseHateSubPoints;

        private static ConfigEntry<int> _reactionChanceOverall;
        private static ConfigEntry<int> _reactionChanceH;
        //private static ConfigEntry<int> _reactionChanceMasturbation;
        //private static ConfigEntry<int> _reactionChanceFight;
        private static ConfigEntry<int> _reactionChanceSkinship;
        private static ConfigEntry<int> _reactionChanceNormal;
        //private static ConfigEntry<int> _losingReactionChance;
        //private static ConfigEntry<int> _h2ReactionChance;
        //private static ConfigEntry<int> _h3ReactionChance;

        private static ConfigEntry<int> _NPCSexTimerMin;
        private static ConfigEntry<int> _NPCSexTimerMax;
        //private static ConfigEntry<int> _actionLimit;
        private static ConfigEntry<int> nightChance;
        private static ConfigEntry<int> nightChanceH;

        private static ConfigEntry<float> _pcRunSpeed;
        private static ConfigEntry<float> _pcWalkSpeed;
        private static ConfigEntry<float> _npcRunSpeed;
        private static ConfigEntry<float> _npcWalkSpeed;
        private static ConfigEntry<float> _lowestActionRate;
        private static ConfigEntry<float> _NPCSexSpeed;

        public static bool gameFixes = false;
        public static bool actionLowestRate = false;

        public override void Load()
        {
            //Plugin startup logic
            Log = base.Log;

            //Stats Reduction
            _enableStatsReduction = Config.Bind("Stats Reduction", "Enable Stats Reduction for Characters", true, new ConfigDescription("The characters will lose some points on their Stamina, Study, LifeStyle and Talk stats at the end of the day, the amount of point they lose will depend on what was set on the abilities tab in the Character Maker", null, new ConfigurationManagerAttributes { Order = 3 }));
            _disablePCReduction = Config.Bind("Stats Reduction", "Disable PC stats reduction", true, new ConfigDescription("Disable the stats getting reduced for the Playable Character. By default this option is Enable, which means the Playable Character won't get his/her stats reduce at the end of the day", null, new ConfigurationManagerAttributes { Order = 2 }));
            _basePoint = Config.Bind("Stats Reduction", "Set Base Point Reduction", 45, new ConfigDescription("The max base amount of points a character can lose by the end of the day, it will pick a value between 1 and the assigned value", new AcceptableValueRange<int>(1, 100), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 1 }));
            _pointAditive = Config.Bind("Stats Reduction", "Set multiplier", 15, new ConfigDescription("Set the multiplier for the base value, 10 is equal to 1.1, 50 is equal to 1.5, 100 is equal to 2. Depending of the character stat Level is how much the multiplier affect that stat (Lowest is x4, Low is x3, Normal x2, High x1 and Highest is unaffected by the multiplier)", new AcceptableValueRange<int>(10, 100), new ConfigurationManagerAttributes { Order = 0 }));

            //Interruption Manager / Reaction Manager
            _enableReactionManager = Config.Bind("Interruption Manager", "Enable Interruption Manager", true, new ConfigDescription("If enabled, it will use the sliders below to determined the probability of interruptions from NPCs", null, new ConfigurationManagerAttributes { Order = 12 }));
            _interruptPCNPC = Config.Bind("Interruption Manager", "Reduce Interruptions for", CharaType.PC | CharaType.NPC,
            new ConfigDescription("Set if the reduced interruption rate affect PC, NPC or both", null, new ConfigurationManagerAttributes { Order = 11 }));

            _reactionChanceOverall = Config.Bind("Interruption Manager", "Reduce All Interruptions Rate by %", 0, new ConfigDescription("Reduce the chances for all types of Interruptions by the chosen amount.", new AcceptableValueRange<int>(0, 99), new ConfigurationManagerAttributes { Order = 10 }));
            _reactionChanceNormal = Config.Bind("Interruption Manager", "Reduce Normal Interruptions by %", 0, new ConfigDescription("Reduce the chances for Normal Interruptions by the chosen amount.", new AcceptableValueRange<int>(0, 99), new ConfigurationManagerAttributes { Order = 9 }));
            _reactionChanceSkinship = Config.Bind("Interruption Manager", "Reduce Contact Interruptions by %", 0, new ConfigDescription("Reduce the chances for Contact (Hug, Kiss, Touch) Interruptions by the chosen amount.", new AcceptableValueRange<int>(0, 99), new ConfigurationManagerAttributes { Order = 8 }));
            _reactionChanceH = Config.Bind("Interruption Manager", "Reduce public sex Interruptions by %", 0, new ConfigDescription("Reduce the chances of public sex Interruptions by the chosen amount. Doesn't affect NPCs asking for 3P", new AcceptableValueRange<int>(0, 99), new ConfigurationManagerAttributes { Order = 7 }));
            //_masturbationReactionChance = Config.Bind("Interruption Manager", "Reduce public sex Interruptions by", 0, new ConfigDescription("Reduce the chance for all types of Interruptions", new AcceptableValueRange<int>(0, 99), new ConfigurationManagerAttributes { Order = 6 }));
            //_fightReactionChance = Config.Bind("Interruption Manager", "Reduce public sex Interruptions by", 0, new ConfigDescription("Reduce the chance for all types of Interruptions", new AcceptableValueRange<int>(0, 99), new ConfigurationManagerAttributes { Order = 5 }));
            //_losingReactionChance = Config.Bind("Interruption Manager", "Reduce public sex Interruptions by", 0, new ConfigDescription("Reduce the chance for all types of Interruptions", new AcceptableValueRange<int>(0, 99), new ConfigurationManagerAttributes { Order = 2 }));
            //_h2ReactionChance = Config.Bind("Interruption Manager", "Reduce public sex Interruptions by", 0, new ConfigDescription("Reduce the chance for all types of Interruptions", new AcceptableValueRange<int>(0, 99), new ConfigurationManagerAttributes { Order = 1 }));
            //_h3ReactionChance = Config.Bind("Interruption Manager", "Reduce public sex Interruptions by", 0, new ConfigDescription("Reduce the chance for all types of Interruptions", new AcceptableValueRange<int>(0, 99), new ConfigurationManagerAttributes { Order = 0 }));

            //Night Event
            increaseNightEvent = Config.Bind("Night Event", "Increase Night Event Rate", true, new ConfigDescription("If enabled, the rate of the Night event will be increase. Note: Male characters can not visit you if you play as a female or male character (Illgames did not add events for them)", null, new ConfigurationManagerAttributes { Order = 10 }));
            nightChance = Config.Bind("Night Event", "Normal Night Event Rate", 70, new ConfigDescription("Increase the rate of Normal Night Events. NOTE: The game still has a small chance of not triggering the event, so a value of 100 does not mean the event will always trigger", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = 9 }));
            nightChanceH = Config.Bind("Night Event", "Sex Night Event Rate", 70, new ConfigDescription("Increase the rate of Sex Night Events. NOTE: The game still has a small chance of not triggering the event, so a value of 100 does not mean the event will always trigger", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = 8 }));

            //Action Manager
            _actionPorcentageDisplay = Config.Bind("Action Manager", "Display Probability", DisplayPercentage.Normal,
            new ConfigDescription("Normal: Display percentage values as normal | Real: Display the real percentage value | Hide: Hide the percentage value", null, new ConfigurationManagerAttributes { Order = 9 }));
            _removeActionLimit = Config.Bind("Action Manager", "Remove Action Limit", true, new ConfigDescription("If enabled, the 3 action limit per period will be removed", null, new ConfigurationManagerAttributes { Order = 8 }));
            forceBreakUp = Config.Bind("Action Manager", "Easy Break Ups", false, new ConfigDescription("If enabled, the break up action is always at 100 percent for both PC and NPCs (Only for lovers of course)", null, new ConfigurationManagerAttributes { Order = 7 }));

            //Simulation
            _toggleKey_AutoPC = Config.Bind("Simulation", "Toggle Auto PC", KeyCode.Y, new ConfigDescription("Hotkey to Toggle Auto PC | Auto PC means that your character will act like a NPC. It will take a little bit of time before the character starts to move", null, new ConfigurationManagerAttributes { Order = 13 }));
            _toggleKey_SwitchPCCharacter = Config.Bind("Simulation", "Switch PC Character", KeyCode.U, new ConfigDescription("Hotkey to Switch PC Character | You can Change your character (PC) to any other character present on the map (You can only switch with NPCs that have the blue circle)", null, new ConfigurationManagerAttributes { Order = 12 }));
            _toggleKey_AutoCharaVoice = Config.Bind("Simulation", "Auto PC Voice", KeyCode.O, new ConfigDescription("Hotkey to play a voice line during Auto PC. It does not do anything else", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 11 }));

            _allowHugSkinship = Config.Bind("Simulation", "Hugging does not cause cheating for", AllowHugging.None,
            new ConfigDescription("Set how Hug works in game. None: Hugging works as normal | All Characters: Hugging no longer triggers the Cheating Status | Exclude Some Traits: Hugging triggers Cheating only for some traits (Jealous and Evil) | Lowest Virtue Only: Hugging does not trigger the Cheating Status for Lowest Virtue Characters", null, new ConfigurationManagerAttributes { Order = 10 }));

            _doNotSkipNPC = Config.Bind("Simulation", "NPCs offscreen actions", true, new ConfigDescription("NPCs will no longer skip their actions if they are on a different map", null, new ConfigurationManagerAttributes { Order = 9 }));
            _pcWalkSpeed = Config.Bind("Simulation", "Set PC Walk Speed", 1f, new ConfigDescription("Set the base walking speed of the Playable Character", new AcceptableValueRange<float>(1f, 10f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 8 }));
            _pcRunSpeed = Config.Bind("Simulation", "Set PC Run Speed", 1f, new ConfigDescription("Set the base running speed of the Playable Character", new AcceptableValueRange<float>(1f, 10f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 7 }));
            _npcWalkSpeed = Config.Bind("Simulation", "Set NPC Walk Speed", 1f, new ConfigDescription("Set the base walking speed for NPCs", new AcceptableValueRange<float>(1f, 5f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 6 }));
            _npcRunSpeed = Config.Bind("Simulation", "Set NPC Run Speed", 1f, new ConfigDescription("Set the base running speed for NPCs", new AcceptableValueRange<float>(1f, 5f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 5 }));

            _NPCLowPolySex = Config.Bind("Simulation", "Set NPCs Sex Animation", NPCSex.None,
            new ConfigDescription("The low Poly sex animations for NPCs will use new values for they duration and or speed", null, new ConfigurationManagerAttributes { Order = 4 }));
            _NPCSexTimerMin = Config.Bind("Simulation", "NPCs Sex Duration Min", 10, new ConfigDescription("Set the minimum amount of time a NPC will have sex in seconds! | Note that this is per position and the NPCs will do 3 during H, so the total time is 3 times this value", new AcceptableValueRange<int>(10, 590), new ConfigurationManagerAttributes { Order = 3 }));
            _NPCSexTimerMax = Config.Bind("Simulation", "NPCs Sex Duration Max", 20, new ConfigDescription("Set the maximum amount of time a NPC will have sex in seconds! | Note that this is per position and the NPCs will do 3 during H, so the total time is 3 times this value", new AcceptableValueRange<int>(20, 600), new ConfigurationManagerAttributes { Order = 2 }));
            //_NPCSexSpeed = Config.Bind("Simulation", "Set NPC Sex Speed", 1f, new ConfigDescription("Set the base speed for the NPCs sex animation", new AcceptableValueRange<float>(0.1f, 10f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 0 }));

            //sexSensor = Config.Bind("Simulation", "Sex sensor", false, new ConfigDescription("You will know if NPCs are having sex", null, new ConfigurationManagerAttributes { Order = 1 }));

            //Cheater Enhancer
            _enableCheaterEnhancer = Config.Bind("Cheater Enhancer", "Enable Cheater Enhancer", true, new ConfigDescription("The characters will Gain Hate toward their cheater partners and the one they cheated with at the end of the day", null, new ConfigurationManagerAttributes { Order = 3 }));
            _byEndPeriod = Config.Bind("Cheater Enhancer", "Add Points at the End of Period", false, new ConfigDescription("Add the Hate subPoints at the End of the Period instead of the end of the day. The characters will gain a quarter of the Hate Value by the end of the period. Warning! If you end the day prematurely, the points won't get added at full", null, new ConfigurationManagerAttributes { Order = 2 }));
            _disablePCCheatEnhancer = Config.Bind("Cheater Enhancer", "Do not affect PC", false, new ConfigDescription("If enabled, the Playable Character won't get any Hate subPoints from this Mod", null, new ConfigurationManagerAttributes { Order = 1 }));
            _baseHateSubPoints = Config.Bind("Cheater Enhancer", "Set Base Hate Value", 60, new ConfigDescription("Set the amount of Hate subPoints the character will gain (every 30 subPoints is equal to 1 Point) Note: An evil trait character will get double of this values", new AcceptableValueRange<int>(0, 300), new ConfigurationManagerAttributes { Order = 0 }));

            //Game Fixes
            _applyGameFixes = Config.Bind("Game Fixes", "Apply Gameplay Fixes", true, new ConfigDescription("If enabled, it will apply all gameplay fixes that are enable", null, new ConfigurationManagerAttributes { Order = 10 }));
            _bbqAnswerType = Config.Bind("Game Fixes", "BBQ Date Fixes", HighVirtueBBQ.Normal,
            new ConfigDescription("Fixes the issue of characters with High and Highest virtue inviting other to a BBQ when the requirements are not met. Possible fixes are | Accept: NPC will accept BBQ with a low chance | Do Not Ask: NPC with High and Highest virtue will no longer ask for BBQ outside of the expected requierements (High Love)", null, new ConfigurationManagerAttributes { Order = 9 }));
            _applyAreYouDatingFix = Config.Bind("Game Fixes", "NPCs Are You Dating Fix", true, new ConfigDescription("NPCs can use the action Are You dating Anyone? to get into a relationship, this action is bugged since it can bypass Traits, Favorability and Virtue level, making High and Highest Virtue character to date anyone. Enabling this option will fix this problem", null, new ConfigurationManagerAttributes { Order = 8 }));
            _makePCNonInteractable = Config.Bind("Game Fixes", "PC Non Interactable while following", true, new ConfigDescription("PC will become non interactable while following a NPC, it will become interactable when they reach their destination", null, new ConfigurationManagerAttributes { Order = 7 }));
            //_escapeVoiceFix = Config.Bind("Game Fixes", "Escape Voice Fix", true, new ConfigDescription("The escape voice line will play when a NPC starts to run away", null, new ConfigurationManagerAttributes { Order = 6 }));

            //Other Options
            _enableActionLowestRate = Config.Bind("Other Options", "Override Minimum Success Rate", false, new ConfigDescription("If enabled, it will override the minimum success rate", null, new ConfigurationManagerAttributes { Order = 10 }));
            _actionLowestRatePCNPC = Config.Bind("Other Options", "Set minimum success rate for", CharaType.PC | CharaType.NPC,
            new ConfigDescription("Set the minimum success rate for PC, NPC or both", null, new ConfigurationManagerAttributes { Order = 9 }));
            _lowestActionRate = Config.Bind("Other Options", "Set Minimum Success Rate value", 0f, new ConfigDescription("Set the minimum success rate for an action (it will override action with 0 success rate)", new AcceptableValueRange<float>(0f, 100f), new ConfigurationManagerAttributes { ShowRangeAsPercent = true, Order = 8 }));
            hugLovePointsSwitch = Config.Bind("Other Options", "Switch hug love to friend points", false, new ConfigDescription("If enabled, the hug action will give friend points instead of love", null, new ConfigurationManagerAttributes { Order = 7 }));

            //Fortune
            _fortuneSuccessFix = Config.Bind("Fortune Manager", "Buff fortune 7", true, new ConfigDescription("In vanilla, this fortune barely has an impact in the game. Enabling this option will buff this fortune. Now alongside of improving the 3 options result, now it increases the probability of an interaction by 10 percent", null, new ConfigurationManagerAttributes { Order = 10 }));

            //Cheats
            _forceThreesome = Config.Bind("Cheats", "Force 3P Option", false, new ConfigDescription("The 3P option will always be available when interrupting Public H", null, new ConfigurationManagerAttributes { Order = 10 }));
            _forceThreesomeNPC = Config.Bind("Cheats", "Force NPC to join 3P", false, new ConfigDescription("NPC will ask to join 3P any time they see Public H", null, new ConfigurationManagerAttributes { Order = 9 }));
            _forceHAction = Config.Bind("Cheats", "Force Only H Action", false, new ConfigDescription("Force NPC to only ask for Sex (Results may vary) WARNING: this affect all NPCs regardless of virtue, traits, sex, etc", null, new ConfigurationManagerAttributes { Order = 8 }));
            //forceNightEvent = Config.Bind("Cheats", "Force H Night Event", false, new ConfigDescription("If enabled, the lewd night event will be trigger", null, new ConfigurationManagerAttributes { Order = 7 }));

            CGBNightEvent.NightEventJudgeHook.Install();
            patchedHooks = Harmony.CreateAndPatchAll(typeof(Hooks));
        }
        public override bool Unload()
        {
            patchedHooks?.UnpatchSelf();
            return true;
        }
        public enum AllowHugging
        {
            None = 0,
            AllCharacters = 1,
            ExcludeSomeTraits = 2,
            LowestVirtueOnly = 3,
        }

        [System.Flags]
        public enum CharaType
        {
            None = 0,
            PC = 1 << 0,
            NPC = 1 << 1,
        }

        [System.Flags]
        public enum NPCSex
        {
            None = 0,
            Duration = 1 << 0,
            //Speed = 1 << 1,
        }

        public enum HighVirtueBBQ
        {
            Normal = 0,
            Accept = 1,
            DoNotAsk = 2,
        }

        public enum DisplayPercentage
        {
            Normal = 0,
            Real = 1,
            Hide = 2,
        }

        public static bool[] GetGameFixes()
        {
            bool[] fixes = [_applyGameFixes.Value, 
                            _bbqAnswerType.Value == HighVirtueBBQ.Accept, 
                            _applyAreYouDatingFix.Value];
            return fixes;
        }

        public static bool[] GetActionLowestRateEnable()
        {
            bool[] lowesRate = [
                _enableActionLowestRate.Value,
                (_actionLowestRatePCNPC.Value & CharaType.PC) != 0,
                (_actionLowestRatePCNPC.Value & CharaType.NPC) != 0
                ];
            return lowesRate;
        }

        public static bool[] GetForceActions()
        {
            bool[] forceActions = [forceBreakUp.Value];

            return forceActions;
        }

        public static float GetNewRate()
        {
            return _lowestActionRate.Value;
        }

        public static bool GetFortuneFix()
        {
            return _fortuneSuccessFix.Value;
        }

        public static HighVirtueBBQ GetHighVirtueBBQ()
        {
            return _bbqAnswerType.Value;
        }

        public static int[] GetNightChance()
        {
            return [nightChance.Value,nightChanceH.Value];
        }
        /// <summary>
        /// ToDo
        /// 1:DONE! Character Stats Reduction at the End of the day. 
        /// 2:DONE! Disable NPC offcreen interaction Skip.
        /// 3:DONE! Cheating status enhancer. 
        /// 4:DONE! Force 3P Option.
        /// 5:DONE! Increase frequency of Night Visits.
        /// 7:Blackmailed NPC can choose Lover.
        /// 8:Increase scheming Rate toward PC.
        /// 9:Increase NPC 3P Chance.
        /// 10:DONE! Fix "are you dating" for NPC.
        /// 11:Can Take Blackmailed character to Private Rooms.
        /// 12:DONE! Increase PC Movement Speed.
        /// 13:Apologize can remove Cheating status.
        /// 14:DONE! Hugs is Normal.
        /// 15:Disable Follow Me Command on Beach.
        /// 16:DONE! Force NPC H Action.
        /// 17:DONE! Reduce Interruption rate.
        /// 18:DONE! Force NPC to join 3P.
        /// 19:DONE! Hide Probability.
        /// 20:DONE! Fix BBQ Date for High and Highest Virtue.
        /// 21:DONE! Increase NPC Sex Duration.
        /// 22:DONE! Add PC Auto Play.
        /// 23:DONE! Switch PC character during the day.
        /// 24:Unknown! Fix NPC Escape voice not playing.
        /// 25:DONE! Fix PC while Following NPC. 
        /// 26:DONE! Remove Interaction Limit.
        /// 27:Pending! Increse NPC sex Speed.  <- v1.5.7
        /// 28:DONE! Fix Fortune 7 Effect.
        /// 29:DONE! Break Up action always 100%. <- v1.6.3
        /// 30: Hug give friendship instead of love.
        /// 31: Day Counter.
        /// 32: Improved Night Event.
        /// </summary>
        internal static class Hooks
        {
            //static bool blackmail = false;
            private static Il2CppSystem.Collections.Generic.Dictionary<int, GlobalListLoad.MoveAnimationPitch> npcSpeedDic = new();

            [HarmonyPostfix]
            [HarmonyPatch(typeof(TitleScene), nameof(TitleScene.Start))]
            public static void GetNPCsMovementSpeed(TitleScene __instance)
            {
                if (npcSpeedDic.Count == 0)
                {
                    foreach (var npc in GlobalListLoad.Instance.moveAnimSpeedNPCTable)
                    {
                        GlobalListLoad.MoveAnimationPitch move = new();
                        move.runRate = npc.value.runRate;
                        move.speed_run = npc.Value.speed_run;
                        move.speed_walk = npc.Value.speed_walk;
                        npcSpeedDic.Add(npc.Key, move);
                    }
                }
                CustomGameBalance.GetNightRateList();
            }
            [HarmonyPostfix]
            [HarmonyPatch(typeof(SimulationScene), nameof(SimulationScene.Start))]
            public static void SimStart(SimulationScene __instance)
            {
                CGBSwitchCharacter.CreateSwitchPCCharacterButton(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SimulationScene), nameof(SimulationScene.Update))]
            public static void DectectKeyDown(SimulationScene __instance)
            {
                if (Input.GetKeyDown(_toggleKey_AutoPC.Value))
                {
                    CustomGameBalance.SetAutoPC();
                    return;
                }

                if (Input.GetKeyDown(_toggleKey_AutoCharaVoice.Value))
                {
                    if (GameChara.PlayerAI != null)
                    {
                        if (GameChara.PlayerAI.BehaviourCtrl.isAuto) LowpolyActionVoiceManager.Instance.LowpolyVoicePlay(33, GameChara.PlayerAI);
                    }
                    return;
                }

                if (Input.GetKeyDown(_toggleKey_SwitchPCCharacter.Value))
                {
                    CGBSwitchCharacter.SwitchPCCharacter();
                    return;
                }

                if (_makePCNonInteractable.Value)
                {
                    if (GameChara.Player != null)
                    {
                        if (GameChara.Player.charasGameParam.isChase && !GameChara.Player.charasGameParam.isWithAction)
                        {
                            var isNotFree = ADVManager._instance.IsHScene || ADVManager._instance.IsADV
                                        || (MyRoom._instance != null && MyRoom._instance.IsOpen());
                            if (!isNotFree) GameChara.Player.charasGameParam.isWithAction = true;
                        } 
                    }
                }    
            }

            /*[HarmonyPostfix]
            [HarmonyPatch(typeof(MyRoom), nameof(MyRoom.IsOpen))]
            public static void MyRoomAutoPlay(MyRoom __instance, bool __result)
            {
                if (__result)
                {
                    if (!__instance._tglAutoPlay.m_EnableCalled) __instance._tglAutoPlay.m_EnableCalled = true;
                    if (__instance._tglAutoPlay.isPointerInside && __instance._tglAutoPlay.isPointerDown)
                    {
                    }
                }
            }*/

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SV.Talk.PCPassiveTalkTask), nameof(SV.Talk.PCPassiveTalkTask.UIActiveAndBaseEnd))]
            public static void SetPCInteractible()
            {
                if (GameChara.Player.charasGameParam.isWithAction) GameChara.Player.charasGameParam.isWithAction = false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(MemoryParameter), nameof(MemoryParameter.EndOfTimeZoneProcess))]
            public static void EndOfTimeZoneGameBalance(MemoryParameter __instance)
            {
                if (_enableCheaterEnhancer.Value)
                {
                    if (_byEndPeriod.Value)
                    {
                        var _tempKeyActor = HelperFunctions.FindMainActorInstanceMemory(__instance);
                        if (_tempKeyActor.Value != null)
                        {
                            CustomGameBalance.CheaterEnhancer(_tempKeyActor.Value, _baseHateSubPoints.Value, _disablePCCheatEnhancer.Value, _byEndPeriod.Value);
                        }
                        else Log.LogInfo($"Error: Actor is Null");
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(MemoryParameter), nameof(MemoryParameter.EndOfDay))]
            public static void EndOfDayGameBalance(MemoryParameter __instance, Actor _self)
            {
                if (_enableCheaterEnhancer.Value)
                {
                    if (!_byEndPeriod.Value) CustomGameBalance.CheaterEnhancer(_self, _baseHateSubPoints.Value, _disablePCCheatEnhancer.Value, _byEndPeriod.Value);
                }

                /*Log.LogInfo($"-----------------------------------------");
                Log.LogInfo($"Name: " + _self.Name);
                Log.LogInfo($"LvPhysical: " + _self.gameParameter.LvPhysical);
                Log.LogInfo($"LvTalk: " + _self.gameParameter.LvTalk);
                Log.LogInfo($"LvStudy?: " + _self.gameParameter.LvStudy);
                Log.LogInfo($"LvLiving: " + _self.gameParameter.LvLiving);
                Log.LogInfo($"-----------------------------------------");
                Log.LogInfo($"Stamina: " + _self.charasGameParam._baseParameter_k__BackingField.Stamina);
                Log.LogInfo($"Conversation: " + _self.charasGameParam._baseParameter_k__BackingField.Conversation);
                Log.LogInfo($"Study: " + _self.charasGameParam._baseParameter_k__BackingField.Study);
                Log.LogInfo($"Living: " + _self.charasGameParam._baseParameter_k__BackingField.Living);
                Log.LogInfo($"JobPoint: " + _self.charasGameParam._baseParameter_k__BackingField.JobPoint);*/

                if (_enableStatsReduction.Value) CustomGameBalance.EndOfDayStatsReduction(_self, _basePoint.Value, _pointAditive.Value, _disablePCReduction.Value);

                /*Log.LogInfo($"-----------------------------------------");
                Log.LogInfo($"Stamina: " + _self.charasGameParam._baseParameter_k__BackingField.Stamina);
                Log.LogInfo($"Conversation: " + _self.charasGameParam._baseParameter_k__BackingField.Conversation);
                Log.LogInfo($"Study: " + _self.charasGameParam._baseParameter_k__BackingField.Study);
                Log.LogInfo($"Living: " + _self.charasGameParam._baseParameter_k__BackingField.Living);
                Log.LogInfo($"JobPoint: " + _self.charasGameParam._baseParameter_k__BackingField.JobPoint);*/

            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(BehaviourController), nameof(BehaviourController.RunAndWalk))]
            public static void SetNewBaseSpeed(BehaviourController __instance, bool _isPC, int _actionNo)
            {
                if (_isPC)
                {
                    var _walkSpeed = GlobalListLoad.Instance.moveAnimSpeedPCTable[0].speed_walk;
                    var _runSpeed = GlobalListLoad.Instance.moveAnimSpeedPCTable[0].speed_run;

                    if (_walkSpeed != _pcWalkSpeed.Value) GlobalListLoad.Instance.moveAnimSpeedPCTable[0].speed_walk = (float)Math.Round(_pcWalkSpeed.Value,2);
                    if (_runSpeed != _pcRunSpeed.Value) GlobalListLoad.Instance.moveAnimSpeedPCTable[0].speed_run = (float)Math.Round(_pcRunSpeed.Value,2);
                }
                else 
                {
                    foreach (var npcspeed in npcSpeedDic)
                    {
                        GlobalListLoad.Instance.moveAnimSpeedNPCTable[npcspeed.Key].speed_walk = (float)Math.Round(npcspeed.Value.speed_walk * _npcWalkSpeed.Value,2);
                        GlobalListLoad.Instance.moveAnimSpeedNPCTable[npcspeed.Key].speed_run = (float)Math.Round(npcspeed.Value.speed_run * _npcRunSpeed.Value,2);
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(WitnessChara), nameof(WitnessChara.IsEntrySkinship))]
            public static bool RemoveHugCheckFromSkinship(bool __result, AI _self, AI _target)
            {
                switch (_allowHugSkinship.Value)
                {
                    case AllowHugging.None:
                        return __result;

                    case AllowHugging.AllCharacters:
                        if (_target.charaData.CommandNo == 32 || _target.charaData.CommandNo == 54) return false;
                        break;

                    case AllowHugging.ExcludeSomeTraits:
                        if (_target.charaData.CommandNo == 32 || _target.charaData.CommandNo == 54)
                        {
                            if (_self.charaData.gameParameter.individuality.answer.Contains(10) || _self.charaData.gameParameter.individuality.answer.Contains(36)) return __result;
                            return false;
                        }
                        break;

                    case AllowHugging.LowestVirtueOnly:
                        if (_target.charaData.CommandNo == 32 || _target.charaData.CommandNo == 54)
                        {
                            if (_self.charaData.gameParameter.lvChastity == 0) return false;
                        }
                        break;
                }
                return __result;
            }

            /*[HarmonyPostfix]
            [HarmonyPatch(typeof(WitnessChara), nameof(WitnessChara.IsEntryChating))]
            public static bool IncludeHugForChating(bool __result, AI _self, AI _target)
            {
                switch (_allowHugSkinship.Value)
                {
                    case AllowHugging.None:
                        return __result;

                    case AllowHugging.AllCharacters:
                        if ((_target.charaData.CommandNo == 32 || _target.charaData.CommandNo == 54) && _target.chaCtrl.) return true;
                        break;

                    case AllowHugging.ExcludeSomeTraits:
                        if (_target.charaData.CommandNo == 32 || _target.charaData.CommandNo == 54)
                        {
                            if (_self.charaData.gameParameter.individuality.answer.Contains(10) || _self.charaData.gameParameter.individuality.answer.Contains(36)) return __result;
                            return true;
                        }
                        break;

                    case AllowHugging.LowestVirtueOnly:
                        if (_target.charaData.CommandNo == 32 || _target.charaData.CommandNo == 54)
                        {
                            if (_self.charaData.gameParameter.lvChastity == 0) return true;
                        }
                        break;
                }
                return __result;
            }*/

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ADVManager), nameof(ADVManager.Set3PFlag))]
            public static void Force3P(ADVManager __instance, Actor actor, Actor actor1, Actor actor2)
            {
                if (_forceThreesome.Value)
                {
                    if (actor.chaCtrl.sex == 0 && actor1.chaCtrl.sex == 0 && actor2.chaCtrl.sex == 0) return;
                    if (actor.chaCtrl.sex == 1 && actor1.chaCtrl.sex == 1 && actor2.chaCtrl.sex == 1)
                    {
                        if (actor.chaCtrl.fileParam.isFutanari || actor1.chaCtrl.fileParam.isFutanari || actor2.chaCtrl.fileParam.isFutanari) __instance._packData.Is3P = true;
                        return;
                    }
                    __instance._packData.Is3P = true;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SVThinking), nameof(SVThinking.OnUpdate))]
            public static void ActionOverride(SVThinking __instance)
            {
                if (_forceHAction.Value)
                {
                    if (!__instance._charaCtrl.AI.charaData.IsPC)
                    {
                        var _command = __instance._charaCtrl.AI.charaData.CommandNo;
                        switch (_command)
                        {
                            case 47:
                                return;                       
                            case 48:
                                return;
                            case 50:
                                return;
                            case 51:
                                return;
                        }
                        __instance._charaCtrl.AI.charaData.CommandNo = 35;
                    } 
                    return;
                } 
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ReactionManager), nameof(ReactionManager.Confirmation))]
            public static int ReactionOverride(int __result, AI _ai, AI _ai1, AI _ai2, int no)
            {
                if (__result == -1) return __result;

                if (_forceThreesomeNPC.Value)
                {
                    if (no == 1)
                    {
                        if (_ai != null && _ai1 != null && _ai2 != null)
                        {
                            if (_ai.chaCtrl.sex == 0 && _ai1.chaCtrl.sex == 0 && _ai2.chaCtrl.sex == 0) return __result;
                            if (_ai.chaCtrl.sex == 1 && _ai1.chaCtrl.sex == 1 && _ai2.chaCtrl.sex == 1)
                            {
                                if (_ai.chaCtrl.fileParam.isFutanari || _ai1.chaCtrl.fileParam.isFutanari || _ai2.chaCtrl.fileParam.isFutanari) return __result = 4;
                                return __result;
                            }
                        }
                        return __result = 4;
                    }
                }

                if (_enableReactionManager.Value)
                {
                    bool[] _affectChara = [false,false];
                    if ((_interruptPCNPC.Value & CharaType.PC) != 0) _affectChara[0] = true;
                    if ((_interruptPCNPC.Value & CharaType.NPC) != 0) _affectChara[1] = true;

                    if (_affectChara[0] || _affectChara[1])
                    {
                        if (_reactionChanceOverall.Value > 0 || _reactionChanceNormal.Value > 0 || _reactionChanceSkinship.Value > 0 || _reactionChanceH.Value > 0)
                        {
                            int[] reacProbs =
                            [
                                _reactionChanceOverall.Value,
                                _reactionChanceNormal.Value,
                                _reactionChanceSkinship.Value,
                                _reactionChanceH.Value,
                            ];

                            __result = CustomGameBalance.ReactionChance(_ai, _ai1, _ai2, no, __result, reacProbs, _affectChara);
                        }
                    }
                }

                //Doesn't work, Audio is broken somewhere else.
                /*if (_applyGameFixes.Value && _escapeVoiceFix.Value) 
                {
                    __result = 3;
                    switch (no)
                    {
                        case 2:
                            if (__result != 0 && __result != -1)
                            {
                                LowpolyActionVoiceManager.Instance.LowpolyVoicePlay(33, _ai);
                                Log.LogInfo($"Escape 2! {__result}");
                            }
                            break;
                        case 5:
                            if (__result != 0 && __result != 1 && __result != 2 && __result != 3)
                            {
                                LowpolyActionVoiceManager.Instance.LowpolyVoicePlay(33, _ai);
                                Log.LogInfo($"Escape 5! {__result}");
                            }
                            break;
                        case 6:
                            if (__result != 0 && __result != 1 && __result != 2)
                            {
                                _ai.BehaviourCtrl.isAuto = true;
                                LowpolyActionVoiceManager.Instance.LowpolyVoicePlay(33, _ai);
                                _ai.BehaviourCtrl.isAuto = false;
                                Log.LogInfo($"Escape 6! {__result}");
                            }
                            break;
                        case 8:
                            if (__result == 2)
                            {
                                LowpolyActionVoiceManager.Instance.LowpolyVoicePlay(33, _ai);
                                Log.LogInfo($"Escape 8! {__result}");
                            }
                            break;
                    }
                }*/

                return __result;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ThinkingManager), nameof(ThinkingManager.InterpersonalCommandSelectionTarget))]
            public static int SetTarget(int __result, Actor _actor, int _commandID)
            {
                if (__result != -1) __result = CustomGameBalance.NewCommandTarget(_actor, _commandID, __result);
                return __result;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CommandUI), nameof(CommandUI.ToStringProb))]
            public static string OverwriteProbability(string __result, int rate)
            {
                if(_actionPorcentageDisplay.Value == DisplayPercentage.Hide) return __result = "";
                if (_actionPorcentageDisplay.Value == DisplayPercentage.Real) return __result = rate.ToString() + "%";
                return __result;
            }

            static bool[] fixes = [false, false];
            [HarmonyPriority(300)]
            [HarmonyPostfix]
            [HarmonyPatch(typeof(BaseAnswer), nameof(BaseAnswer.Judge))]
            public static void ActionAnswerRate(BaseAnswer __instance, bool __result, YesNoJudgeManager.AnswerInfo _ansInfo, YesNoJudgeManager.YesNoInfo _ynInfo, int _commandID, int _questionCount, Il2CppStructArray<bool> _calcs)
            {
                CustomGameBalance.NewAnswerRate(_ansInfo, _ynInfo, _commandID, _questionCount);
            }

            private static AI _npc = null;
            private static AI _npc2 = null;
            private static AI _npc3 = null;
            private static int tempMapID_1 = 0;
            private static int tempMapID_2 = 0;
            private static int tempMapID_3 = 0;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ADV.Commands.Game.LowChara.LowPolyText), nameof(ADV.Commands.Game.LowChara.LowPolyText.Do))]
            public static void RemoveNPCSkipPre(LowPolyText __instance)
            {
                if (_doNotSkipNPC.Value)
                {
                    __instance.LowPolyChara.TryGet(0, out Actor _actor);
                    __instance.LowPolyChara.TryGet(1, out Actor _actor2);
                    __instance.LowPolyChara.TryGet(2, out Actor _actor3);

                    if (_actor != null)
                    {
                        _npc = SV.GameChara.FindCharaAI(_actor._chaCtrl_k__BackingField);
                        if (_npc != null)
                        {
                            if (MapManager.Instance._mapID == _npc.BehaviourCtrl.nowMapID)
                            {
                                tempMapID_1 = -1;
                                tempMapID_2 = -1;
                                tempMapID_3 = -1;
                                return;
                            }
                            //Log.LogInfo($"Voice Skip Pre Name: {_actor.Name}");
                            //Log.LogInfo($"Voice Skip Pre: temp:{tempMapID_1} npcMap:{_npc.BehaviourCtrl.nowMapID} Current Map:{MapManager.Instance._mapID}");

                            tempMapID_1 = _npc.BehaviourCtrl.nowMapID;
                            _npc.BehaviourCtrl.nowMapID = MapManager.Instance._mapID;
                        }
                    }
                    else
                    {
                        _npc = null;
                        tempMapID_1 = -1;
                        //Log.LogInfo($"Null Actor1");
                    }

                    if (_actor2 != null)
                    {
                        _npc2 = SV.GameChara.FindCharaAI(_actor2._chaCtrl_k__BackingField);
                        if (_npc2 != null)
                        {                            
                            //Log.LogInfo($"Voice Skip Pre Name: {_actor2.Name}");
                            //Log.LogInfo($"Voice Skip Pre: temp2:{tempMapID_2} npcMap:{_npc2.BehaviourCtrl.nowMapID} Current Map:{MapManager.Instance._mapID}");
                            tempMapID_2 = _npc2.BehaviourCtrl.nowMapID;
                            _npc2.BehaviourCtrl.nowMapID = MapManager.Instance._mapID;
                        }
                    }
                    else
                    {
                        _npc2 = null;
                        tempMapID_2 = -1;
                        //Log.LogInfo($"Null Actor2");
                    }

                    if (_actor3 != null)
                    {
                        _npc3 = SV.GameChara.FindCharaAI(_actor3._chaCtrl_k__BackingField);
                        if (_npc3 != null)
                        {
                            //Log.LogInfo($"Voice Skip Pre: temp3:{tempMapID_3} npcMap:{_npc3.BehaviourCtrl.nowMapID} Current Map:{MapManager.Instance._mapID}");
                            tempMapID_3 = _npc3.BehaviourCtrl.nowMapID;
                            _npc3.BehaviourCtrl.nowMapID = MapManager.Instance._mapID;
                        }
                    }
                    else
                    {
                        _npc3 = null;
                        tempMapID_3 = -1;
                        //Log.LogInfo($"Null Actor3");
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ADV.FukidashiUICanvas), nameof(ADV.FukidashiUICanvas.Emit))]
            public static bool DisplayFukidashi()
            {
                if (_doNotSkipNPC.Value) 
                {
                    if (tempMapID_1 > -1) return false;
                }
                return true;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ADV.Commands.Game.LowChara.LowPolyText), nameof(ADV.Commands.Game.LowChara.LowPolyText.Do))]
            public static void RemoveNPCSkipPost(LowPolyText __instance)
            {
                if (_doNotSkipNPC.Value)
                {
                    if (tempMapID_1 > -1 && _npc != null)
                    {
                        _npc.BehaviourCtrl.nowMapID = tempMapID_1;
                    }
                    if (tempMapID_2 > -1 && _npc2 != null) 
                    { 
                        _npc2.BehaviourCtrl.nowMapID = tempMapID_2;
                    }
                    if (tempMapID_3 > -1 && _npc3 != null) 
                    { 
                        _npc3.BehaviourCtrl.nowMapID = tempMapID_3;
                    }
                    //Log.LogInfo($"Voice Skip Post: TempMapID:{tempMapID_1} - {tempMapID_2} - {tempMapID_3}");
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(LowPolyHMotionPlay), nameof(LowPolyHMotionPlay.Do))]
            public static void NPCSexDuration(LowPolyHMotionPlay __instance)
            {
                if ((_NPCLowPolySex.Value & NPCSex.Duration) != 0)
                {
                    if (__instance.Args.Count > 2)
                    {
                        __instance.Args[1] = _NPCSexTimerMin.Value.ToString();
                        if (_NPCSexTimerMin.Value >= _NPCSexTimerMax.Value) __instance.Args[2] = (_NPCSexTimerMin.Value + 1).ToString();
                        else __instance.Args[2] = _NPCSexTimerMax.Value.ToString();
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(YesNoJudgeManager), nameof(YesNoJudgeManager.CommonCondition19__22))]
            public static bool RemoveActionLimit()//int _commandID
            {
                if (_removeActionLimit.Value) return false;
                return true;
            }

            [HarmonyPriority(300)]
            [HarmonyPostfix]
            [HarmonyPatch(typeof(SuccessJudgeManager), nameof(SuccessJudgeManager.Judge))]
            public static int OverrideSuccess(int __result, int _commandNo)
            {
                __result = CustomGameBalance.NewSuccessValue(__result);
                return __result;
            }

           /*[HarmonyPrefix]
            [HarmonyPatch(typeof(SimulationButtonAction), nameof(SimulationButtonAction.ShiftFromSimNightToRoomMorning))]
            public static void NightCharactersPool(SimulationButtonAction __instance)
            {
                if (increaseNightEvent.Value) CustomGameBalance.SetNightCharacters();
            }

            [HarmonyPrefix]
            [HarmonyWrapSafe]
            [HarmonyPatch(typeof(SimulationButtonAction), nameof(SimulationButtonAction.ShiftFromSimNightToRoomMorningADV))]
            static bool ShiftFromSimNightToRoomMorningPrefix(SimulationButtonAction __instance, ref UniTask __result)
            {
                if (forceNightEvent.Value)
                {
                    var charaAI = CustomGameFunctions.GetSelectedChara();
                    if (charaAI != null) __result = __instance.SneakingVisitADV(charaAI);
                }
                return true;
            }
            
            [HarmonyPrefix]
            [HarmonyWrapSafe]
            [HarmonyPatch(typeof(NightEventManager), nameof(NightEventManager.ConditionsJudge))]
            public static void SetNightEventRate(NightEventManager __instance, Actor _src, Actor _target)
            {
                //Log.LogInfo($"Night Event Judge:{_src.charasGameParam.Index} - {_target.charasGameParam.Index}");
                //if (increaseNightEvent.Value) CustomGameBalance.NightEventChance(__instance, _src);
            }*/
        }
    }
}
