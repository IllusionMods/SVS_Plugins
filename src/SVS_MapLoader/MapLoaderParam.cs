using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
namespace MapLoader
{
    internal class MapLoaderParam
    {
        public class JobsParam
        {
            public bool CustomJob { get; set; }
            public string JobName { get; set; }
            public string[] LocalizationNames { get; set; }
            public int JobID { get; set; } //ID of the Jobs.
            public int JobMapID { get; set; } //Map where the Job is done.
            public string JobSp { get; set; } = ""; //Job Icon
            public string JobADVPath { get; set; } = ""; //ADV Data
            public Dictionary<string, List<int[]>> StartingLocation { get; set; } //Job starting locations
            public Dictionary<string, List<int[]>> ChangeOfClothes { get; set; } //Clothes to use and change to.
            public Dictionary<string, List<int[]>> ClothesChangingMap { get; set; } //Changing Room Map, usually only one is used, but can accept more than 1.
            public Dictionary<string, List<bool>> JobSchedule { get; set; } //ADV Work schedule.
            public Dictionary<string, List<int[]>> ExplorationArea { get; set; } //Explorable Areas.
        }
        public static List<JobsParam> DeserializeJobs(string _jobFile)
        {
            var sr = new StreamReader(File.OpenRead(_jobFile));
            var json = sr.ReadToEnd();
            try
            {
                return JsonSerializer.Deserialize<List<JobsParam>>(json);
            }
            catch (ArgumentNullException)
            {
                MapLoaderPlugin.Log.LogInfo($"Failed to load jobs.json file from: {_jobFile}");
                return null;
            }
            catch (Exception)
            {
                MapLoaderPlugin.Log.LogInfo($"Failed to load one or more Custom Jobs from: {_jobFile}");
                return JsonSerializer.Deserialize<List<JobsParam>>(json);
            }
        }
        public class BGMInfo
        {
            public string Name { get; set; }
            public int MoodType { get; set; }
            public string Bundle { get; set; }
            public string Asset { get; set; }
            public List<int> PlayOnMapID { get; set; }
        }
        public static List<BGMInfo> DeserializeBGM(string _bgmList)
        {
            var sr = new StreamReader(File.OpenRead(_bgmList));
            var json = sr.ReadToEnd();
            try
            {
                return JsonSerializer.Deserialize<List<BGMInfo>>(json);
            }
            catch (Exception)
            {
                MapLoaderPlugin.Log.LogInfo($"Failed to load bgm.json File");
                return null;
            }
        }
        public class CustomMapParam
        {
            public bool Enable { get; set; }
            public string Name { get; set; }
            public string Version { get; set; }
            public string MapLoaderVersion { get; set; }
            public bool ReplaceDefaultMaps { get; set; }
            public List<string> MapListBundle { get; set; }
            public List<string> JobsADVList { get; set; }
        }
        public static CustomMapParam DeserializeMapInfo(string _mapList)
        {
            var sr = new StreamReader(File.OpenRead(_mapList));
            var json = sr.ReadToEnd();
            try
            {
                return JsonSerializer.Deserialize<CustomMapParam>(json);
            }
            catch (Exception ex)
            {
                MapLoaderPlugin.Log.LogInfo($"Failed to load pack.json File: {ex}");
                return null;
            }
        }
        public class OldCustomMapParam
        {
            public bool Enable { get; set; }
            public string Name { get; set; }
            public string Version { get; set; }
            public List<string> MapListBundle { get; set; }
            public List<string> ADVLitListBundle { get; set; }
        }
        public static OldCustomMapParam DeserializeOldMapInfo(string _mapList)
        {
            var sr = new StreamReader(File.OpenRead(_mapList));
            var json = sr.ReadToEnd();
            try
            {
                return JsonSerializer.Deserialize<OldCustomMapParam>(json);
            }
            catch (Exception ex)
            {
                MapLoaderPlugin.Log.LogInfo($"Failed to load pack.json File: {ex}");
                return null;
            }
        }
        public class MapActionInfoParam
        {
            public string Name { get; set; }
            public int ID { get; set; }
            public float BaseRate { get; set; } = 10f;
            public List<float> Rates { get; set; }
            public bool SoloLocation { get; set; } = false;
            public bool ChangingRoom { get; set; } = false;
            public bool EatPlace { get; set; } = false;
            public bool StudyPlace { get; set; } = false;
            public bool MotionPlace { get; set; } = false;
        }
        public static List<MapActionInfoParam> DeserializeMapAction(string _mapList)
        {
            var sr = new StreamReader(File.OpenRead(_mapList));
            var json = sr.ReadToEnd();
            try
            {
                var value = JsonSerializer.Deserialize<List<MapActionInfoParam>>(json);
                sr.Close();
                return value;
            }
            catch (Exception)
            {
                MapLoaderPlugin.Log.LogInfo($"Failed to load actions.json File");
                return null;
            }
        }

        public class JobADV
        {
            public string ADVAsset { get; set; } = "";
            public string JobNameID { get; set; } = "";
            public List<ScenarioParam> ScenarioParams { get; set; }
        }
        public static List<JobADV> DeserializeJobADV(string _jobADV)
        {
            var sr = new StreamReader(File.OpenRead(_jobADV));
            var json = sr.ReadToEnd();
            try
            {
                return JsonSerializer.Deserialize<List<JobADV>>(json);
            }
            catch (Exception ex)
            {
                MapLoaderPlugin.Log.LogInfo($"Failed to load pack.json File: {ex}");
                return null;
            }
        }
        public class ScenarioParam
        {
            public int Version { get; set; } = 0; //Init value
            public bool Multi { get; set; } = false; //Init value
            public string Command { get; set; }
            public string[] Args { get; set; }
        }
        public static List<ScenarioParam> DeserializeScenarioParam(string _scenario)
        {
            var sr = new StreamReader(File.OpenRead(_scenario));
            var json = sr.ReadToEnd();
            try
            {
                return JsonSerializer.Deserialize<List<ScenarioParam>>(json);
            }
            catch (Exception ex)
            {
                MapLoaderPlugin.Log.LogInfo($"Failed to load pack.json File: {ex}");
                return null;
            }
        }
    }
}
