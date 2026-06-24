using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Character;
using CharacterCreation;
using CharacterCreation.UI;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SaveData;
using SV;
using SV.CoordeSelectScene;
using TMPro;
namespace SVS_MoreOutfits
{
    [BepInPlugin(GUID, DisplayName, Version)]
    public class MoreOutfitsPlugin : BasePlugin
    {
        public const string DisplayName = Constants.Name;
        public const string GUID = "SVS_MoreOutfits";
        public const string Version = Constants.Version;

        internal static new ManualLogSource Log;
        //private static Harmony patchedHooks;
        private static ConfigEntry<int> maxOutfits { get; set; }

        private static ConfigEntry<bool> changePCOutfit;

        private static ConfigEntry<bool> weekendOutfit;
        private static ConfigEntry<bool> nightOutfit;
        private static ConfigEntry<bool> nightOutfitOptionOne;

        private static ConfigEntry<bool> lewdOutfit;

        private static ConfigEntry<bool> costumeOutfit;
        private static ConfigEntry<GameDay> costumeDay;
        private static ConfigEntry<GamePeriod> costumePeriod;

        private static ConfigEntry<bool> sportsOutfit;

        //internal static ConfigEntry<KeyCode> toggleKey_Test { get; set; }

        private static readonly List<string> outfitsList =
        [
            "Weekend",
            "Night",
            "Lewd",
            "Costume",
            "Sports",
            "Bath",
            "Camping",
            "Home",
            "Outfit 12",
            "Outfit 13",
            "Outfit 14",
            "Outfit 15",
            "Outfit 16",
            "Outfit 17"
        ];

        public override void Load()
        {
            // Plugin startup logic
            Log = base.Log;

            maxOutfits = Config.Bind("Outfits", "Set Outfit Amount", 8, new ConfigDescription("Set the Max amount of outfits for all characters. Any extra outfit above the default value (7) will not have conditions, in other words, NPCs will not use them", new AcceptableValueRange<int>(3, 17), new ConfigurationManagerAttributes { Order = 19 }));

            changePCOutfit = Config.Bind("Outfits", "Change PC Outfits", false, new ConfigDescription("PC will change into custom outfits automatically", null, new ConfigurationManagerAttributes { Order = 18 }));

            weekendOutfit = Config.Bind("Outfits", "Use Weekend Outfit", true, new ConfigDescription("Characters will use their weekend Outfits durin Saturday and Sunday", null, new ConfigurationManagerAttributes { Order = 17 }));
            nightOutfit = Config.Bind("Outfits", "Use Night Outfit", true, new ConfigDescription("Characters will use their Night Outfits, they will change during the evening", null, new ConfigurationManagerAttributes { Order = 16 }));
            lewdOutfit = Config.Bind("Outfits", "Use Lewd Outfit", true, new ConfigDescription("Characters will use their Lewd Outfits when get a horny fortune ", null, new ConfigurationManagerAttributes { Order = 15 }));
            costumeOutfit = Config.Bind("Outfits", "Use Costume", false, new ConfigDescription("Characters will use a costume on the day and period specify. Note: This does not override the job or swimsuit outfits, those outfits take priority over this one", null, new ConfigurationManagerAttributes { Order = 14 }));
            sportsOutfit = Config.Bind("Outfits", "Use Sports Outfits", false, new ConfigDescription("Characters will use their sport outfits on the day and period specify. Note: This does not override the swimsuit outfit, that outfit take priority over this one", null, new ConfigurationManagerAttributes { Order = 13 }));

            nightOutfitOptionOne = Config.Bind("Outfits Settings", "Night Outfit only at Night", false, new ConfigDescription("Characters will use their Night Outfits only at night", null, new ConfigurationManagerAttributes { Order = 10 }));
            costumeDay = Config.Bind("Outfits Settings", "Set Costume Day", GameDay.None,
            new ConfigDescription("Set the day when the costume will be use", null, new ConfigurationManagerAttributes { Order = 9 }));
            costumePeriod = Config.Bind("Outfits Settings", "Set Costume Period", GamePeriod.Morning | GamePeriod.Midday | GamePeriod.Evening | GamePeriod.Night,
            new ConfigDescription("Set the Period when the costume will be use", null, new ConfigurationManagerAttributes { Order = 8 }));

            //patchedHooks =
            Harmony.CreateAndPatchAll(typeof(Hooks));
        }
        public enum GameDay
        {
            None = -1,
            Monday = 0,
            Tuesday = 1,
            Wednesday = 2,
            Thursday = 3,
            Friday = 4,
            Saturday = 5,
            Sunday = 6,
            Weekend = 7,
            All = 8,
        }

