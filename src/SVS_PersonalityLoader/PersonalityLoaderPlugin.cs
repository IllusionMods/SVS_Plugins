using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;
using SV;
using SV.Talk;
using SV.Chara;
using SV.Config;
using System.IO;
using Manager;
using SVS_PersonalityLoader;

namespace PersonalityLoader
{
    [BepInPlugin(GUID, DisplayName, Version)]
    public class PersonalityLoaderPlugin : BasePlugin
    {
        public const string DisplayName = Constants.Name;
        public const string GUID = "SVS_PersonalityLoader";
        public const string Version = Constants.Version;

        internal static new ManualLogSource Log;
        private static Harmony patchedHooks;

        public static string PersonalityDirectory { get; } = Path.Combine(Paths.GameRootPath, "abdata\\etcetra\\list");

        public override void Load()
        {
            Log = base.Log;
            
            patchedHooks = Harmony.CreateAndPatchAll(typeof(Hooks));
        }
        public override bool Unload()
        {
            patchedHooks?.UnpatchSelf();
            return true;
        }
        internal static class Hooks
        {
            static int _tmpPerso_A;
            static int _tmpPerso_B;
            static int _tmpPerso_C;
            static int _tmpPerso_D;

            static int _tempID_A;
            static int _tempID_B;
            static int _tempID_C;
            static int _tempID_D;

