# Sharing is Caring

**Sharing is Caring** is a mod for the game **R.E.P.O.** that allows team-wide effects from upgrade items and health packs. Only the host needs to install it.

## Features

- ✅ Syncs upgrade values across all players — health, mobility, grab strength, and modded upgrades.
- 📉 Shares healing effects from health packs with the whole team.
- ↻ Configurable sync interval (default: 5 seconds).
- ⚙️ Lightweight, host-only operation for multiplayer sessions.
- 🛠️ Fully configurable via BepInEx.

## Installation

1. Download `SharingIsCaring.dll` from the [Releases](https://github.com/kronflux/SharingIsCaringMod/releases).
2. Drop it into your `BepInEx/plugins/` folder.
3. Launch the game as the host — that’s it.

## Build Instructions

To compile the mod yourself:

1. Clone this repository.
2. Create a folder called `References/` in the project root and place the following DLLs from the game:
   - `Assembly-CSharp.dll`
   - `UnityEngine.dll`
   - `UnityEngine.CoreModule.dll`
   - `PhotonUnityNetworking.dll`
   - `PhotonRealtime.dll`
3. Open the solution in **Visual Studio** (targeting **.NET Framework 4.8**).
4. Build the project using the `Release` configuration.

You can also use the provided `build.bat` to compile it automatically if your environment is set up.

## Configuration

After first launch, a config file will be created at:

```
BepInEx/config/FluxTeam.SharingIsCaring.cfg
```

Example:

```ini
[General]
EnableUpgradeSync = true
EnableHealthSync = true
DebugLogging = false

[Upgrade Sync]
SyncHealth = true
SyncMobility = true
SyncGrab = true
SyncModded = true
SyncInterval = 5.0
```

## Known Compatibility

Works with mods that allow players to join mid-game. Will not sync upgrades in the Main Menu, Lobby Menu, Shop, or Arena. Upgrades are synced only in Lobby and active Missions.

## License

MIT — free to use, modify, and share. Please credit the original author.