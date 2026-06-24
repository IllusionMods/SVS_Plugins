using System;
using System.Collections.Generic;
using System.IO;

namespace PersonalityLoader
{
    internal static class PersonalityLoaderFunctions
    {
        public static Dictionary<int, int> personalitiesIDsDic = new();

        public static int SetCustomPersonalityAnimation(string path, int _id)
        {
            if (personalitiesIDsDic.TryGetValue(_id, out var personalityAnimation)) return personalityAnimation;
            if (path == null) throw new ArgumentException("personality folder not found");

            var _personalityFolder = new DirectoryInfo(path);
            var _customPersonalities = _personalityFolder.GetDirectories("*", SearchOption.TopDirectoryOnly);

            foreach (var _folder in _customPersonalities)
            {
                var animator = _folder.GetFiles("animator.csv");
                if (animator.Length != 0)
                {
                    using var sr = new StreamReader(File.OpenRead(animator[0].FullName));
                    while (!sr.EndOfStream)
                    {
                        //var _tempIDs = new int[2];
                        var line = sr.ReadLine();
                        if (line != null)
                        {
                            var tempList = line.Split(',');
                            if (int.TryParse(tempList[0], out int validValue) && int.TryParse(tempList[1], out int validValue2))
                            {
                                personalitiesIDsDic.TryAdd(validValue, validValue2);
                            }
                        }
                    }
                }
            }
            if (personalitiesIDsDic.TryGetValue(_id, out var animation)) return animation;
            PersonalityLoaderPlugin.Log.LogInfo($"Failed to load animation for Personality {_id}");
            return 0;
        }
    }
}
