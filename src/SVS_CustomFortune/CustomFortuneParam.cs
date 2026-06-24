using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SVS_CustomFortune
{
    internal class CustomFortuneParam
    {
        public class FortuneParam
        {
            public bool Enable { get; set; } = false;
            public int ID { get; set; } = -1;
            public string Name { get; set; } = "none";
            public string FortuneType { get; set; } = "Normal";
            public int FortuneRate { get; set; } = 1;
            public List<float> FavorPoints { get; set; } = new();
            public List<int> StatesPoints { get; set; } = new();
            public int AddSuccessPoint { get; set; } = 0;
            public ActionParam ActionsCommands { get; set; } = new();
            public OutfitParam Outfits { get; set; }
            public string ShortMessage { get; set; } = "As Usual";
            public int SpriteAnimFPS { get; set; } = 1;
            public string[] SpritesPath { get; set; } = [];
            public List<ScenarioParam> ScenarioParams { get; set; } = new();
        }

        public class ScenarioParam
        {
            public int Version { get; set; } = 0; //Init value
            public bool Multi { get; set; } = false; //Init value
            public string Command { get; set; }
            public string[] Args { get; set; }
        }

        public class ActionParam
        {
            public List<int> CharaActionsAnswers { get; set; } = new();
            public Dictionary<int, int> ReduceActionCommandRate { get; set; } = new();
        }

        public class OutfitParam
        {
            public bool UseOutfit { get; set; } = false;
            public int OutfitID { get; set; } = -1;
            public bool AtDayStart { get; set; } = false;
            public List<int> UseDuringPeriod { get; set; } = new();
            public bool ForceOutfit { get; set; } = false;
        }

        public static List<FortuneParam> DeserializeFortunes(string fortuneFile)
        {
            var sr = new StreamReader(File.OpenRead(fortuneFile));
            var json = sr.ReadToEnd();
            try
            {
                return JsonSerializer.Deserialize<List<FortuneParam>>(json);
            }
            catch (ArgumentNullException)
            {
                CustomFortunePlugin.Log.LogInfo($"Failed to load fortune.json file from: {fortuneFile}");
                return null;
            }
            catch (Exception)
            {
                CustomFortunePlugin.Log.LogInfo($"Failed to load one or more Custom fortunes from: {fortuneFile}");
                return JsonSerializer.Deserialize<List<FortuneParam>>(json);
            }
        }
    }
}
