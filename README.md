# L2Presence

[![Build Windows EXE](https://github.com/doubletr1ple/L2Pressence/actions/workflows/build.yml/badge.svg)](https://github.com/doubletr1ple/L2Pressence/actions/workflows/build.yml)

A small Windows tray application that publishes Lineage II character activity to Discord Rich Presence.

## Features

- Runs silently in the Windows system tray with no console window.
- Detects every running `l2.exe` client.
- Uses each Lineage II main window title as the character name.
- Shows one or multiple characters in a single Discord Rich Presence card.
- Shows the `ElmoreLab Erica` server name and session timer.
- Uses the `l2_epilogue` Rich Presence image asset.
- Clears Discord Presence when all Lineage II clients are closed.
- Requires no configuration file: Discord Application ID `1538990029031084183` is built in.

## Discord asset

Upload [`DiscordAssets/l2_epilogue.png`](DiscordAssets/l2_epilogue.png) to the Rich Presence Art Assets section of the Discord application and name it exactly `l2_epilogue`.

## Build

Install the .NET 8 SDK and run:

```bat
build.bat
```

To create the self-contained Windows x64 single-file executable, run:

```bat
publish-win-x64.bat
```

The distributable file will be written to `dist\L2Presence.exe`. Friends only need this EXE; a separate .NET installation or `settings.json` is not required.

Every push to `main` also builds the executable on GitHub Actions. Download the `L2Presence-win-x64` artifact from the completed workflow run.

## Usage

Start Discord Desktop, launch `L2Presence.exe`, then start Lineage II. Right-click the tray icon to see detected characters or exit the application.

