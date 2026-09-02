<div align="center">
  <img src="DiscordAssets/l2_epilogue.png" width="220" alt="L2Presence crest">
  <h1>L2Presence</h1>
  <p>Discord Rich Presence and a per-window borderless toggle for Lineage II.</p>
</div>

[![Build Windows EXE](https://github.com/doubletr1ple/L2Pressence/actions/workflows/build.yml/badge.svg)](https://github.com/doubletr1ple/L2Pressence/actions/workflows/build.yml)
[![Latest Release](https://img.shields.io/github/v/release/doubletr1ple/L2Pressence?label=release)](https://github.com/doubletr1ple/L2Pressence/releases/latest)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Windows x64](https://img.shields.io/badge/Windows-x64-0078D4)](https://www.microsoft.com/windows)

L2Presence is a small Windows tray application built for the Lineage II client. It detects running characters from `l2.exe` window titles and publishes them to Discord Rich Presence. An optional global hotkey can toggle borderless mode for one window at a time.

## Download

[Download the latest L2Presence.exe](https://github.com/doubletr1ple/L2Pressence/releases/latest/download/L2Presence.exe)

The release is a self-contained Windows x64 executable. No installer or separate .NET installation is required. A ZIP archive and a SHA-256 checksum are provided on the [Latest Release](https://github.com/doubletr1ple/L2Pressence/releases/latest) page as well.

## Features

- Runs quietly in the Windows notification area.
- Detects every running `l2.exe` client.
- Reads character names from the actual Lineage II viewport windows, ignoring overlays.
- Displays one or multiple characters in a single Discord activity.
- Shows the configured server name and current session duration.
- Clears Discord activity after the final Lineage II client closes.
- Uses the Lineage II crest for Discord Rich Presence and the tray icon.
- Removes the complete Windows frame independently for the window under the cursor.
- Remembers each changed window's original style and can restore it.
- Supports a configurable modifier-key and mouse-button combination.
- Ships as a self-contained Windows x64 executable.

L2Presence does not move or resize windows. It does not inject code into the game process.

## Requirements

- Windows 10 or Windows 11, x64.
- Discord Desktop running on the same Windows session.
- A Lineage II client whose process is named `l2.exe`.
- The .NET SDK is only required when building from source. The published EXE is self-contained.

## Quick Start

1. Download [`L2Presence.exe`](https://github.com/doubletr1ple/L2Pressence/releases/latest/download/L2Presence.exe), or build it locally.
2. Start Discord Desktop.
3. Run `L2Presence.exe`.
4. Start one or more Lineage II clients.
5. Right-click the tray icon to view detected characters or configure Borderless.

No configuration file is required for Discord Rich Presence. The Discord application ID and default server information are included in the application.

## Discord Rich Presence

L2Presence checks for `l2.exe` every two seconds. It identifies the visible `L2UnrealWWindowsViewportWindow` for each client so Discord and other overlays cannot replace the character-title source. The application publishes:

- `Character: Name` for one detected client;
- `Characters: Name1, Name2` for multiple clients;
- `ElmoreLab Erica` as the default server;
- a session timer that starts when the first character is detected.

When every Lineage II client closes, the activity is removed and the timer is reset.

## Borderless Toggle

Borderless control and its global hotkey are enabled when L2Presence starts. To use it:

1. Right-click the tray icon.
2. Open `Borderless`.
3. Confirm `Borderless hotkey: on` is displayed.
4. Point at the target window and press `Shift + Alt + Middle Mouse`.

The default shortcut affects only the root window under the cursor. It removes the title bar, resize frame, and Windows edge styles. Press it again to restore that window's exact original styles.

Use `Borderless > Configure hotkey...` to choose another modifier combination and mouse button. The shortcut is saved to:

```text
%LocalAppData%\L2Presence\borderless-settings.json
```

The tray menu also provides:

| Command | Behavior |
| --- | --- |
| `L2Presence vX.Y.Z` | Shows the version of the currently running build. |
| `Enable/Disable borderless hotkey` | Installs or removes the global input hooks. |
| `Configure hotkey...` | Changes the borderless shortcut. |
| `Toggle borderless for active window` | Toggles the window that was active before the tray menu opened. |
| `Restore all window borders` | Restores every window changed during the current session. |

L2Presence restores tracked window styles when it exits normally.

## Configuration From Source

Application defaults are defined in [`AppSettings.cs`](AppSettings.cs):

| Setting | Default | Purpose |
| --- | --- | --- |
| `DiscordApplicationId` | `1538990029031084183` | Discord Rich Presence application. |
| `ProcessName` | `l2` | Process name to detect without `.exe`. |
| `ServerName` | `ElmoreLab Erica` | Text displayed in Discord. |
| `PollIntervalSeconds` | `2` | Lineage II detection interval. |
| `LargeImageKey` | `l2_epilogue` | Discord Art Asset key. |
| `CharacterNamePrefixToRemove` | empty | Optional title prefix removal. |
| `CharacterNameSuffixToRemove` | empty | Optional title suffix removal. |

Forks using another Discord application must upload [`DiscordAssets/l2_epilogue.png`](DiscordAssets/l2_epilogue.png) to their Discord Rich Presence Art Assets and use the key `l2_epilogue`, or update the corresponding settings.

## Build From Source

Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), clone the repository, and run:

```powershell
dotnet restore
dotnet build -c Release
```

The standard build is written under:

```text
bin\Release\net8.0-windows\win-x64\
```

To create the self-contained single-file executable:

```powershell
dotnet publish L2Presence.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o dist
```

Windows command scripts are also included:

```text
build.bat
publish-win-x64.bat
run.bat
```

Every push and pull request targeting `main` runs the Windows publish workflow and uploads `L2Presence-win-x64` as a build artifact. Pushing a semantic version tag such as `v1.0.0` creates a GitHub Release containing:

- `L2Presence.exe` for direct use;
- `L2Presence-win-x64.zip` for archive download;
- `L2Presence.exe.sha256` for integrity verification.

## Project Structure

```text
L2Presence
|-- DiscordPresenceService.cs   Discord RPC lifecycle and activity updates
|-- L2WindowDetector.cs         l2.exe and character-title detection
|-- TrayApplicationContext.cs   tray menu and application lifetime
|-- TrayIconFactory.cs          embedded crest to Windows tray icon
|-- AltSnapModule/
|   |-- Actions/                per-HWND borderless style storage
|   |-- Input/                  low-level keyboard and mouse hooks
|   |-- Interop/                focused Win32 helpers
|   `-- Settings/               saved shortcut and settings dialog
|-- DiscordAssets/              Discord and tray artwork
|-- RELEASE_NOTES.md            current GitHub Release notes
`-- .github/workflows/          CI and tagged release workflows
```

The `AltSnapModule` directory name is retained for source continuity; it contains only the borderless feature. There is no move, resize, snapping, or action-menu implementation.

## Troubleshooting

### Discord activity does not appear

- Confirm Discord Desktop is running before L2Presence.
- Confirm the game process is named `l2.exe`.
- Check that the Lineage II window title contains the expected character name.
- Restart L2Presence after restarting Discord.
- Run L2Presence and Lineage II at the same Windows integrity level.

### Discord shows its built-in Lineage II activity

Discord's automatic game detection is independent from custom Rich Presence and can take display priority while Lineage II is in the foreground. Open `User Settings > Activity Settings > Registered Games` in Discord and disable activity sharing for Lineage II only. Keep the global activity-sharing setting enabled so L2Presence can continue publishing its custom Rich Presence.

L2Presence reads the dedicated Lineage II viewport instead of relying on `Process.MainWindowHandle`, which can point to Discord's overlay while the game is active.

### The borderless shortcut does nothing

- Confirm `Borderless hotkey: on` is shown in the tray menu.
- Keep the cursor over the intended game window when pressing the shortcut.
- Check that another global-hotkey utility is not using the same combination.
- Try `Toggle borderless for active window` from the tray menu.

### Restore a window frame

Press the borderless shortcut again over that window, or select `Restore all window borders` from the tray menu.

## Privacy and Security

L2Presence runs locally. It reads process names and top-level window titles, then sends Rich Presence updates through Discord's local IPC connection. Global keyboard and mouse hooks are installed only while the optional borderless hotkey is enabled. The application does not inject DLLs, read game memory, automate gameplay, or send input to Lineage II.

The release executable is not code-signed. Windows SmartScreen may therefore show a warning on first launch; verify the SHA-256 checksum from the GitHub Release before running it.
