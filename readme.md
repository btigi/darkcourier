Introduction
============
Dark Courier is a mod installation tool for Black Geyser: Couriers of Darkness, developed by GrapeOcean Technologies and published by V Publishing.

Dark Courier is currently in an alpha preview, with limited features and support, and only runs on Windows, however features include:
- Unpacking bgd.data
- Reading mod config files
- Mod config file driven JSON patching of resources
- Uninstalling changes
- Packing bgd.data

No validation on the changes is made.

Download
========
Compiled downloads are not available during this early development phase.

Compiling
=========
To clone and run this application, you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/darkcourier

# Go into the repository
$ cd darkcourier

# Build  the app
$ dotnet build
```

Note: This repository temporarily contains code for ii.SimpleZip and JsonPatch.Dynamic.NetStandard, pending their availability as nuget packages.

Usage
=====
Dark Courier is a command-line application, and accepts command lines as below:

Unpack all files from bgd.data

```dc extractall path_to_bgd.data unpacked_directory```

Install a mod file into an unpacked directory

```dc install unpacked_directory path_to_mod_file.json```

Uninstall a mod file from an unpacked directory

```dc uninstall unpacked_directory path_to_mod_file.json```

Pack all files into bgd.data

```dc create path_to_bgd.data unpacked_directory```

Roadmap
=======
dc: Improve move uninstallation
dc: Add automated 'domino' uninstallation
dc: GUI version
dc: Cross-platform support?
Mod: Add components
Mod: Allow externalization of JSON Patch values
Mod: Distribute as single archive files
Misc: Central mod archive?

Support Disclaimer
==================
Dark Courier is currently in an alpha preview, bugs are possible, and the format of mods is expected to change.

Mods can introduce bugs, or surface hidden bugs already present in the game. 

Mods may corrupt your game, preventing your from completing your playthrough or breaking your save games.

Not all mods are compatible, and their usage may introduce logical or actual inconsistencies.

Players with a modded game may not receive support for bug reports from the developers (unless the report is obviously independent from the modded nature of the game).

Purchasing
==========
Black Geyser: Couriers of Darkness is available purchase on Steam https://store.steampowered.com/app/1374930/ and GOG https://www.gog.com/en/game/black_geyser_couriers_of_darkness
For further information check the official game website https://www.blackgeyser.com/

Licencing
=========
Dark Courier is licenced under CC BY-NC-ND 4.0 https://creativecommons.org/licenses/by-nc-nd/4.0/ Full licence details are available in licence.md

