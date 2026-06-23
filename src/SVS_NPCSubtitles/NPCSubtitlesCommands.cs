
namespace SVS_NPCSubtitles
{
    internal class NPCSubtitlesCommands
    {
        public static string GetCharaIntention(int commandID, out bool success)
        {
            success = true;
            switch (commandID)
            {
                case 0://DailyLifeTalk
                    return "\"Daily Life Talk\"";

                case 1://LoveTalk
                    return "\"Love Talk\"";

                case 2://EroTalk
                    return "\"Lewd Talk\"";

                case 3://Cheer
                    return "\"Cheer up\"";

                case 4://Soothe
                    return "\"Calm down\"";

                case 5://Praise
                    return "\"Praise\"";

                case 6://Grumble
                    return "\"Grumble\"";

                case 7://Apologize
                    return "\"Apologize\"";

                case 8://SpeakIllOf
                    return "\"Beware of...\"";

                case 9://SolveMyWeaknesses
                    return "\"I'm getting blackmail!\"";

                case 10://MealsRecommendations
                    return "\"Meal recommendations\"";

                case 11://StudyRecommendations
                    return "\"Study recommendations\"";

                case 12://MotionRecommendations
                    return "\"Exercise recommendations\"";

                case 13://PartTimeJobRecommendations
                    return "\"Work recommendations\"";

                case 14://HumanRelationsReflection
                    return "\"Be mindful of others\"";

                case 15://IllicitLoveReflection
                    return "\"Be mindful of others feelings\"";

                case 16://LoveAffairReflection
                    return "\"Be mindful of others relationships\"";

                case 17://GoodThingsAboutYou
                    return "\"Good rumor about you\"";

                case 18://BadRumors
                    return "\"Bad rumors about you\"";

                case 19://GetAlong
                    return "\"Get a long with [NPC_thatPersonName]\"";

                case 20://WantToGetAlong
                    return "\"I want to get along with [NPC_thatPersonName]\"";

                case 21://WhatDoYouThinkAboutThatGuy
                    return "\"what do you think of [NPC_thatPersonName]\"";

                case 22://DinnerWithYou
                    return "\"Let's eat together\"";

                case 23://StudyWithYou
                    return "\"Let's study together\"";

                case 24://ExerciseWithYou
                    return "\"Let's exercise together\"";

                case 25://HelpMeWithThis
                    return "\"Let's work together\"";

                case 26://ComeToMyRoomWithYou
                    return "\"Come to my room\"";

                case 27://ShowGoodwill
                    return "\"Show Goodwill\"";

                case 28://AppointmentForAParty
                    return "\"Inviting to BBQ\"";

                case 29://OfferToTakePartInARelationship
                    return "\"Asking to become Lovers\"";

                case 30://BrokeItOff
                    return "\"Breaking Up!\"";

                case 31://Caressing
                    return "\"Headpat\"";

                case 32://HugSomeoneClose
                    return "\"Hug\"";

                case 33://Kiss
                    return "\"Kiss\"";

                case 34://Touch
                    return "\"Grope\"";

                case 35://H
                    return "\"Asking for Sex\"";

                case 36://ComeHere
                    return "\"Follow me\"";

                case 37://TakingOnesLover
                    return "\"Follow me for sex\"";

                case 38://ComeBackHereLater
                    return "\"Come here later\"";

                case 39://Nothing
                    return "\"Nothing\"";

                case 40://LetsEatPeople
                    return "\"Let's all go to eat!\"";

                case 41://LetsStudy
                    return "\"Let's all go to study!\"";

                case 42://LetsAllExercise
                    return "\"Let's all do some exercise!\"";

                case 43://LetsAllHelp
                    return "\"Let's all go to work!\"";

                case 44://AllToBeFriendsWithThatMan
                    return "\"Let's be friend with [NPC_thatPersonName]\"";

                case 45://GatherAround
                    return "\"Everyone come here!\"";

                case 46://LetsAllHaveSex
                    return "\"Let's All Have Sex!\"";

                case 47://Scramble
                    return "\"Scramble\"";

                case 48://Intervention
                    return "\"Intervening\"";

                case 49://StoppingFight
                    return "\"Stop fighting!\"";

                case 50://ScrambleForH
                    return "\"Scramble for sex\"";

                case 51://StoppingH
                    return "\"Stop having sex!\"";

                case 52://LetsHave3P
                    return "\"Let's have 3P!\"";

                case 53://KissMe
                    return "\"Kiss me\"";

                case 54://GiveYouAHug
                    return "\"I will hug you\"";

                case 55://HowsItGoing
                    return "\"How is it going?\"";

                case 56://DidYouHaveSexWithThatGuy
                    return "\"Did you have sex with [NPC_thatPersonName]?\"";

                case 57://LikeThat
                    return "\"Like that\"";

                case 58://HaveYouEverHadSex
                    return "\"Have you ever had sex?\"";

                case 59://DatingAnyone
                    return "\"Are you dating anyone?\"";

                case 60://TradePossessions
                    return "\"Trade items\"";

                case 61://BadRumorPursuit
                    return "\"Bad rumor\"";

                case 62://AbductTheSubject
                    return "\"Abduction\"";

                case 63://AbductionOfTheTargetSubject
                    return "\"Abduction\"";

                case 64://GoodMorningKiss
                    return "\"Good morning kiss\"";

                case 65://SurpriseKiss
                    return "\"Surprise kiss\"";

                case 66://BreakingUpWithYou
                    return "\"Breaking Up!\"";

                case 67://GonnaCheatOnYou
                    return "\"I'm gonna cheat!\"";

                case 68://NotTellAnyoneAboutThis
                    return "\"Don't tell anyone about this\"";

                case 69://DontFollowMe
                    return "\"Don't follow me!\"";

                case 70://KidnapperAdvanceNotice
                    return "\"Someone went missing\"";

                case 71://ThatGuySeemsToLikeYou
                    return "\"[NPC_thatPersonName] seem to like you\"";

                case 72://ThatGuyWasConfessing
                    return "\"[NPC_thatPersonName] was confessing\"";

                case 73://TheGuyWasHavingSex
                    return "\"[NPC_thatPersonName] was having sex\"";

                case 74://FollowedByThatPerson
                    return "\"[NPC_thatPersonName] is following me\"";

                case 75://YoureVeryPopular
                    return "\"You are very popular!\"";

                case 76://DarknessDailyH
                    return "\"Evil sex ritual\"";

                case 77://WantA3P
                    return "\"I want to do 3P\"";

                case 78://TakeAdvantageOfWeakness
                    return "\"You are gonna obey me or else..\"";

                case 79://KeepHimAway
                    return "\"Don't talk to...\"";

                case 80://LetMeSeeThat
                    return "\"Show me that\"";

                case 81://LetMeHold
                    return "\"Let me do it now!\"";

                case 82://EliminateWeaknesses
                    return "\"You are free to go\"";

                case 83://SoloCooking
                    return "\"Eating\"";

                case 84://SoloStudy
                    return "\"Studying\"";

                case 85://SoloTraining
                    return "\"Training\"";

                case 86://SoloPartTimeJob
                    return "\"Working\"";

                case 87://Consult_Meals
                    return "\"Should I eat more?\"";

                case 88://Consult_Study
                    return "\"Should I study more?\"";

                case 89://Consult_Exercise
                    return "\"Should I exercise more?\"";

                case 90://Consult_PartTimeJob
                    return "\"Should I work more?\"";

                case 91://Consult_Relations
                    return "\"Should I be more friendly?\"";

                case 92://Consult_Love
                    return "\"Should I be more romantic?\"";

                case 93://Consult_H
                    return "\"Should I be more promiscuous?\"";

                case 94://LetsAllHomeParty
                    return "\"Let's start the BBQ!\"";

                case 95://ImportantItemExchange
                    return "\"Important item trade\"";

                case 96://ComeToMyRoomWithYouHVer
                    return "\"Come to my room for... you know...\"";

                case 97://LetsAllHaveSex_Combination
                    return "\"Let's All Have Sex!\"";
                default:
                    success = false;
                    return "\"Unknown Talk\"";
            }
        }
    }
}
