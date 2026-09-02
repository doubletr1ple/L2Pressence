# L2Presence v1.0.1

This maintenance release fixes Discord Rich Presence detection while Lineage II is the active window and enables the Borderless hotkey by default.

## Highlights

- Detects the real `L2UnrealWWindowsViewportWindow` for every `l2.exe` client, ignoring Discord's overlay window.
- Keeps character detection and custom Discord Rich Presence active while Lineage II is in the foreground.
- Waits for the Discord RPC connection to be ready before publishing and restores Presence after reconnecting.
- Shows the running L2Presence version at the top of the tray menu.
- Enables the configurable Borderless hotkey at startup.
- Uses `Shift + Alt + Middle Mouse` as the default configurable shortcut.
- Toggles only the root window under the cursor.
- Removes the complete Windows frame and restores the exact original styles.
- Refreshes the Lineage II viewport after a style change to prevent white edges or stale frame artifacts.
- Moves all configuration and manual Borderless actions to the tray menu.
- Uses the Discord Rich Presence crest as the tray icon.

## Download

Download `L2Presence.exe` for direct use or `L2Presence-win-x64.zip` as an archive. The application is self-contained for Windows x64 and does not require a separate .NET installation.

`L2Presence.exe.sha256` contains the checksum for verifying the downloaded executable.

## Scope

This release intentionally contains no window move, resize, snapping, action-menu, process injection, memory reading, or gameplay automation functionality.

The executable is not code-signed, so Windows SmartScreen may display a warning on first launch.
