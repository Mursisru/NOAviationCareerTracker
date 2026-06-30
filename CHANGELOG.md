# Changelog

## [0.1.0] - 2026-06-30

### Changed
- Documentation refresh: Developer header, badges, GitHub Alerts, Keywords, gitignore hygiene.


All notable changes to **Aviation Career Tracker (ACT)** are documented here.  
Release tags on GitHub use clean semver only (`0.1.0`). Development builds append `Build DEV…` in `AppVersion.DisplayVersion`.

---

## [0.1.0] — 2026-06-24

First public release. Career dashboard, tactical flight log, binary mission recorder, and TacView-style replay for **Nuclear Option** via **NOLoader**.

### Added

- **Career progression** — 20 ranks with +25% XP curve per level; weighted lifetime K/D; destroyed/lost unit value tracking; favorite aircraft, tactic, and weapon slots.
- **Fullscreen dashboard UI** — career snapshot panel, XP bar, value bars, metric cards, signature loadout column.
- **Flight Log (6 tabs)** — Faction Ledger, Weapons Audit, Flight Performance, Tactical Patterns, Black Box, Recorder/TacView.
- **Reactive telemetry** — Cecil IL postfixes on combat, weapons, gear, pilot state, pause/debrief/briefing hooks; `NetworkTransform::Receive` sampling (no Update polling for game state).
- **`.norep` binary recorder** — delta-encoded frames, async writer thread, session index under `Data/Recordings/`.
- **TacView player** — isolated orthographic replay scene, ribbon tracks, timeline 1×–8×, launch from debrief.
- **UI contexts** — Main Menu entry button, Briefing overlay, Pause overlay, Debrief/Leaderboard with optional 3D reconstruction launch.
- **Unit economy catalog** — embedded JSON database for aircraft, ground, naval, and static unit values.
- **Pattern detection** — 45-second ring buffer; Stratosphere Ghost, NOE Terrain Masking, BVR Intercept, SEAD Hard-Lock Duel, close-call ejection detection.
- **XP engines** — combo multipliers, penalty engine, weapons intercept XP table.
- **Deploy script** — `scripts/deploy-mod.ps1` (DEV_SDK build, mod folder copy, optional PatchTool).

### Fixed (development cycle)

- Mod failed to load with placeholder patch hashes; real `expectedSignatureHash` values in `mod.json`.
- Main menu button opened Workshop / Singleplayer due to cloned UnityEvent listeners — standalone buttons via `ACTGameUiBridge`.
- UI compressed into sidebar column — fullscreen overlay canvas (`ACTCanvasHost`) parented to game root canvas.
- Radial360 fill artifacts on dashboard rank badge — square badge with horizontal progress bar and `RectMask2D`.
- Faction tab duplicated stats (cards + bar list) — cards only.
- Typography, nav rail alignment, metric card padding, flight bar contrast.

### Requirements

- Nuclear Option (Steam), matching `Assembly-CSharp.dll` for patch hashes in `mod.json`.
- [NOLoader](https://github.com/Mursisru/NOLoader) installed in game root.
- PatchTool run once after install or game update.

---

[0.1.0]: https://github.com/Mursisru/NOAviationCareerTracker/releases/tag/v0.1.0
