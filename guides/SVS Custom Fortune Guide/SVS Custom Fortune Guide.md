# Custom Fortune Guide

Guide made by Darksoldier27 (DS27). This is a copy of the original guide on [Google Docs](https://docs.google.com/document/d/1MiH7E8uqqVg104-ATxwKAf4fY2G-XhGTPpxXsosanUc) converted to markdown.

Feel free to submit edits to this guide in form of PRs to this repository.

## What You Need

- [SVS CustomFortune mod](https://github.com/IllusionMods/SVS_Plugins/releases)
- [Notepad++](https://notepad-plus-plus.org/downloads/) or similar text editor
- [Custom Fortune Template](999_Template.zip)

### Custom Fortune Folder

The template comes with a folder named: 999_Template.

The 999 is the priority level, the Custom Fortune mod will use this value to determine the order the fortunes pack, it goes from lowest to highest, so lowest values are readed first

After the _ comes the name of the pack, here you can put whatever you want.

So the name format goes like this: `[Priority]_[FortunePackName]` -> `001_MyFortunes`.

If 2 packs have the same priority, the mod will assign a priority value automatically.

You can use numbers higher than 999 just in case.

Once you have chosen your priority and name. Now we can start working on the Json File

### Custom Fortune Json

The json file is the most important file, and is the one that contains the Custom Fortune information.

The json file should keep the name: fortunes.json
The mod will search for this file name, if it can not find it, your fortunes won’t be loaded.

Open the json file that comes with the template (fortunes.json) and you need to edit the following fields.

## Initial custom fortune setup

### Enable:

The enable field is to set if the fortune will be loaded or not.

- Acceptable values: true, false

Examples:

1. `"Enable": true, <- it will be loaded`
2. `"Enable": false, <- it won’t be loaded`

### ID:

The ID field is where we set our ID, the id can be any value (even negatives) as long as it is not already in use. If the mod detects 2 or more fortunes using the same ID, only the first one that has been loaded will work, while the others will be ignored. (The mod will show a warning when it detects fortunes with the same ID)

- Acceptable values: int (Whole numbers, not decimals).

Example:

1. `"ID": 20,`

### Name:

The name field is not used by the mod, at least not for anything important. Is mostly to identify what fortune is being used.

- Acceptable values: string (text).

Examples:

1. `"Name": "My fortune",`
2. `"Name": "Trouble day",`
3. `"Name": "Good Day",`

### FortuneRate:

The Fortune Rate field is used to determine the likelihood of the fortune, the higher the number here, the more frequent this fortune will be.

- Acceptable values: int (Whole numbers not decimals).

**`Example:`**

1. `"FortuneRate": 1, <- default, same as vanilla fortunes.`
2. `"FortuneRate": 10, <- 10 times more likely.`

### FavorPoints:

The Favor Points field is used to increase or decrease the amount of points a character gets from interactions.

- Acceptable values: float (Number or decimals).

FavorPoints need 4 values, each one representing one of the favorability types:
"FavorPoints": [A,B,C,D]

- A: Love points.
- B: Friend points.
- C: Distant points.
- D: Hate points.

Examples:

1. `"FavorPoints": [1,1,1,1], <- default`
2. `"FavorPoints": [0.5,1,1,1], <- Love will gain half the amount of points.`
3. `"FavorPoints": [2,1,1,2], <- Love and Hate will gain double the amount of points.`

### StatesPoints:

The StatesPoints are used to set a modifier for the mood of the character.

- Acceptable values: int (Numbers not decimals).

StatesPoints need 10 values, each one representing a mood:
"StatesPoints": [A,B,C,D,E,F,G,H,I,J]

- A: UPLIFT
- B: SHYNESS
- C: JEALOUSY
- D: ANGER
- E: DISAPPOINTMENT
- F: PEACEOFMIND (Calm)
- G: RUT (Horny)
- H: EARNESTNESS
- I: TENSION
- J: NORMAL

Don’t go crazy with the values, with a value of 2 on any of the categories is enough to influence the mood in a big way.

Examples:

1. `"StatesPoints": [0,0,0,0,0,0,0,0,0,0], <- Default values`
2. `"StatesPoints": [-1,0,1,1,0,0,0,0,0,0], <- Reduce Uplift mood and increase Jealousy and Angry moods.`

### AddSuccessPoint:

The Add Success Point field is not really used by the game, but this mod takes this value and it uses it to increase or decrease the answer % of an action by a % amount.

- Acceptable values: int (Numbers not decimals).

As an example, let’s say the action “Casual Topic” has a success rate of 80%, if we apply a value of 10 to the addSuccessPoint, this value now becomes 88%! if our fortune gets selected. The same happens if we use a negative number like -20, now the success rate of 80% will turn into 64%.
Note: This won’t affect actions that are set as 0%

This will affect all actions unless specified on the CharaActionAnswers Field, in that case, this will only affect the actions listed there.

Examples:

1. `"AddSuccessPoint": 0, <- Default Value`
2. `"AddSuccessPoint": 10, <- Increase answer success rate by 10%`
3. `"AddSuccessPoint": -50, <- Reduce answer success rate by 50%`

### ActionsCommands:

The ActionsCommand contains some customisable fields for our fortune to use. Set each one based on what you want for your fortune to do.

#### CharaActionsAnswers:

For this field, if the value of the addSuccessPoint is not equal to 0, it will check the values contained on this field to determine the answer success rate for the actions listed here.

- Acceptable values: int (Numbers not decimals).

If you want the AddSuccessPoint to only affect a specific set of actions, you need to add the ID of the action. You can check the actions ID here.

Examples:

1. `"CharaActionsAnswers": [], <- Default value, no actions specified.`
2. `"CharaActionsAnswers": [2,37], <- AddSuccessPoint affects only “Sexy Topic” and “Private Sex” answer rates.`
3. `"CharaActionsAnswers": [0,1,2,35,], <- AddSuccessPoint affects all “Let’s Talk/Chat” actions and “Public Sex” answer rates.`

You can add as many actions as you want, just make sure the last value in the square brackets ([]) don’t end with a comma (,)
```cs
"CharaActionsAnswers": [0,1,2,3,], <- BAD!
"CharaActionsAnswers": [0,1,2,3], <- Correct!
```

#### ReduceActionCommandRate:

The ReduceActionCommandRate is used if you want to reduce an action from happening.

- Acceptable values: int (Whole number not decimals) and int (Whole number not decimals).

The way we set this is as follow:
"ReduceActionCommandRate": {"ActionID": ReducedByValue}

if we want to add more actions, we do it this way:
```cs
"ReduceActionCommandRate": {"0": 10,
 "1": 50,
 "2": 80} <- the last value should not end with a ,
```

Examples:

1. `"ReduceActionCommandRate": {"999": 0} <- default value, No action will be affected.`
2. `"ReduceActionCommandRate": {"0": 30, “1”: 20} <- it will reduce the action chance of “Casual Topics” by 30% and “Romantic Topics” by 20%.`

### ShortMessage:

This is the text that appears during the fortune ADV, not the description\!

- Acceptable values: string (text).

### SpriteAnimFPS:

Animation FPS:  default value is 1\.

- Acceptable values: int (Numbers not decimals).

### SpritesPath:

The path for your custom sprites

- Acceptable values: string (text).

### ScenarioParams:

This field is the most important, this is what set the fortune ADV. Here we only need to edit 2 specific things

#### Args:

- Acceptable values: string (text).

First look for:
```cs
"Command": "OpenFortuneUI",
"Args": ["999","2","1","0.5"]
```
Here we need to change the 999 to the ID of our fortune.

Next, look for:
```cs
"Command": "Text",
"Args": ["",
   `"『本日の占い』\r\n～～～No traslated: This is a template fortune～～",
   `"[Today's Fortune]\r\n～～～This is a template fortune～～～",
   `"『今日的占卜』\r\n～～～No traslated: Mind your actions, a troubled day awaits you～～～",
   `"『本日運勢』\r\n～～～No traslated: Mind your actions, a troubled day awaits you～～～"]
```
The text here is the text that will appear when you open the fortune.
The blue Text is for English while the others are for Japanese and Chinese.
Only edit the text inside the “”

# Set Sprites

You can use custom sprite for your fortune, make sure the sprite is 304x304 otherwise the sprite will look bigger or smaller.

# Actions IDs: {#actions-ids:}
```cs
DailyLifeTalk = 0,
LoveTalk = 1,
EroTalk = 2,
Cheer = 3,
Soothe = 4,
Praise = 5,
Grumble = 6,
Apologize = 7,
SpeakIllOf = 8,
SolveMyWeaknesses = 9,
MealsRecommendations = 10,
StudyRecommendations = 11,
MotionRecommendations = 12,
PartTimeJobRecommendations = 13,
HumanRelationsReflection = 14,
IllicitLoveReflection = 15,
LoveAffairReflection = 16,
GoodThingsAboutYou = 17,
BadRumors = 18,
GetAlong = 19,
WantToGetAlong = 20,
WhatDoYouThinkAboutThatGuy = 21,
DinnerWithYou = 22,
StudyWithYou = 23,
ExerciseWithYou = 24,
HelpMeWithThis = 25,
ComeToMyRoomWithYou = 26,
ShowGoodwill = 27,
AppointmentForAParty = 28,
OfferToTakePartInARelationship = 29,
BrokeItOff = 30,
Caressing = 31,
HugSomeoneClose = 32,
Kiss = 33,
Touch = 34,
H = 35,
ComeHere = 36,
TakingOnesLover = 37,
ComeBackHereLater = 38,
Nothing = 39,
LetsEatPeople = 40,
LetsStudy = 41,
LetsAllExercise = 42,
LetsAllHelp = 43,
AllToBeFriendsWithThatMan = 44,
GatherAround = 45,
LetsAllHaveSex = 46,
Scramble = 47,
Intervention = 48,
StoppingFight = 49,
ScrambleForH = 50,
StoppingH = 51,
LetsHave3P = 52,
KissMe = 53,
GiveYouAHug = 54,
HowsItGoing = 55,
DidYouHaveSexWithThatGuy = 56,
LikeThat = 57,
HaveYouEverHadSex = 58,
DatingAnyone = 59,
TradePossessions = 60,
BadRumorPursuit = 61,
AbductTheSubject = 62,
AbductionOfTheTargetSubject = 63,
GoodMorningKiss = 64,
SurpriseKiss = 65,
BreakingUpWithYou = 66,
GonnaCheatOnYou = 67,
NotTellAnyoneAboutThis = 68,
DontFollowMe = 69,
KidnapperAdvanceNotice = 70,
ThatGuySeemsToLikeYou = 71,
ThatGuyWasConfessing = 72,
TheGuyWasHavingSex = 73,
FollowedByThatPerson = 74,
YoureVeryPopular = 75,
DarknessDailyH = 76,
WantA3P = 77,
TakeAdvantageOfWeakness = 78,
KeepHimAway = 79,
LetMeSeeThat = 80,
LetMeHold = 81,
EliminateWeaknesses = 82,
SoloCooking = 83,
SoloStudy = 84,
SoloTraining = 85,
SoloPartTimeJob = 86,
Consult_Meals = 87,
Consult_Study = 88,
Consult_Exercise = 89,
Consult_PartTimeJob = 90,
Consult_Relations = 91,
Consult_Love = 92,
Consult_H = 93,
LetsAllHomeParty = 94,
ImportantItemExchange = 95,
ComeToMyRoomWithYouHVer = 96,
LetsAllHaveSex_Combination = 97,
```
