using System.Collections.Generic;
using System.IO;
using BepInEx;
using Manager;
using SV;
using UnityEngine;

namespace MapLoader
{
    internal class MapLoaderCustomBGM
    {
        private static readonly Dictionary<int, Dictionary<string,AssetBundle>> customBGMDic = new();
        private static readonly Dictionary<string, List<int>> bgmMapList = new();
        
        private static string currentBGMPlaying = "";
        private static int nowMood = -1;
        private static int previousMap = -1;
        private static bool resetBGM;
        public static void InitCustomBGM()
        {
            if (customBGMDic.Count == 0) GetGameBGMBundles();
            else return;
            var tempCustomBGM = global::MapLoader.MapLoader.GetCustomBGMList();
            foreach (var customBGM in tempCustomBGM) 
            {
                var bundleFile = Path.Combine(Paths.GameRootPath, "abdata\\" + customBGM.Bundle);
                if (File.Exists(bundleFile))
                {
                    var _bundle = AssetBundle.LoadFromFile(bundleFile);
                    if (customBGMDic.TryGetValue(customBGM.MoodType, out var custmBgm)) custmBgm.Add(customBGM.Asset, _bundle);
                    else customBGMDic.Add(customBGM.MoodType, new Dictionary<string, AssetBundle> { { customBGM.Asset, _bundle } });
                }
                
                bgmMapList.Add(customBGM.Asset, customBGM.PlayOnMapID);
            }
        }
        public static void GetGameBGMBundles()
        {
            var _loadedBundles = AssetBundle.GetAllLoadedAssetBundles_Native();
            foreach (var item in _loadedBundles)
            {
                if (item.Contains("sv_bgm_03"))
                {
                    customBGMDic.Add(5, new Dictionary<string, AssetBundle> { {"sv_bgm_03", item } });
                }
                if (item.Contains("sv_bgm_04"))
                {
                    customBGMDic.Add(7, new Dictionary<string, AssetBundle> { { "sv_bgm_04", item } });
                }
                if (item.Contains("sv_bgm_05")) 
                {
                    customBGMDic.Add(10, new Dictionary<string, AssetBundle> { { "sv_bgm_05", item } });
                }
                if (item.Contains("sv_bgm_06"))
                {
                    customBGMDic.Add(4, new Dictionary<string, AssetBundle> { { "sv_bgm_06", item } });
                }
                if (item.Contains("sv_bgm_07"))
                {
                    customBGMDic.Add(8, new Dictionary<string, AssetBundle> { { "sv_bgm_07", item } });
                }
                if (item.Contains("sv_bgm_08"))
                {
                    customBGMDic.Add(9, new Dictionary<string, AssetBundle> { { "sv_bgm_08", item } });
                }
                if (item.Contains("sv_bgm_09"))
                {
                    customBGMDic.Add(6, new Dictionary<string, AssetBundle> { { "sv_bgm_09", item } });
                }
                if (item.Contains("sv_bgm_10"))
                {
                    customBGMDic.Add(3, new Dictionary<string, AssetBundle> { { "sv_bgm_10", item } });
                }
                if (item.Contains("sv_bgm_11"))
                {
                    customBGMDic.Add(11, new Dictionary<string, AssetBundle> { { "sv_bgm_11", item } });
                }
                if (item.Contains("sv_bgm_14"))
                {
                    customBGMDic.Add(14, new Dictionary<string, AssetBundle> { { "sv_bgm_14", item } });
                    customBGMDic.Add(15, new Dictionary<string, AssetBundle> { { "sv_bgm_14", item } });
                }
            }
        }
        public static void ChangeBGMWhenChangingMap(bool isCustomBGM_On)
        {
            if (!isCustomBGM_On) 
            {
                if (resetBGM)
                {
                    resetBGM = false;
                    AtmosphereAndBGMManager.Instance.ResetNowBGM();
                }
                return;
            }

            resetBGM = true;

            if (previousMap != MapManager.Instance.MapID)
            {
                previousMap = MapManager.Instance.MapID;

                if (bgmMapList.TryGetValue(currentBGMPlaying, out var bgmMap))
                {
                    if (!bgmMap.Contains(previousMap)) AtmosphereAndBGMManager.Instance.ResetNowBGM();
                }
                else
                {
                    if (customBGMDic.TryGetValue(nowMood, out var cusBgm))
                    {
                        foreach (var bgm in cusBgm)
                        {
                            if (bgmMapList.TryGetValue(bgm.Key, out var bgmMap2))
                            {
                                if (bgmMap2.Contains(previousMap)) AtmosphereAndBGMManager.Instance.ResetNowBGM();
                            }
                        }
                    }                 
                }
            }
        }
        private static string GetMapBGM(string defBGM, int moodType)
        {
            if (bgmMapList.Count == 0) return defBGM;
            if (!MapLoaderPlugin.GetIsCustomBGMOn()) return defBGM;
            var mapID = MapManager.Instance.MapID;

            foreach (var bgm in customBGMDic[moodType])
            {
                if (bgmMapList.TryGetValue(bgm.Key, out var bgmMap3))
                {
                    return bgmMap3.Contains(mapID) ? bgm.Key : defBGM;
                }
            }
            return defBGM;
        }
        public static void PlayCustomBGM(Manager.Sound.Loader _loader)
        {
            if (AtmosphereAndBGMManager.Instance == null) return;
            if (_loader == null) return;
            if (customBGMDic.Count == 0)
            {
                InitCustomBGM();
                if (customBGMDic.Count == 0) return;
            }
            
            switch (_loader.asset)
            {
                case "sv_bgm_00"://
                    break;
                case "sv_bgm_01"://
                    break;
                case "sv_bgm_02"://
                    break;
                case "sv_bgm_03"://5: AtmosphereCheerful
                    nowMood = 5;
                    if (customBGMDic.ContainsKey(5))
                    {
                        var assetName = GetMapBGM(_loader.asset, 5);
                        if (customBGMDic[5].ContainsKey(assetName))
                        {
                            var _c = customBGMDic[5][assetName].LoadAsset<AudioClip>(assetName);
                            if (AtmosphereAndBGMManager.Instance.bgmAudioClips[2].Clip != _c)
                            {
                                AtmosphereAndBGMManager.Instance.bgmAudioClips[2].Clip = _c;
                                currentBGMPlaying = assetName;
                            }
                        }                       
                    }
                    break;
                case "sv_bgm_04"://7: AtmosphereFeelingGood
                    nowMood = 7;
                    if (customBGMDic.ContainsKey(7))
                    {
                        var assetName = GetMapBGM(_loader.asset, 7);
                        if (customBGMDic[7].ContainsKey(assetName))
                        {
                            var _c = customBGMDic[7][assetName].LoadAsset<AudioClip>(assetName);
                            if (AtmosphereAndBGMManager.Instance.bgmAudioClips[4].Clip != _c)
                            {
                                AtmosphereAndBGMManager.Instance.bgmAudioClips[4].Clip = _c;
                                currentBGMPlaying = assetName;
                            }
                        }
                    }
                    break;
                case "sv_bgm_05"://10: AtmosphereRelax
                    nowMood = 10;
                    if (customBGMDic.ContainsKey(10))
                    {
                        var assetName = GetMapBGM(_loader.asset, 10);
                        if (customBGMDic[10].ContainsKey(assetName))
                        {
                            var _c = customBGMDic[10][assetName].LoadAsset<AudioClip>(assetName);
                            if (AtmosphereAndBGMManager.Instance.bgmAudioClips[7].Clip != _c)
                            {
                                AtmosphereAndBGMManager.Instance.bgmAudioClips[7].Clip = _c;
                                currentBGMPlaying = assetName;
                            }
                        }
                    }
                    break;
                case "sv_bgm_06"://4: AtmosphereSerious
                    nowMood = 4;
                    if (customBGMDic.ContainsKey(4))
                    {
                        var assetName = GetMapBGM(_loader.asset, 4);
                        if (customBGMDic[4].ContainsKey(assetName))
                        {
                            var _c = customBGMDic[4][assetName].LoadAsset<AudioClip>(assetName);
                            if (AtmosphereAndBGMManager.Instance.bgmAudioClips[1].Clip != _c)
                            {
                                AtmosphereAndBGMManager.Instance.bgmAudioClips[1].Clip = _c;
                                currentBGMPlaying = assetName;
                            }
                        }
                    }
                    break;
                case "sv_bgm_07"://8: AtmosphereDiscomfort
                    nowMood = 8;
                    if (customBGMDic.ContainsKey(8))
                    {
                        var assetName = GetMapBGM(_loader.asset, 8);
                        if (customBGMDic[8].ContainsKey(assetName))
                        {
                            var _c = customBGMDic[8][assetName].LoadAsset<AudioClip>(assetName);
                            if (AtmosphereAndBGMManager.Instance.bgmAudioClips[5].Clip != _c)
                            {
                                AtmosphereAndBGMManager.Instance.bgmAudioClips[5].Clip = _c;
                                currentBGMPlaying = assetName;
                            }
                        }
                    }
                    break;
                case "sv_bgm_08"://9: AtmosphereAwkward
                    nowMood = 9;
                    if (customBGMDic.ContainsKey(9))
                    {
                        var assetName = GetMapBGM(_loader.asset, 9);
                        if (customBGMDic[9].ContainsKey(assetName))
                        {
                            var _c = customBGMDic[9][assetName].LoadAsset<AudioClip>(assetName);
                            if (AtmosphereAndBGMManager.Instance.bgmAudioClips[6].Clip != _c)
                            {
                                AtmosphereAndBGMManager.Instance.bgmAudioClips[6].Clip = _c;
                                currentBGMPlaying = assetName;
                            }
                        }
                    }
                    break;
                case "sv_bgm_09"://6: AtmosphereGloomy
                    nowMood = 6;
                    if (customBGMDic.ContainsKey(6))
                    {
                        var assetName = GetMapBGM(_loader.asset, 6);
                        if (customBGMDic[6].ContainsKey(assetName))
                        {
                            var _c = customBGMDic[6][assetName].LoadAsset<AudioClip>(assetName);
                            if (AtmosphereAndBGMManager.Instance.bgmAudioClips[3].Clip != _c)
                            {
                                AtmosphereAndBGMManager.Instance.bgmAudioClips[3].Clip = _c;
                                currentBGMPlaying = assetName;
                            }
                        }
                    }
                    break;
                case "sv_bgm_10"://3: AtmosphereH
                    nowMood = 3;
                    if (customBGMDic.ContainsKey(3))
                    {
                        var assetName = GetMapBGM(_loader.asset, 3);
                        if (customBGMDic[3].ContainsKey(assetName))
                        {
                            var _c = customBGMDic[3][assetName].LoadAsset<AudioClip>(assetName);
                            if (AtmosphereAndBGMManager.Instance.bgmAudioClips[0].Clip != _c)
                            {
                                AtmosphereAndBGMManager.Instance.bgmAudioClips[0].Clip = _c;
                                currentBGMPlaying = assetName;
                            }
                        }
                    }
                    break;
                case "sv_bgm_11"://11: AtmosphereNormal
                    nowMood = 11;
                    if (customBGMDic.ContainsKey(11))
                    {
                        var assetName = GetMapBGM(_loader.asset, 11);
                        if (customBGMDic[11].ContainsKey(assetName))
                        {
                            var _c = customBGMDic[11][assetName].LoadAsset<AudioClip>(assetName);
                            if (AtmosphereAndBGMManager.Instance.bgmAudioClips[8].Clip != _c)
                            {
                                AtmosphereAndBGMManager.Instance.bgmAudioClips[8].Clip = _c;
                                currentBGMPlaying = assetName;
                            }
                        }
                    }
                    break;
                case "sv_bgm_12"://
                    break;
                case "sv_bgm_13"://
                    break;
                case "sv_bgm_14"://14: AtmosphereFight and AtmosphereScramble
                    nowMood = 14;
                    if (customBGMDic.ContainsKey(14))
                    {
                        var assetName = GetMapBGM(_loader.asset, 14);
                        if (customBGMDic[14].ContainsKey(assetName))
                        {
                            var _c = customBGMDic[14][assetName].LoadAsset<AudioClip>(assetName);
                            if (AtmosphereAndBGMManager.Instance.bgmAudioClips[9].Clip != _c)
                            {
                                AtmosphereAndBGMManager.Instance.bgmAudioClips[9].Clip = _c;
                                currentBGMPlaying = assetName;
                            }
                        }
                    }
                    if (customBGMDic.ContainsKey(15))
                    {
                        var assetName = GetMapBGM(_loader.asset, 15);
                        if (customBGMDic[15].ContainsKey(assetName))
                        {
                            var _d = customBGMDic[15][assetName].LoadAsset<AudioClip>(assetName);
                            if (AtmosphereAndBGMManager.Instance.bgmAudioClips[10].Clip != _d)
                            {
                                AtmosphereAndBGMManager.Instance.bgmAudioClips[10].Clip = _d;
                                currentBGMPlaying = assetName;
                            }
                        }
                    }
                    break;
            }
        }
    }
}