            static int _tempIndexNPC_A;
            static int _tempIndexNPC_B;
            static int _tempIndexNPC_C;
            static int _tempIndexNPC_D;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TalkTaskBase), nameof(TalkTaskBase.ADVStartInPlayer))]
            public static void CustomPersonalityPreLoad(TalkTaskBase __instance, string _advAsset, int _charaID, int _category, int _playerAction, AI _player, AI _npc, AI _third, AI _fourth, AI _fifth, bool _isBackGround = true, bool _isResetClotheType = true)
            {
                _tmpPerso_A = -1;
                _tmpPerso_B = -1;
                _tmpPerso_C = -1;
                _tmpPerso_D = -1;

                _tempID_A = -1;
                _tempID_B = -1;
                _tempID_C = -1;
                _tempID_D = -1;

                _tempIndexNPC_A = -1;
                _tempIndexNPC_B = -1;
                _tempIndexNPC_C = -1;
                _tempIndexNPC_D = -1;

                if (_npc != null)
                {
                    if ((_npc.charaData.parameter.personality > 15 && _npc.charaData.parameter.personality < 100) || _npc.charaData.parameter.personality > 104)
                    {
                        _tmpPerso_A = _npc.charaData.parameter.personality;
                        _tempID_A = _npc.charaData.charasGameParam._Index_k__BackingField;
                    }
                }

                if (_third != null)
                {
                    if ((_third.charaData.parameter.personality > 15 && _third.charaData.parameter.personality < 100) || _third.charaData.parameter.personality > 104)
                    {
                        _tmpPerso_B = _third.charaData.parameter.personality;
                        _tempID_B = _third.charaData.charasGameParam._Index_k__BackingField;
                    }
                }

                if (_fourth != null)
                {
                    if ((_fourth.charaData.parameter.personality > 15 && _fourth.charaData.parameter.personality < 100) || _fourth.charaData.parameter.personality > 104)
                    {
                        _tmpPerso_C = _fourth.charaData.parameter.personality;
                        _tempID_C = _fourth.charaData.charasGameParam._Index_k__BackingField;
                    }
                }

                if (_fifth != null)
                {
                    if ((_fifth.charaData.parameter.personality > 15 && _fifth.charaData.parameter.personality < 100) || _fifth.charaData.parameter.personality > 104)
                    {
                        _tmpPerso_D = _fifth.charaData.parameter.personality;
                        _tempID_D = _fifth.charaData.charasGameParam._Index_k__BackingField;
                    }                    
                }

                if (_npc != null || _third != null || _fourth != null || _fifth != null)
                {
                    var _sim = SimulationScene._instance.tempAIs;
                    if (_sim != null)
                    {
                        int ind = 0;
                        foreach (var _NPC in _sim)
                        {
                            if (_NPC.charaData.charasGameParam._Index_k__BackingField == _tempID_A)
                            {
                                SimulationScene._instance.tempAIs[ind]._charaData._chaCtrl_k__BackingField._data_k__BackingField.Parameter.personality = PersonalityLoaderFunctions.SetCustomPersonalityAnimation(PersonalityDirectory, _tmpPerso_A);
                                _tempIndexNPC_A = ind;                            
                            }
                            if (_NPC.charaData.charasGameParam._Index_k__BackingField == _tempID_B)
                            {
                                SimulationScene._instance.tempAIs[ind]._charaData._chaCtrl_k__BackingField._data_k__BackingField.Parameter.personality = PersonalityLoaderFunctions.SetCustomPersonalityAnimation(PersonalityDirectory, _tmpPerso_B);
                                _tempIndexNPC_B = ind;
                            }
                            if (_NPC.charaData.charasGameParam._Index_k__BackingField == _tempID_C)
                            {
                                SimulationScene._instance.tempAIs[ind]._charaData._chaCtrl_k__BackingField._data_k__BackingField.Parameter.personality = PersonalityLoaderFunctions.SetCustomPersonalityAnimation(PersonalityDirectory, _tmpPerso_C);
                                _tempIndexNPC_C = ind;
                            }
                            if (_NPC.charaData.charasGameParam._Index_k__BackingField == _tempID_D)
                            {
                                SimulationScene._instance.tempAIs[ind]._charaData._chaCtrl_k__BackingField._data_k__BackingField.Parameter.personality = PersonalityLoaderFunctions.SetCustomPersonalityAnimation(PersonalityDirectory, _tmpPerso_D);
                                _tempIndexNPC_D = ind;
                            }
                            ind++;
                        }
                    }
                }
            }
            
            [HarmonyPostfix]
            [HarmonyPatch(typeof(TalkTaskBase), nameof(TalkTaskBase.ADVStartInPlayer))]
            public static void CustomPersonalityPostLoad(TalkTaskBase __instance, string _advAsset, int _charaID, int _category, int _playerAction, AI _player, AI _npc, AI _third, AI _fourth, AI _fifth, bool _isBackGround = true, bool _isResetClotheType = true)
            {
                if ((_tmpPerso_A > 15 && _tmpPerso_A < 100) || _tmpPerso_A > 104)
                {
                    SimulationScene._instance.tempAIs[_tempIndexNPC_A]._charaData._chaCtrl_k__BackingField._data_k__BackingField.Parameter.personality = _tmpPerso_A;
                    //_tmpPerso_A = -1;
                }
                if ((_tmpPerso_B > 15 && _tmpPerso_B < 100) || _tmpPerso_B > 104)
                {
                    SimulationScene._instance.tempAIs[_tempIndexNPC_A]._charaData._chaCtrl_k__BackingField._data_k__BackingField.Parameter.personality = _tmpPerso_B;
                    //_tmpPerso_B = -1;
                }
                if ((_tmpPerso_C > 15 && _tmpPerso_C < 100) || _tmpPerso_C > 104)
                {
                    SimulationScene._instance.tempAIs[_tempIndexNPC_A]._charaData._chaCtrl_k__BackingField._data_k__BackingField.Parameter.personality = _tmpPerso_C;
                    //_tmpPerso_C = -1;
                }
                if ((_tmpPerso_D > 15 && _tmpPerso_D < 100) || _tmpPerso_D > 104)
                {
                    SimulationScene._instance.tempAIs[_tempIndexNPC_A]._charaData._chaCtrl_k__BackingField._data_k__BackingField.Parameter.personality = _tmpPerso_D;
                    //_tmpPerso_D = -1;
                }
            }

            //Game Voice Setting
            [HarmonyPostfix]
            [HarmonyPatch(typeof(VoiceSetting), nameof(VoiceSetting.Init))]
            public static void CreateCustomPersonalityVoiceSetting(VoiceSetting __instance)
            {              
                if (Game.expIDCharaDic == null) return;

                if (Game.expIDCharaDic.Count != 0)
                {
                    Il2CppSystem.Collections.Generic.List<int> _persoKeys = new Il2CppSystem.Collections.Generic.List<int>();
                    foreach (var ID in Game.expIDCharaDic)
                    {
                        if (ID.Key > 99)
                        {
                            if (!_persoKeys.Contains(ID.Key)) _persoKeys.Add(ID.Key);
                        }
                    }

                    var _personalities = Manager.Voice.InfoTable;
                    if (_personalities == null) return;

                    var _num = __instance._table.Count + 3;
                    foreach (var _perso in _persoKeys)
                    {
                        if (_perso > 99)
                        {
                            if (_personalities.ContainsKey(_perso))
                            {
                                __instance.Create(_num, _perso, _personalities[_perso].Personality);
                                Log.LogInfo($"Created Voice Setting for: {_personalities[_perso].Personality} ID:{_perso}");
                                _num++;
                            }
                        }
                    }
                }
            }
        }
    }
}
