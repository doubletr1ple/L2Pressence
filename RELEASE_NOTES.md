# L2Presence v1.0.0

The first stable L2Presence release combines the existing Discord Rich Presence integration with an optional, per-window Borderless toggle tested with Lineage II Interlude.

## Highlights

- Detects one or multiple `l2.exe` clients and publishes their character names to Discord.
- Keeps Discord Rich Presence as the default and only always-on feature.
- Adds an optional Borderless hotkey, disabled at startup.
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
