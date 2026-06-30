# Aviation Career Tracker v0.1.0

First release of the NOLoader career meta-mod for Nuclear Option.

## Install

1. Requires [NOLoader](https://github.com/Mursisru/NOLoader) in the game folder.
2. Extract this zip to `Nuclear Option\NOLoader\mods\AviationCareer\`.
3. **Close the game** and run PatchTool (see README) to apply IL patches.

## Contents

- `NOLoader.AviationCareer.dll` — mod assembly
- `NOLoader.ModConfig.dll` — shared INI helper
- `mod.json` — NOLoader manifest (15 Cecil patches)
- `mod_config.ini` — default settings

## Highlights

- 20-rank career progression with fullscreen dashboard UI
- 6-tab Flight Log (Faction, Weapons, Flight, Patterns, Black Box, Recorder)
- `.norep` mission recorder + TacView-style 3D replay from debrief
- Reactive telemetry via IL hooks (no Update polling)

Full documentation: https://github.com/Mursisru/NOAviationCareerTracker
