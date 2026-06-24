using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ADV;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Manager;
using SV;
using SV.Chara;
using SV.Title;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace SVS_NPCSubtitles
{
    [BepInPlugin(GUID, DisplayName, Version)]
    public class NPCSubtitles : BasePlugin
    {
        public const string DisplayName = Constants.Name;
        public const string GUID = "DS27.SVS.NPCSubtitles";
        public const string Version = Constants.Version;

        internal static new ManualLogSource Log;
        private static Harmony patchedHooks;
        private static ConfigEntry<Language> subLanguaje;
        private static ConfigEntry<bool> enableNPCSubtitles;
        private static ConfigEntry<int> fontSize;
        private static ConfigEntry<Color> textColor { get; set; }

        public override void Load()
        {
            // Plugin startup logic
            Log = base.Log;

            var colorConverter = new TypeConverter
            {
                ConvertToString = (obj, _) => ColorUtility.ToHtmlStringRGBA((Color)obj),
                ConvertToObject = (str, _) =>
                {
                    if (!ColorUtility.TryParseHtmlString("#" + str.Trim('#', ' '), out var c))
                        throw new FormatException("Invalid color string, expected hex #RRGGBBAA");
                    return c;
                }
            };
            TomlTypeConverter.AddConverter(typeof(Color), colorConverter);

            enableNPCSubtitles = Config.Bind("NPC Subtitles", "Enable NPC Subtitles", true, new ConfigDescription("Will display text instead of the sprite animation when NPCs talk to each other", null, new ConfigurationManagerAttributes { Order = 5 }));
            fontSize = Config.Bind("NPC Subtitles", "Font Size", 20, new ConfigDescription("Change font size.", new AcceptableValueRange<int>(17, 72), new ConfigurationManagerAttributes { Order = 2 }));
            subLanguaje = Config.Bind("NPC Subtitles", "Subtitle language", Language.English,
            new ConfigDescription("Set the subtitle language (only for loaded chara dialogues, if dialogue is missing and will be shown in english or as an action description)", null, new ConfigurationManagerAttributes { Order = 0 }));

            textColor = Config.Bind("NPC Subtitles", "Text Color", new Color(0.99215686f, 0.99215686f, 0.99215686f, 1f), new ConfigDescription("Color of text.", null, new ConfigurationManagerAttributes { Order = 1 }));

            patchedHooks = Harmony.CreateAndPatchAll(typeof(Hooks));
        }
        public override bool Unload()
        {
            patchedHooks?.UnpatchSelf();
            return true;
        }     

        private static Dictionary<string, string[]> voiceTextDic = new();
        //private static Dictionary<string, string[]> voiceMaleTextDic = new Dictionary<string, string[]>();
        private static bool gotCharaText;
        private enum Language
        {
            Japanese = 0,
            English = 1,
            SimplifiedChinese = 2,
            TraditionalChinese = 3,
        }

        private static void DeserializeVoiceText()
        {
            string NPCTextFile = Path.Combine(Paths.GameRootPath, "UserData\\sub\\SVS_Subtitles_Female_NPC.json");

            if (File.Exists(NPCTextFile))
            {
                var sr = new StreamReader(File.OpenRead(NPCTextFile));
                var json = sr.ReadToEnd();
                try
                {
                    voiceTextDic = JsonSerializer.Deserialize<Dictionary<string, string[]>>(json);
                    gotCharaText = true;
                    sr.Close();
                }
                catch (Exception ex)
                {
                    Log.LogInfo($"Failed to read SVS_Subtitles_Female_NPC.json file {ex}");
                    gotCharaText = false;
                }
            }
            else Log.LogInfo("SVS_Subtitles_Female_NPC.json not found!");
        }

        private static string ReplaceTagWithValue(string _text)
        {
            if (_text == "0")
            {
                var commandID = charaAI.charaData.CommandNo;
                _text = NPCSubtitlesCommands.GetCharaIntention(commandID, out _);
                return _text;
            }

            string thatCharaName = charaAI.charaData.charasGameParam.thatPersonName;
            string npcItemName = "Item";
            string pcItemName = "Item";
            string npcImportantItemName = "Important Item";
            string pcImportantItemName = "Important Item";
            string mapName = "this place";

            if (Game.BelongingsInfoTable != null)
            {
                int itemID = charaAI.charaData.gameParameter.belongings[0];
                if (Game.BelongingsInfoTable.ContainsKey(itemID))
                {
                    npcItemName = Game.BelongingsInfoTable[itemID];
                }
                itemID = charaAI.charaData.gameParameter.belongings[1];
                if (Game.BelongingsInfoTable.ContainsKey(itemID))
                {
                    npcImportantItemName = Game.BelongingsInfoTable[itemID];
                }

                if (charaAI.BehaviourCtrl.target.kind == BehaviourController.TargetInfo.TargetKind.Chara)
                {
                    if (Game.Charas.ContainsKey(charaAI.BehaviourCtrl.target.id))
                    {
                        itemID = Game.Charas[charaAI.BehaviourCtrl.target.id].gameParameter.belongings[0];
                        if (Game.BelongingsInfoTable.ContainsKey(itemID))
                        {
                            pcItemName = Game.BelongingsInfoTable[itemID];
                        }
                        itemID = Game.Charas[charaAI.BehaviourCtrl.target.id].gameParameter.belongings[1];
                        if (Game.BelongingsInfoTable.ContainsKey(itemID))
                        {
                            pcImportantItemName = Game.BelongingsInfoTable[itemID];
                        }
                    }
                }                
            }
            
            int mapID = charaAI.charaData.charasGameParam.thinkingPropertyTemp.beTakenMapNo;
            if (MapManager.Instance != null && mapID >= 0)
            {
                if (MapManager.Instance.MapListTable.ContainsKey(mapID))
                {
                    mapName = MapManager.Instance.MapListTable[mapID].Name;
                }
            }
         
            _text = _text.Replace("[NPC_thatPersonName]", thatCharaName);
            _text = _text.Replace("[PC_thatPersonName]", thatCharaName);
            _text = _text.Replace("[NPC_onesProperty0_item]", npcItemName);
            _text = _text.Replace("[PC_onesProperty0_item]", pcItemName);
            _text = _text.Replace("[NPC_onesProperty1_item]", npcImportantItemName);
            _text = _text.Replace("[PC_onesProperty1_item]", pcImportantItemName);
            _text = _text.Replace("[NPC]", "This person");
            _text = _text.Replace("[NPC2]", "That person");
            _text = _text.Replace("[NPC_comehereLaterMap]", mapName);
            return _text;
        }

        private static GameObject npcSubObj;
        private static AI charaAI;
        private static string subText = "";
        private static bool isDisplay;
        private static bool isReset;
        internal static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(TitleScene), nameof(TitleScene.Start))]
            public static void GetSubtitles()
            {
                DeserializeVoiceText();
            }         

            [HarmonyPostfix]
            [HarmonyPatch(typeof(LowCharaADV), nameof(LowCharaADV.VoicePlay))]
            public static void NPCSubAboveHeadText(LowCharaADV __instance)
            {
                if (isDisplay)
                {
                    if (enableNPCSubtitles.Value && gotCharaText)
                    {
                        if (__instance._voiceList != null)
                        {
                            foreach (var voiceList in __instance._voiceList)
                            {
                                foreach (var voice in voiceList)
                                {
                                    if (subText == "") subText = voice.Audio.name;
                                }
                            }
                        }

                        if (npcSubObj != null && subText != "")
                        {
                            var tmp = npcSubObj.GetComponent<TextMeshProUGUI>();
                            tmp.color = textColor.Value;
                            if (voiceTextDic.ContainsKey(subText))
                            {
                                if (fontSize.Value > 10) tmp.fontSize = fontSize.Value;
                                switch (subLanguaje.Value)
                                {
                                    case Language.Japanese:
                                        tmp.text = voiceTextDic[subText][1];
                                        break;
                                    case Language.English:
                                        tmp.text = voiceTextDic[subText][2];
                                        break;
                                    case Language.SimplifiedChinese:
                                        tmp.text = voiceTextDic[subText][3];
                                        break;
                                    case Language.TraditionalChinese:
                                        tmp.text = voiceTextDic[subText][4];
                                        break;
                                }
                                tmp.text = ReplaceTagWithValue(tmp.text);
                            }
                            else
                            {
                                Log.LogDebug($"{subText} Not in dictionary, trying to get intention");
                                int charaCommandID = charaAI.charaData.CommandNo;
                                tmp.text = NPCSubtitlesCommands.GetCharaIntention(charaCommandID, out bool success);
                                tmp.text = ReplaceTagWithValue(tmp.text);
                                if (!success)
                                {
                                    var tempImage = tmp.transform.GetComponentInParent<Image>();
                                    if (tempImage != null)
                                    {
                                        tempImage.enabled = true;
                                        Log.LogDebug($"Intention not found or it has not text. Restoring Fukidashi sprite");
                                    }
                                    tmp.gameObject.SetActive(false);
                                }
                            }
                        }                   
                    }
                    isDisplay = false;
                }                         
            }

            [HarmonyPriority(300)]
            [HarmonyPostfix]
            [HarmonyPatch(typeof(FukidashiUICanvas), nameof(FukidashiUICanvas.Emit))]
            public static Fukidashi DisplaySubtitle(Fukidashi __result, Transform target)
            {              
                if (__result != null)
                {
                    if (!enableNPCSubtitles.Value && isReset)
                    {
                        Log.LogDebug($"Restoring Fukidashi sprite");
                        var initImage = __result.gameObject.GetComponent<Image>();
                        if (initImage != null) initImage.enabled = true;
                        var initText = __result.transform.GetComponentInChildren<TextMeshProUGUI>(true);
                        if (initText != null)
                        {
                            initText.text = "";
                            initText.gameObject.SetActive(false);
                        } 
                        isReset = false;
                    }
                    
                    if (enableNPCSubtitles.Value && gotCharaText)
                    {          
                        isReset = true;
                        if (target != null)
                        {
                            var tempParentAI = target.GetComponentInParent<AI>();
                            if (tempParentAI != null)
                            {
                                charaAI = tempParentAI;                               
                            }
                        }

                        var tempImage = __result.gameObject.GetComponent<Image>();
                        if (tempImage != null) tempImage.enabled = false;
                        var tempText = __result.transform.GetComponentInChildren<TextMeshProUGUI>(true);
                        if (tempText != null)
                        {
                            tempText.color = textColor.Value;
                            tempText.gameObject.SetActive(true);
                            npcSubObj = tempText.gameObject;
                            if (subText != "") subText = "";
                            tempText.text = NPCSubtitlesCommands.GetCharaIntention(charaAI.charaData.CommandNo, out bool success);
                            tempText.text = ReplaceTagWithValue(tempText.text);
                            isDisplay = true;
                        }
                    }         
                }             
                return __result;
            }        
        }
    }
}
