using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SaveData;
using SV.H;
using SV.H.UI;

namespace SVS_CharaSweat
{
    [BepInPlugin(GUID, DisplayName, Version)]
    public class CharaSweatPlugin : BasePlugin
    {
        public const string DisplayName = Constants.Name;
        public const string GUID = "DS27.SVS.CharaSweat";
        public const string Version = Constants.Version;

        internal static new ManualLogSource Log;
        private static Harmony patchedHooks;

        private static ConfigEntry<bool> _enableHSweat;

        public override void Load()
        {
            //Plugin startup logic
            Log = base.Log;

            _enableHSweat = Config.Bind("H Scene", "Enable Sweat", true, "If enabled, characters will start to sweat during H scenes");
            
            patchedHooks = Harmony.CreateAndPatchAll(typeof(Hooks));
        }
        internal static class Hooks
        {
            private static bool[] sweatPC = { false, false };
            private static bool[] sweatNPC = { false, false };
            private static bool[] sweatNPC1 = { false, false };
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HScene), nameof(HScene.InitializeParameter))]
            public static void StartHScene(HScene __instance, Parameter parameter, Il2CppReferenceArray<Actor> actors)
            {
                sweatPC = [false, false];
                sweatNPC = [false, false];
                sweatNPC1 = [false, false];
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HScene), nameof(HScene.Update))]
            public static void HSceneUpdate(HScene __instance)
            { 
                if (_enableHSweat.Value)
                {
                    if (__instance.Actors.Count > 0)
                    {
                        if (__instance.GaugeController.Gauges.Count > 0)
                        {
                            //PC H Gauge
                            if (__instance.GaugeController.Gauges[0]._gauge._mode.Value == Gauge.Mode.High)
                            {
                                if (!sweatPC[0])
                                {
                                    if (__instance.GaugeController.Gauges[0].GaugeValue > 80f)
                                    {
                                        __instance.Actors[0].Actor.chaCtrl.ChangeSweat(0.5f);
                                        sweatPC[0] = true;
                                        Log.LogInfo($"Sweat Enable for PC ");
                                    }
                                }
                                else
                                {
                                    if (!sweatPC[1])
                                    {
                                        if (__instance.GaugeController.Gauges[0].GaugeValue > 100f)
                                        {
                                            __instance.Actors[0].Actor.chaCtrl.ChangeSweat(1f);
                                            sweatPC[1] = true;
                                            Log.LogInfo($"Sweat Enable for PC");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!sweatPC[0])
                                {
                                    if (__instance.GaugeController.Gauges[0].GaugeValue > 80f)
                                    {
                                        __instance.Actors[0].Actor.chaCtrl.ChangeSweat(0.5f);
                                        sweatPC[0] = true;
                                        Log.LogInfo($"Sweat Normal Enable for PC ");
                                    }
                                }
                            }

                            //NPC H Gauge
                            if (__instance.GaugeController.Gauges[1]._gauge._mode.Value == Gauge.Mode.High)
                            {
                                if (!sweatNPC[0])
                                {
                                    if (__instance.GaugeController.Gauges[1].GaugeValue > 80f)
                                    {
                                        __instance.Actors[1].Actor.chaCtrl.ChangeSweat(0.5f);
                                        sweatNPC[0] = true;
                                        Log.LogInfo($"Sweat Enable for NPC");
                                    }
                                }
                                else
                                {
                                    if (!sweatNPC[1])
                                    {
                                        if (__instance.GaugeController.Gauges[1].GaugeValue > 100f)
                                        {
                                            __instance.Actors[1].Actor.chaCtrl.ChangeSweat(1f);
                                            sweatNPC[1] = true;
                                            Log.LogInfo($"Sweat Enable for NPC");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!sweatNPC[0])
                                {
                                    if (__instance.GaugeController.Gauges[1].GaugeValue > 80f)
                                    {
                                        __instance.Actors[1].Actor.chaCtrl.ChangeSweat(0.5f);
                                        sweatNPC[0] = true;
                                        Log.LogInfo($"Sweat Normal Enable for NPC");
                                    }
                                }
                            }

                            //3P H Gauge
                            if (__instance.GaugeController.Gauges.Count == 3)
                            {
                                if (__instance.GaugeController.Gauges[2]._gauge._mode.Value == Gauge.Mode.High)
                                {
                                    if (!sweatNPC1[0])
                                    {
                                        if (__instance.GaugeController.Gauges[2].GaugeValue > 80f)
                                        {
                                            __instance.Actors[2].Actor.chaCtrl.ChangeSweat(0.5f);
                                            sweatNPC1[0] = true;
                                            Log.LogInfo($"Sweat Enable for NPC");
                                        }
                                    }
                                    else
                                    {
                                        if (!sweatNPC1[1])
                                        {
                                            if (__instance.GaugeController.Gauges[2].GaugeValue > 100f)
                                            {
                                                __instance.Actors[2].Actor.chaCtrl.ChangeSweat(1f);
                                                sweatNPC1[1] = true;
                                                Log.LogInfo($"Sweat Enable for NPC");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (!sweatNPC1[0])
                                    {
                                        if (__instance.GaugeController.Gauges[2].GaugeValue > 80f)
                                        {
                                            __instance.Actors[2].Actor.chaCtrl.ChangeSweat(0.5f);
                                            sweatNPC1[0] = true;
                                            Log.LogInfo($"Sweat Normal Enable for NPC");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }   
    }
}
