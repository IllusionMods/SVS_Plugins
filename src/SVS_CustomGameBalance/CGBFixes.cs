using SaveData;
using SV;
using System;

namespace SVS_CustomGameBalance
{
    internal class CGBFixes
    {
        private static Random rnd = new Random();
        public static void FixOnAnswerRate(YesNoJudgeManager.AnswerInfo answerInfo, YesNoJudgeManager.YesNoInfo yesNoInfo, int commandID, int questionCount)
        {
            var fixes = CustomGameBalancePlugin.GetGameFixes();
            if (!fixes[0]) return;
            int askingCharaID = yesNoInfo.active.charasGameParam.Index;
            if (!yesNoInfo.passive.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(askingCharaID)) return;
            float answerRate = 0;
            float modifier = 1f;

            //Buff for Fortune 7
            if (CustomGameBalancePlugin.GetFortuneFix())
            {
                if (Manager.Game.saveData.dataCount.circularNotice == 7 && Manager.Game.saveData.dataCount.isCircularNoticeUse)
                {
                    modifier = 1.1f;
                }
            }
            //Fix for inviting to a BBQ
            if (fixes[1] && commandID == 28)
            {
                //Check for if the character has BBQ planned
                if (yesNoInfo.passive.charasGameParam.memory.passivePromiseHousePartys.Count > 0 || yesNoInfo.passive.charasGameParam.memory.activePromiseHousePartys.Count > 0)
                {
                    answerInfo.rate = 0;
                    answerInfo.ans = 1;
                    return;
                }

                if (yesNoInfo.passive.gameParameter.LvChastity > 2)
                {
                    answerRate = CalcBBQAnswer(yesNoInfo.passive, yesNoInfo.active, askingCharaID, modifier);
                    if (answerRate <= 0)
                    {
                        answerInfo.rate = 0;
                        answerInfo.ans = 1;
                        return;
                    }
                    if (answerRate >= 100)
                    {
                        answerInfo.rate = answerRate;
                        answerInfo.ans = 0;
                        return;
                    }
                    else
                    {
                        int chance = rnd.Next(1, 100);
                        if (chance <= answerRate) answerInfo.ans = 0;
                        else answerInfo.ans = 1;
                    }                   
                }
                return;
            }

            //Fix for "Are you dating anyone - Confession"
            if (fixes[2] && commandID == 59)
            {
                if (questionCount == 1)
                {
                    switch (yesNoInfo.passive.gameParameter.LvChastity)
                    {
                        case 3:
                            if (yesNoInfo.passive.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(askingCharaID))
                            {
                                if (yesNoInfo.passive.charasGameParam.sensitivity.tableFavorabiliry[askingCharaID].ranks[0] == SensitivityParameter.Rank.HIGH || yesNoInfo.passive.charasGameParam.sensitivity.tableFavorabiliry[askingCharaID].ranks[0] == SensitivityParameter.Rank.MAX) break;
                                else
                                {
                                    if (yesNoInfo.passive.charasGameParam.sensitivity.tableFavorabiliry[askingCharaID].ranks[0] == SensitivityParameter.Rank.MIDDLE)
                                    {
                                        answerInfo.rate = 10 * modifier;
                                        int chance = rnd.Next(1, 100);
                                        if (chance <= answerInfo.rate) answerInfo.ans = 0;
                                        else answerInfo.ans = 1;
                                    }
                                    else
                                    {
                                        answerInfo.rate = 0;
                                        answerInfo.ans = 1;
                                    }
                                }
                            }
                            break;

                        case 4:
                            if (yesNoInfo.passive.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(askingCharaID))
                            {
                                if (yesNoInfo.passive.charasGameParam.sensitivity.tableFavorabiliry[askingCharaID].ranks[0] == SensitivityParameter.Rank.HIGH || yesNoInfo.passive.charasGameParam.sensitivity.tableFavorabiliry[askingCharaID].ranks[0] == SensitivityParameter.Rank.MAX) break;
                                else
                                {
                                    answerInfo.rate = 0;
                                    answerInfo.ans = 1;
                                }
                            }
                            break;
                    }
                }
            }
        }
        public static int CalcBBQAnswer(Actor _actor, Actor _TargetActor, int targetID, float modifier)
        {
            int baseRate = 0;

            if ((_actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[0] == SensitivityParameter.Rank.MIDDLE && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[1] == SensitivityParameter.Rank.MIDDLE)
                || _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[1] == SensitivityParameter.Rank.HIGH
                || (_actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[0] == SensitivityParameter.Rank.MIDDLE && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[2] == SensitivityParameter.Rank.LOW && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[3] == SensitivityParameter.Rank.LOW))
            {
                switch (_actor.gameParameter.LvChastity)
                {
                    case 3:
                        if (_actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[1] == SensitivityParameter.Rank.HIGH) baseRate = 60;
                        if (_actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[0] == SensitivityParameter.Rank.MIDDLE && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[1] == SensitivityParameter.Rank.MIDDLE) baseRate = 50;
                        else if (_actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[0] == SensitivityParameter.Rank.MIDDLE && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[2] == SensitivityParameter.Rank.LOW && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[3] == SensitivityParameter.Rank.LOW) baseRate = 40;
                        else break;
                        if (_actor.charasGameParam.memory.lovers.Count > 0)
                        {
                            foreach (var lover in _actor.charasGameParam.memory.lovers)
                            {
                                if (lover.id == targetID)
                                {
                                    baseRate += 22;
                                    break;
                                }
                            }
                        }
                        break;
                    case 4:
                        if (_actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[1] == SensitivityParameter.Rank.HIGH) baseRate = 40;
                        if (_actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[0] == SensitivityParameter.Rank.MIDDLE && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[1] == SensitivityParameter.Rank.MIDDLE) baseRate = 30;
                        else if (_actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[0] == SensitivityParameter.Rank.MIDDLE && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[2] == SensitivityParameter.Rank.LOW && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[3] == SensitivityParameter.Rank.LOW) baseRate = 20;
                        else break;
                        if (_actor.charasGameParam.memory.lovers.Count > 0)
                        {
                            foreach (var lover in _actor.charasGameParam.memory.lovers)
                            {
                                if (lover.id == targetID)
                                {
                                    baseRate += 11;
                                    break;
                                }
                            }
                        }
                        break;
                }

                ///Additive and subtraction
                //Check Traits
                if (_actor.gameParameter.individuality.answer.Contains(6)) baseRate += 4;
                if (_actor.gameParameter.individuality.answer.Contains(7)) baseRate += 7;
                if (_actor.gameParameter.individuality.answer.Contains(9)) baseRate -= 10;
                if (_actor.gameParameter.individuality.answer.Contains(27)) baseRate += 5;

                //Check Mood
                if (_actor.charasGameParam.state._State_k__BackingField == StateParameter.StateKind.UPLIFT) baseRate += 4;
                if (_actor.charasGameParam.state._State_k__BackingField == StateParameter.StateKind.EARNESTNESS) baseRate += 8;
                if (_actor.charasGameParam.state._State_k__BackingField == StateParameter.StateKind.ANGER) baseRate -= 16;
                if (_actor.charasGameParam.state._State_k__BackingField == StateParameter.StateKind.DISAPPOINTMENT) baseRate -= 8;
                if (_actor.charasGameParam.state._State_k__BackingField == StateParameter.StateKind.TENSION) baseRate -= 4;

                ///Multi and Division
                if (_actor.gameParameter.individuality.answer.Contains(2))//Bad with Guys
                {
                    if (_TargetActor.parameter.sex == 0)
                    {
                        var division = baseRate / 1.1f;
                        baseRate = (int)division;
                    }
                }
                if (_actor.gameParameter.individuality.answer.Contains(3))//Bad With Women
                {
                    if (_TargetActor.parameter.sex == 1)
                    {
                        var division = baseRate / 1.1f;
                        baseRate = (int)division;
                    }
                }
                if (_TargetActor.gameParameter.individuality.answer.Contains(4))//Charmer
                {
                    if (_TargetActor.parameter.sex != _actor.parameter.sex)
                    {
                        var division = baseRate * 1.1f;
                        baseRate = (int)division;
                    }
                    else
                    {
                        var division = baseRate / 1.1f;
                        baseRate = (int)division;
                    }
                }
                if (_actor.gameParameter.individuality.answer.Contains(5))//Chivalrous 
                {
                    if (_TargetActor.parameter.sex == _actor.parameter.sex)
                    {
                        var division = baseRate * 1.1f;
                        baseRate = (int)division;
                    }
                }

                ///Overrides
                if (_actor.gameParameter.individuality.answer.Contains(29))//Singleminded
                {
                    if (_actor.charasGameParam.memory.lovers.Count > 0)
                    {
                        bool foundLover = false;
                        foreach (var lover in _actor.charasGameParam.memory.lovers)
                        {
                            if (lover.id == targetID)
                            {
                                foundLover = true;
                                break;
                            }
                        }
                        if (foundLover) baseRate += 25;
                        else baseRate = 0;
                    }
                }
            }

            if (baseRate < 0) return 0;
            return (int)(baseRate * modifier);
        }
        public static int HighVirtueDoNotAskBBQ(Actor actor, int command, int result)
        {
            if (command != 28) return result;
            if (!actor.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(result)) return result;

            if (actor.gameParameter.LvChastity > 2)
            {
                if (actor.charasGameParam.sensitivity.tableFavorabiliry[result].ranks[0] == SensitivityParameter.Rank.HIGH || actor.charasGameParam.sensitivity.tableFavorabiliry[result].ranks[0] == SensitivityParameter.Rank.MAX) return result;
                else return -1;
            }
            return result;
        }
    }
}
