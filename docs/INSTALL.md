# Player installation

Install **Aviation Career Tracker** into a Nuclear Option game that already has **NOLoader**.

## Prerequisites

1. [Nuclear Option](https://store.steampowered.com/app/2168680/Nuclear_Option/) (Steam).
2. [NOLoader](https://github.com/Mursisru/NOLoader) — follow [NOLoader INSTALL.md](https://github.com/Mursisru/NOLoader/blob/master/docs/INSTALL.md).

## Install the mod

### Option A — GitHub Release (recommended)

1. Download **`AviationCareer-v0.1.0.zip`** from [Releases](https://github.com/Mursisru/NOAviationCareerTracker/releases).
2. Extract into:

   ```text
   <Nuclear Option>\NOLoader\mods\AviationCareer\
   ```

   You should have:

   ```text
   AviationCareer\
     NOLoader.AviationCareer.dll
     NOLoader.ModConfig.dll
     mod.json
     mod_config.ini
   ```

### Option B — Build from source

See [Developer guide](../README.md#developer-guide) in the root README.

## Apply IL patches

**Close the game first.**

Run PatchTool from your NOLoader clone (or use `scripts/deploy-mod.ps1` from a dev tree):

```powershell
dotnet run --project path\to\NOLoader.PatchTool\NOLoader.PatchTool.csproj -c DEV_SDK -- "C:\Program Files (x86)\Steam\steamapps\common\Nuclear Option"
```

PatchTool injects the Cecil postfixes listed in `mod.json` into `Assembly-CSharp.dll`. Without this step, hooks and UI will not activate.

## Verify

1. Launch Nuclear Option.
2. Main menu → **Aviation Career** (below Workshop).
3. Full-screen dashboard opens; Flight Log tabs on the left.

## Data folders

Created at runtime under the mod root:

| Path | Purpose |
|------|---------|
| `Data/profile.json` | Persistent career profile (XP, ranks, favorites) |
| `Data/Recordings/*.norep` | Mission replay files |
| `mod_config.ini` | User settings (created on first load if missing) |

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| Mod not in NOLoader list | Check `mod.json` and DLL names; read `NOLoader/logs/proxy.log` |
| Button missing on main menu | Re-run PatchTool; confirm `MainMenu::Start` hash matches your game build |
| Button opens Workshop | Update to latest ACT build (standalone button fix) |
| UI squashed / overlapping | Update to v0.1.0+ (fullscreen canvas fix) |
| Hooks silent after game update | Recompute patch hashes with PatchTool and update `mod.json` |

## Uninstall

1. Delete `NOLoader\mods\AviationCareer\`.
2. Restore vanilla `Assembly-CSharp.dll` from Steam “Verify integrity” or a backup if you need to remove IL patches.
