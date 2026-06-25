# SVS_Plugins

Plugins for Samabake Scramble.

Originally the plugins here were written by @DarkSoldier27 but since he has decided to move away from modding he shared the source to be put up on a shared repository.
That being said, anyone can add new plugins to this repository.

## Installation

1. Install the latest versions of: 
    - [BepInEx v6 nightly](https://github.com/BepInEx/BepInEx)
    - [BepisPlugins](https://github.com/IllusionMods/BepisPlugins/releases)
2. Download the plugin release you want from the links below. Make sure it's a version for your game.
3. Extract the plugin .zip file to your game folder (where the BepInEx folder and game .exe is).

## If you are a modder

If you'd like to contribute code fixes and improvements: fork this repository, create a new branch, push your changes, and open a new PR.

To build this repository you will need VisualStudio2022+ with the `.NET desktop development` and `Game development for Unity` workloads.
All dependencies are downloaded via nuget on first build of the solution. Check the wiki if you are having issues with build steps failing.
To make a release, remove old `bin\out` folder and rebuild the whole repo in Release configuration.

You can discuss modding on the Koikatsu Discord server in the modding channels. There are also various modding guides linked in the pins of these channels you may want to check out.

## Plugin descriptions and downloads

Make sure you download the version for your game (the first part before _ is the initials of the game, e.g. AC = Aicomi).

If a plugin is listed but it's not a link, then it's either experimental or obsolete. You will need to compile it from source yourself, and you will not get any support.

You can get the latest nightly builds of all plugins from the [CI workflow](https://github.com/IllusionMods/SVS_Plugins/actions/workflows/ci.yaml). Open the latest successful run and download the build from the Artifacts section.

> Note: Descriptions below were AI generated and might have some errors in them.

### SVS_CharaSweat
Adds dynamic sweat effects to characters during H scenes. Characters will start sweating based on their arousal gauge levels, with sweat intensity increasing as the scene progresses. The plugin can be toggled on/off and automatically manages sweat states for both PC and NPC characters.

### SVS_CustomFortune
Allows creation and loading of custom fortune events from JSON files. Supports custom fortune types, rates, favor points, state points, success points, action commands, outfit changes, sprites, and scenario parameters. Enables modders to create entirely new fortune events with custom animations and effects.

> Check [this guide](guides/Custom%20Fortune%20Guide.pdf) for instructions on creating custom fortunes ([original on Google docs](https://docs.google.com/document/d/1MiH7E8uqqVg104-ATxwKAf4fY2G-XhGTPpxXsosanUc/edit?usp=sharing)).

### SVS_CustomGameBalance
A comprehensive gameplay customization plugin that provides extensive control over game mechanics including:
- Auto-play for player character
- Character voice automation
- Stats reduction customization (can be disabled for PC)
- Cheater system enhancement with configurable settings
- Threesome and H action forcing options
- Reaction manager for character interactions
- Action probability and lowest rate controls
- Various game fixes (Dating fix, NPC skip prevention, etc.)
- Fortune success rate fixes
- Night event frequency control
- Breakup forcing options
- BBQ answer type configuration
- Hug love points toggle
- Extensive favorability point modifiers

### SVS_ExpandCharacterRoster
Expands the character limit beyond the default 24 characters up to a maximum of 96 characters. Features an in-game slider to adjust the character limit dynamically. Includes reaction fixes to prevent characters from freezing.

### SVS_FadeController
Allows customization of screen fade transitions between scenes. Users can adjust the fade duration (0.1-1.0 seconds) and change the fade color using RGBA hex values. Provides better visual control over scene transitions.

### SVS_FavorabilityGainController
Provides comprehensive control over how characters build relationships and gain favorability points:
- Shows exact favorability points at the Jizo statue
- Multiple game modes: Normal, FullPoint, Reverse, Random, and Unforeseen
- Individual control over Love, Friend, Distant, and Hate point gains (0-200%)
- Option to remove bonus "first impression" points for slower relationship building
- Sensitivity settings for point calculations
- Option to enable hetero/homo friend point gains across sexual orientations
- Detailed UI showing favorability values and descriptions

### SVS_MapLoader
Enables loading of custom maps and locations into the game with advanced features:
- Custom BGM support for custom maps
- Randomized title screen backgrounds (3D maps only)
- Custom job system with designated NPC interaction areas
- Starting location customization
- Changing room map customization
- Integration with SVS_MoreOutfits plugin for outfit-specific map functionality

### SVS_MoreOutfits
Expands the outfit system beyond the default 7 outfits up to 17 slots. Features include:
- Automatic outfit changes for PC (optional)
- Weekend-specific outfits (Saturday/Sunday)
- Night outfit system with configurable timing
- Lewd outfit activation with horny fortunes
- Custom costume system for specific days/periods
- Sports outfit scheduling
- Additional outfit slots (Bath, Camping, Home, Outfits 12-17)
- Day and time-based outfit automation

### SVS_NPCSubtitles
Displays text subtitles when NPCs talk to each other, replacing sprite animations. Features:
- Multi-language support (Japanese, English, Simplified Chinese, Traditional Chinese)
- Customizable font size (17-72)
- Adjustable text color (RGBA)
- Loads custom subtitle files from UserData\sub\SVS_Subtitles_Female_NPC.json
- Improves understanding of NPC conversations and interactions

### SVS_PersonalityLoader
Loads custom personality types beyond the default game personalities. Allows characters with personality IDs above 15 (and other custom ranges) to properly function in talk events and ADV scenes. Essential for using custom personality mods and expanding character variety.

> Modders: Check [this guide](guides/SVS%20Custom%20Personality%20guide/SVS%20Custom%20Personality%20guide.md) for instructions on creating custom personalities.

### SVS_SixthSense
Provides player awareness of hidden NPC activities happening around the game world:
- Sex sensor: Alerts when NPCs are having sex and their location
- Blackmail sensor: Notifies when blackmail events occur
- Masturbation sensor: Shows when and where NPCs are masturbating
- Fight sensor: Alerts to NPC conflicts and locations
- Mood sensor: Displays NPC mood when selecting them (based on relationship level)
- Ambiguity mode to hide character names
- Customizable text timer (5-60 seconds)
- Adjustable text colors (text and underlay)

### SVS_TraitUnlocker
Makes it possible to choose more than the default 2 traits for characters.