        [Flags]
        public enum GamePeriod
        {
            None = 0,
            Morning = 1 << 0,
            Midday = 1 << 1,
            Evening = 1 << 2,
            Night = 1 << 3,
        }
        public static int GetMaxOutfits()
        {
            return maxOutfits.Value;
        }
        public static bool[] GetOptions()
        {
            bool[] options = [
                changePCOutfit.Value,
                weekendOutfit.Value,
                nightOutfit.Value,
                lewdOutfit.Value,
                costumeOutfit.Value,
                sportsOutfit.Value];
            return options;
        }

        public static bool[] GetExtraOptions()
        {
            bool[] extraOptions = [nightOutfitOptionOne.Value];
            return extraOptions;
        }

        public static ValueTuple<int, bool[]> GetCostumeDayAndPeriod()
        {
            var dayAndPeriod = new ValueTuple<int, bool[]>(-1, [false, false, false, false]);
            switch (costumeDay.Value)
            {
                case GameDay.None:
                    dayAndPeriod.Item1 = -1;
                    break;
                case GameDay.Monday:
                    dayAndPeriod.Item1 = 0;
                    break;
                case GameDay.Tuesday:
                    dayAndPeriod.Item1 = 1;
                    break;
                case GameDay.Wednesday:
                    dayAndPeriod.Item1 = 2;
                    break;
                case GameDay.Thursday:
                    dayAndPeriod.Item1 = 3;
                    break;
                case GameDay.Friday:
                    dayAndPeriod.Item1 = 4;
                    break;
                case GameDay.Saturday:
                    dayAndPeriod.Item1 = 5;
                    break;
                case GameDay.Sunday:
                    dayAndPeriod.Item1 = 6;
                    break;
                case GameDay.Weekend:
                    dayAndPeriod.Item1 = 7;
                    break;
                case GameDay.All:
                    dayAndPeriod.Item1 = 8;
                    break;
            }

            if ((costumePeriod.Value & GamePeriod.Morning) != 0) dayAndPeriod.Item2[0] = true;
            if ((costumePeriod.Value & GamePeriod.Midday) != 0) dayAndPeriod.Item2[1] = true;
            if ((costumePeriod.Value & GamePeriod.Evening) != 0) dayAndPeriod.Item2[2] = true;
            if ((costumePeriod.Value & GamePeriod.Night) != 0) dayAndPeriod.Item2[3] = true;

            return dayAndPeriod;
        }
        internal static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(SimulationScene), nameof(SimulationScene.Start))]
            public static void GetChangeOfClothesList(SimulationScene __instance)
            {
                MoreOutfits.CreateOldChangeOfClothesList();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HumanCustom), nameof(HumanCustom.Start))]
            public static void CreateOutfitsSlotsMaker(HumanCustom __instance)
            {
                if (maxOutfits.Value > 3) MoreOutfits.CreateNewOutfitIcons(outfitsList, maxOutfits.Value);
            }

            [HarmonyPrefix] //On Postfix, toggles don't work
            [HarmonyPatch(typeof(CoordeSelect), nameof(CoordeSelect.Initialize))]
            public static void CreateOutfitsSlotsSimulation(CoordeSelect __instance)
            {
                if (maxOutfits.Value > 3) MoreOutfits.CreateNewOutfitIcons(outfitsList, maxOutfits.Value);
            }

            [HarmonyPrefix] //Load Character with new outfits
            [HarmonyPatch(typeof(HumanData), nameof(HumanData.Copy))]
            public static void IncreaseCharaOutfits(HumanData __instance, HumanData dst, HumanData src)
            {
                if (dst.Coordinates.Count != maxOutfits.Value)
                {
                    Il2CppReferenceArray<HumanDataCoordinate> moreCoordinate = new Il2CppReferenceArray<HumanDataCoordinate>(maxOutfits.Value);
                    for (int i = 0; i < maxOutfits.Value; i++)
                    {
                        if (i >= dst.Coordinates.Count) moreCoordinate[i] = new(dst.Coordinates[0]);
                        else moreCoordinate[i] = dst.Coordinates[i];
                    }
                    dst.Coordinates = moreCoordinate;
                }
            }

            [HarmonyPrefix] //Used when adding a character into the main game, otherwise the other method works fine.
            [HarmonyPatch(typeof(HumanData), nameof(HumanData.SetCoordinateBytes))]
            public static void ForceIncreaseCharaOutfits(HumanData __instance)
            {
                if (__instance != null)
                {
                    if (__instance.Coordinates.Count != maxOutfits.Value)
                    {
                        Il2CppReferenceArray<HumanDataCoordinate> moreCoordinate = new Il2CppReferenceArray<HumanDataCoordinate>(maxOutfits.Value);
                        for (int i = 0; i < maxOutfits.Value; i++)
                        {
                            if (i >= __instance.Coordinates.Count) moreCoordinate[i] = new(__instance.Coordinates[0]);
                            else moreCoordinate[i] = __instance.Coordinates[i];
                        }
                        __instance.Coordinates = moreCoordinate;
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CoordeSelect), nameof(CoordeSelect.SetCoordeTitle))]
            public static void CustomCoordTittle(CoordeSelect __instance, int type)
            {
                if (type > 2)
                {
                    __instance._txtCoordeTitle._tmpText.text = outfitsList[type - 3] + __instance._txtCoordeTitle._tmpText.text;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CoordinateTypeChange), nameof(CoordinateTypeChange.ChangeType))]
            public static void DisplayOutfitName(CoordinateTypeChange __instance, int type)
            {
                if (__instance._human.data.Coordinates.Count > __instance._coordinateTypeNames.Count)
                {
                    if (__instance._coordinateTypeNames.Count < 3)
                    {
                        var names = new Il2CppStringArray(__instance._human.data.Coordinates.Count)
                        {
                            //Create array for JP languaje.
                            [0] = "私服",
                            [1] = "役職服",
                            [2] = "水着"
                        };

                        int number = 0;
                        for (int i = 0; i < __instance._human.data.Coordinates.Count; i++)
                        {
                            if (i >= 3)
                            {
                                names[i] = outfitsList[number];
                                number++;
                            }
                        }
                        __instance._coordinateTypeNames = names;
                    }
                    else
                    {
                        var names = new Il2CppStringArray(__instance._human.data.Coordinates.Count);

                        int number = 0;
                        for (int i = 0; i < __instance._human.data.Coordinates.Count; i++)
                        {
                            if (i >= __instance._coordinateTypeNames.Count)
                            {
                                names[i] = outfitsList[number];
                                number++;
                            }
                            else names[i] = __instance._coordinateTypeNames[i];
                        }
                        __instance._coordinateTypeNames = names;
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CoordinateCopyFace), nameof(CoordinateCopyFace.UpdateCustomUI))]
            public static void CoordCopyFace(CoordinateCopyFace __instance)
            {
                if (__instance._human == null) return;

                if (__instance._human.data.Coordinates.Count != __instance._ddDstCoordeType.TMP_Dropdown.options.Count)
                {
                    int number = 0;
                    for (int i = 0; i < __instance._human.data.Coordinates.Count; i++)
                    {
                        if (i < 3) continue;
                        TMP_Dropdown.OptionData newOption = new();
                        newOption.m_Text = outfitsList[number];
                        newOption.text = outfitsList[number];
                        __instance._ddDstCoordeType.TMP_Dropdown.options.Add(newOption);
                        __instance._ddSrcCoordeType.TMP_Dropdown.options.Add(newOption);
                        number++;
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CoordinateCopyBody), nameof(CoordinateCopyBody.UpdateCustomUI))]
            public static void CoordCopyBody(CoordinateCopyBody __instance)
            {
                if (__instance._human == null) return;

                if (__instance._human.data.Coordinates.Count != __instance._ddDstCoordeType.TMP_Dropdown.options.Count)
                {
                    int number = 0;
                    for (int i = 0; i < __instance._human.data.Coordinates.Count; i++)
                    {
                        if (i < 3) continue;
                        TMP_Dropdown.OptionData newOption = new();
                        newOption.m_Text = outfitsList[number];
                        newOption.text = outfitsList[number];
                        __instance._ddDstCoordeType.TMP_Dropdown.options.Add(newOption);
                        __instance._ddSrcCoordeType.TMP_Dropdown.options.Add(newOption);
                        number++;
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CoordinateCopyHair), nameof(CoordinateCopyHair.UpdateCustomUI))]
            public static void CoordCopyHair(CoordinateCopyHair __instance)
            {
                if (__instance._human == null) return;

                if (__instance._human.data.Coordinates.Count != __instance._ddDstCoordeType.TMP_Dropdown.options.Count)
                {
                    int number = 0;
                    for (int i = 0; i < __instance._human.data.Coordinates.Count; i++)
                    {
                        if (i < 3) continue;
                        TMP_Dropdown.OptionData newOption = new();
                        newOption.m_Text = outfitsList[number];
                        newOption.text = outfitsList[number];
                        __instance._ddDstCoordeType.TMP_Dropdown.options.Add(newOption);
                        __instance._ddSrcCoordeType.TMP_Dropdown.options.Add(newOption);
                        //__instance._ddDstCoordeType.TMP_Dropdown.RefreshShownValue();
                        number++;
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CoordinateCopyClothes), nameof(CoordinateCopyClothes.UpdateCustomUI))]
            public static void CoordCopyClothes(CoordinateCopyClothes __instance)
            {
                if (__instance._human == null) return;

                if (__instance._human.data.Coordinates.Count != __instance._ddDstCoordeType.TMP_Dropdown.options.Count)
                {
                    int number = 0;
                    for (int i = 0; i < __instance._human.data.Coordinates.Count; i++)
                    {
                        if (i < 3) continue;
                        TMP_Dropdown.OptionData newOption = new();
                        newOption.m_Text = outfitsList[number];
                        newOption.text = outfitsList[number];
                        __instance._ddDstCoordeType.TMP_Dropdown.options.Add(newOption);
                        __instance._ddSrcCoordeType.TMP_Dropdown.options.Add(newOption);
                        number++;
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CoordinateCopyAccessory), nameof(CoordinateCopyAccessory.UpdateCustomUI))]
            public static void CoordCopyAccessory(CoordinateCopyAccessory __instance)
            {
                if (__instance._human == null) return;

                if (__instance._human.data.Coordinates.Count != __instance._ddDstCoordeType.TMP_Dropdown.options.Count)
                {
                    int number = 0;
                    for (int i = 0; i < __instance._human.data.Coordinates.Count; i++)
                    {
                        if (i < 3) continue;
                        TMP_Dropdown.OptionData newOption = new();
                        newOption.m_Text = outfitsList[number];
                        newOption.text = outfitsList[number];
                        __instance._ddDstCoordeType.TMP_Dropdown.options.Add(newOption);
                        __instance._ddSrcCoordeType.TMP_Dropdown.options.Add(newOption);
                        number++;
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ThinkingManager), nameof(ThinkingManager.GetChangeOfClothesNum))]
            public static void SetOutfitSelection(Actor _actor, bool _isStart, int _timezone)
            {
                if (_timezone >= 0)
                {
                    MoreOutfits.SetDailyOutfit(_actor, _isStart, _timezone, maxOutfits.Value);
                }
            }

            /*[HarmonyPostfix]
            [HarmonyPatch(typeof(ThinkingManager), nameof(ThinkingManager.GetChangeOfClothesNum))]
            public static int ChangeClothesPerPeriodPost(int __result,Actor _actor, bool _isStart, int _timezone)
            {
                if (_timezone < 0)
                {
                    //Log.LogInfo($"Change into Coordinate:{__result}");
                }
                return __result;
            }*/

            //Doesn't work
            /*[HarmonyPrefix]
            [HarmonyPatch(typeof(HumanData), nameof(HumanData.SetCoordinateBytes))]
            public static void CustomCharaTest(HumanData __instance, ref Il2CppStructArray<byte> data, Il2CppSystem.Version version)
            {
                Il2CppSystem.Collections.Generic.List<Il2CppStructArray<byte>> list = MessagePack.MessagePackSerializer.Deserialize<Il2CppSystem.Collections.Generic.List<Il2CppStructArray<byte>>>(data);

                //Reinitialize the array with the new length
                __instance.Coordinates = new Il2CppReferenceArray<HumanDataCoordinate>(list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    if (__instance.Coordinates != null) __instance.Coordinates[i] = new HumanDataCoordinate();
                }

                //Load all the coordinates
                for (int i = 0; i < list.Count; i++)
                {
                    __instance.Coordinates?[i].LoadBytes(list[i], version);
                }
            }*/
        }
    }
}
