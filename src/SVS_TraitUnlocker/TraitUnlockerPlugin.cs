using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using CharacterCreation.UI;
using Character;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Collections.Generic;
using Manager;
using SVS_TraitUnlocker;

namespace TraitUnlocker
{
    [BepInPlugin(GUID, DisplayName, Version)]
    public class TraitUnlockerPlugin : BasePlugin
    {
        public const string DisplayName = Constants.Name;
        public const string GUID = "SVS_TraitUnlocker";
        public const string Version = Constants.Version;

        internal static new ManualLogSource Log;
        private static Harmony patchedHooks;
        public override void Load()
        {
            Log = base.Log;
            var EnableConfig = Config.Bind("General","Enable",true,"Reload the game to Enable/Disable");

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
            [HarmonyPatch(typeof(HumanDataGameParameter_SV), nameof(HumanDataGameParameter_SV.Copy))]
            private static void AnswerMaxOverride(HumanDataGameParameter_SV __instance)
            {
                int _maxTrait = Game._IndividualityInfoTable_k__BackingField.Count;
                int _maxHTrait = Game._PreferenceHInfoTable_k__BackingField.Count;

                if (__instance.individuality.answer.Length == 2)
                {
                    __instance.individuality.answer = new Il2CppStructArray<int>(_maxTrait);
                    //Log.LogInfo($"Answer Max Override DONE!");
                }

                if (__instance.individuality.answer.Length > 2)
                {
                    for (int i = 0; i < _maxTrait; i++)
                    {
                        __instance.individuality.answer[i] = -1;
                    }
                }

                if (__instance.preferenceH.answer.Length == 2)
                {
                    __instance.preferenceH.answer = new Il2CppStructArray<int>(_maxHTrait);
                }

                if (__instance.preferenceH.answer.Length > 2)
                {
                    for (int i = 0; i < _maxHTrait; i++)
                    {
                        __instance.preferenceH.answer[i] = -1;
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(AnswerCheckGroupWindow), nameof(AnswerCheckGroupWindow.Initialize))]
            private static void ToggleGroupOverride(AnswerCheckGroupWindow __instance, CharacterCreation.UI.View.CategoryView viewer, IReadOnlyDictionary<int, string> table, HumanDataGameParameter_SV.AnswerBase answerBase)
            {
                int _maxTrait = Game._IndividualityInfoTable_k__BackingField.Count;

                __instance._toggleGroup._onTotal_k__BackingField = _maxTrait;
            }
        }
    }
}
