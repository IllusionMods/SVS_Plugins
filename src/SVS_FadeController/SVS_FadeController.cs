using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System;
using UnityEngine;

namespace SVS_FadeController
{
    [BepInPlugin(GUID, DisplayName, Version)]
    public class SVS_FadeController : BasePlugin
    {
        public const string DisplayName = Constants.Name;
        public const string GUID = "SVS_FadeController";
        public const string Version = Constants.Version;

        internal static new ManualLogSource Log;
        private static Harmony patchedHooks;
        private static ConfigEntry<float> _newTime;
        public static ConfigEntry<Color> _fadeColor { get; set; }

        public override void Load()
        {
            Log = base.Log;
            var colorConverter = new TypeConverter
            {
                ConvertToString = (obj, type) => ColorUtility.ToHtmlStringRGBA((Color)obj),
                ConvertToObject = (str, type) =>
                {
                    if (!ColorUtility.TryParseHtmlString("#" + str.Trim('#', ' '), out var c))
                        throw new FormatException("Invalid color string, expected hex #RRGGBBAA");
                    return c;
                }
            };
            TomlTypeConverter.AddConverter(typeof(Color), colorConverter);


            var EnableConfig = Config.Bind("General", "Enable", true, "Reload the game to Enable/Disable");
            _newTime = Config.Bind("General", "Fade Timer", 1f, new ConfigDescription("Fade Timer in Seconds, reload the game to apply changes", new AcceptableValueRange<float>(0.1f, 1f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false }));
            //_fadeColor = Config.Bind("General", "Fade Color", FadeColor.white, "Color of the fade transition");
            _fadeColor = Config.Bind("General", "Fade Color", new Color(1, 1, 1, 1f), "Color of the fade transition");

            if (EnableConfig.Value)
            {
                patchedHooks = Harmony.CreateAndPatchAll(typeof(Hooks));
            }
        }
        public override bool Unload()
        {
            patchedHooks?.UnpatchSelf();
            return true;
        }

        internal static class Hooks
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(SceneFadeCanvas), nameof(SceneFadeCanvas.Awake))]
            private static void AwakePreFix(SceneFadeCanvas __instance)
            {      
                if (_fadeColor.Value != Color.white) __instance.SetColor(_fadeColor.Value);
                else __instance.SetColor(Color.white);
                __instance._time = _newTime.Value;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SceneFadeCanvas), nameof(SceneFadeCanvas.SetColor))]
            private static void SetColorPreFix(SceneFadeCanvas __instance, ref Color _color)
            {
                if (_fadeColor.Value != Color.white) _color = _fadeColor.Value;
                else _color = Color.white;
            }
        }    
    }
}
