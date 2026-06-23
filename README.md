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
To make a release, remove old `bin\out` folder and rebuild the whole repo in Release configuration. Afterwards run release.ps1 to generate the combined zip files.

You can discuss modding on the Koikatsu Discord server in the modding channels. There are also various modding guides linked in the pins of these channels you may want to check out.

## Plugin descriptions and downloads

Make sure you download the version for your game (the first part before _ is the initials of the game, e.g. AC = Aicomi).

If a plugin is listed but it's not a link, then it's either experimental or obsolete. You will need to compile it from source yourself, and you will not get any support.

You can get the latest nightly builds of all plugins from the [CI workflow](https://github.com/IllusionMods/SVS_Plugins/actions/workflows/ci.yaml). Open the latest successful run and download the build from the Artifacts section.
