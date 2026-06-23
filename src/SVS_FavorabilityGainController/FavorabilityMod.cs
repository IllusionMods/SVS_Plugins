using Character;
using FavorabiltyGainController;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SaveData;
using SV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FavorabiltyGainController
{
    internal class FavorabilityMod
    {
        private static System.Random _rnd = new System.Random();

        public static FavourableImpressionManager RemoveFirstImpression(FavourableImpressionManager _instance, HumanData _myCharaData, CharactersGameParameter _myGameParam, HumanData _targetCharaData, CharactersGameParameter _targetGameParam)
        {
            if (_myGameParam.sensitivity.tableFavorabiliry.ContainsKey(_targetGameParam._Index_k__BackingField))
            {
                if (_myGameParam.sensitivity.tableFavorabiliry[_targetGameParam._Index_k__BackingField].longStocks.Count < 21)
                {
                    _instance.addRates[0] = 1f;
                    _instance.addRates[1] = 1f;
                    _instance.addRates[2] = 1f;
                    _instance.addRates[3] = 1f;

                    switch (_myCharaData.GameParameter.sexualTarget)
                    {
                        case 0: //Hetero
                            if (_myCharaData.Parameter.sex != _targetCharaData.Parameter.sex)
                            {
                                _instance.addRates[1] = 0;
                            }
                            else
                            {
                                _instance.addRates[0] = 0;
                            }
                            break;

                        case 1: //Lean hetero
                            if (_myCharaData.Parameter.sex != _targetCharaData.Parameter.sex)
                            {
                                _instance.addRates[1] = 0.5f;
                            }
                            else
                            {
                                _instance.addRates[0] = 0.5f;
                            }
                            break;
                        case 3: //Lean Homo
                            if (_myCharaData.Parameter.sex == _targetCharaData.Parameter.sex)
                            {
                                _instance.addRates[1] = 0.5f;
                            }
                            else
                            {
                                _instance.addRates[0] = 0.5f;
                            }
                            break;

                        case 4://Homo
                            if (_myCharaData.Parameter.sex == _targetCharaData.Parameter.sex)
                            {
                                _instance.addRates[1] = 0;
                            }
                            else
                            {
                                _instance.addRates[0] = 0f;
                            }
                            break;
                    }
                }              
            }
            return _instance;
        }

        public static FavourableImpressionManager SetHeteroHomoFriendGain(FavourableImpressionManager _instance, HumanData _myCharaData, HumanData _targetCharaData)
        {
            if (_myCharaData.GameParameter.sexualTarget == 0)
            {
                if (_myCharaData.Parameter.sex != _targetCharaData.Parameter.sex)
                {
                    _instance.addRates[1] = 0.5f;
                }
            }
            if (_myCharaData.GameParameter.sexualTarget == 4)
            {
                if (_myCharaData.Parameter.sex == _targetCharaData.Parameter.sex)
                {
                    _instance.addRates[1] = 0.5f;
                }
            }
            return _instance;
        }
    }
}
