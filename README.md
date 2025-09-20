
## CS2 ExternalView Plugin

ExternalView provides external camera modes (third-person, model view, watch other player, and free camera) for alive players that can be accessed via chat command or console command.

This plugin uses CounterStrikeSharp's built-in `OnServerPreEntityThink`/`OnServerPostEntityThink` events to fix attack and use positions in third-person mode, eliminating the need for a separate C++ metamod plugin.

## Installation

### Standalone

- PREREQUISITES:
	- [CounterStrikeSharp](https://docs.cssharp.dev/index.html)
	- [TNCSSPluginFoundation](https://github.com/fltuna/TNCSSPluginFoundation)
- Download the [latest ExternalView release](https://github.com/spitice/cs2-external-view/releases)
- Copy/move the files to the server's `csgo` directory

> [!WARNING]
> Do not run this standalone version with lupercalia-mg-cs2 plugin!

### With Lupercalia MG Plugin

- Install [Lupercalia MG Plugin](https://github.com/fltuna/lupercalia-mg-cs2)
- Download **HelperOnly** version from the [latest ExternalView release](https://github.com/spitice/cs2-external-view/releases)
- Copy/move the files to the server's `csgo` directory

> [!CAUTION]
> ConVar names are different from the standalone version.


## Test map

I've created a simple map for testing ExternalView.
Feel free to test the plugin in there if you want.

[test_external_view](https://steamcommunity.com/sharedfiles/filedetails/?id=3482813504) on CS2 workshop

```
host_workshop_map 3482813504
```

## Camera modes

ExternalView provides the following camera modes:

- Third-person camera
	- `!tp`
	- `!tp <camera dist>`
	- `!camdist` → resets camera distance
	- `!camdist <camera dist>`
- Shoulder camera (third-person camera + offset)
	- `!tpp` (right-hand)
	- `!tpq` (left-hand)
	- `!camdist`
- Model view / mirror view
	- `!mv`
- Watch other player
	- `!g` (or `!watch`)
	- `!g <search name>`
- Free camera a.k.a., noclip
	- `!fc` (or `!freecam`)

You can disable some camera modes that might be unfair in PvP game modes:

| Mode         | Command                   | `extv_observer_enabled 1` | `extv_observer_enabled 0` |
| ------------ | ------------------------- | :-----------------------: | :-----------------------: |
| Third Person | !tp. !tpp, !tpq, !camdist |             ✅             |             ✅             |
| Model View   | !mv                       |             ✅             |             ✅             |
| Watch        | !g, !watch                |             ✅             |             -             |
| Free Cam     | !fc, !freecam             |             ✅             |             -             |

## ConVars

ExternalView will generate a .cfg file to the following path if it is not created.

```
csgo/cfg/extv/extv.cfg
```

extv.cfg (default):

```
// ===== External View =====

// External view feature is enabled
extv_enabled 1

// The minimum camera distance for third-person camera.
extv_thirdperson_min_distance 50

// The maximum camera distance for third-person camera.
extv_thirdperson_max_distance 200

// The speed of model view camera movement
extv_modelview_speed 160

// The speed of model view camera movement while walk button pressed
extv_modelview_alt_speed 40

// The radius from the player the model view camera can fly around
extv_modelview_radius 120

// The speed of free camera movement
extv_freecam_speed 800

// The speed of free camera movement while walk button pressed
extv_freecam_alt_speed 2400

// True if observer views (i.e., freecam and watch) are enabled for non-admin players.
extv_observer_enabled 1

// True if admins can use all features regardless of the flags (e.g., IsObserverViewEnabled)
extv_admin_privileges_enabled 1

// Enable model view camera feature
extv_modelview_enabled 1

// Enable camera obstruction handling via trace for third-person camera
extv_thirdperson_traceblock_enabled 0
```

### Third-person trace-based camera obstruction

Controls whether the third-person camera uses a world trace to prevent clipping into walls and props. When enabled, the camera position is pulled toward the player if an obstruction is detected; when disabled, the camera uses the desired offset without obstruction checks (may clip through geometry, but has lower CPU overhead).

- 1: Enabled (default) — obstruction-aware camera with collision backoff
- 0: Disabled — no trace, camera always uses desired position

Example:

```
// Turn off trace-based obstruction handling
extv_thirdperson_traceblock_enabled 0

// Turn it back on
extv_thirdperson_traceblock_enabled 1
```

### Disabling freecam and watch for PvP mode

If you want to disable **freecam** and **watch** camera for PvP game modes (e.g., Zombie Escape), set the following convar:

```
extv_observer_enabled 0
```

Still, admins who are given `@css/root` permission can use freecam and watch. If you want to try to see if the feature is actually disabled without modifying admin settings, you can temporarily disable admin privileges and treat all players as non-admins:

```
extv_admin_privileges_enabled 0
```

## Acknowledgements

I'd like to acknowledge the ideas and inspirations I've drawn from the following awesome CS2 modding projects. Massive thanks to the authors, and CS2 modding community!

- [ThirdPerson-WIP](https://github.com/grrhn/ThirdPerson-WIP) by BoinK & UgurhanK
	- Third-person camera idea via overriding CameraService.ViewEntity
- [CS2-PlayerModelChanger](https://github.com/samyycX/CS2-PlayerModelChanger) by samyyc
	- The logic to inspect player model by using prop_physics_override
- [TNCSSPluginFoundation](https://github.com/fltuna/TNCSSPluginFoundation/tree/main) by tuna
	- Easier development of CSSharp plugin
- [CounterStrikeSharp](https://docs.cssharp.dev/index.html)
	- OnServerPreEntityThink/OnServerPostEntityThink events for position fixing

## Changelogs

#### v3.1.0 (25-09-14)

- Compiled with latest CS#
- Improved vector calculation
- Add ConVar: `extv_modelview_enabled` 
  - Default: 1 (enabled)
- Add ConVar: `extv_thirdperson_traceblock_enabled`
  - Toggle trace-based camera obstruction for third-person camera
  - Default: 0 (disabled)

#### v3.0.0 (25-08-26)

- **BREAKING CHANGE**: Removed C++ metamod plugin dependency
- Now uses CounterStrikeSharp's built-in OnServerPreEntityThink/OnServerPostEntityThink events
- Simplified installation process - no longer requires Metamod:Source
- Improved reliability and reduced complexity
- Fixed code for latest TNCSSPluginFoundation

#### v2.1.1 (25-08-16)

- (Helper) Rebuild with the latest HL2 SDK

#### v2.1.0 (25-08-13)

- Fix CsApi.GetPlayer sometimes fails to return the player
- Fix player movement lock which is broken as of AnimGraph2 update
- Fix notification message shown on switching camera mode from !fc/!g/!mv to thirdperson
- (Helper) Rebuild with the latest HL2 SDK
- (Helper) Update signatures changed by AnimGraph2 update
- (Helper) Improve plugin initialization by using custom addresses/gameconfig class

#### v2.0.2 (25-05-25)

- Fix some CVars are not being tracked
	- NOTE: Remove your extv/extv.cfg or copy the default extv.cfg in README to retain the missing CVars

#### v2.0.1 (25-05-25)

- Fix typo on CVar name
  - CORRECTED: `extv_admin_privileges_enabled`
  - OLD: `extv_admin_previleges_enabled`

#### v2.0.0 (25-05-25)

- Initial standalone version

#### v1.0.0 (25-03-15)

- Initial version (only exists in lupercalia-mg-core)
