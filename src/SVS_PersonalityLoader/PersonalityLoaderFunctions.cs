using System;
using System.IO;
using UnityEngine.Playables;

namespace PersonalityLoader
{
    internal static class PersonalityLoaderFunctions
    {
        public static System.Collections.Generic.Dictionary<int, int> personalitiesIDsDic = new System.Collections.Generic.Dictionary<int, int>();

        public static int SetCustomPersonalityAnimation(string path, int _id)
        {
            if (personalitiesIDsDic.ContainsKey(_id)) return personalitiesIDsDic[_id];
            else
            {
                if (path == null) throw new ArgumentNullException("personality folder not found");

                var _personalityFolder = new DirectoryInfo(path);
                var _customPersonalities = _personalityFolder.GetDirectories("*", SearchOption.TopDirectoryOnly);

                foreach (var _folder in _customPersonalities)
                {
                    var animator = _folder.GetFiles("animator.csv");
                    if (animator.Length != 0 && animator != null)
                    {
                        var sr = new StreamReader(File.OpenRead(animator[0].FullName));
                        while (!sr.EndOfStream)
                        {
                            //var _tempIDs = new int[2];
                            var line = sr.ReadLine();
                            if (line != null)
                            {
                                var tempList = line.Split(',');
                                if (int.TryParse(tempList[0], out int _validValue) && int.TryParse(tempList[1], out int _validValue2))
                                {
                                    if (!personalitiesIDsDic.ContainsKey(int.Parse(tempList[0]))) personalitiesIDsDic.Add(int.Parse(tempList[0]), int.Parse(tempList[1]));
                                }
                            }
                        }
                    }
                }
                if (personalitiesIDsDic.ContainsKey(_id)) return personalitiesIDsDic[_id];
            }
            PersonalityLoaderPlugin.Log.LogInfo($"Failed to load animation for Personality {_id}");
            return 0;
        }
    }
}
