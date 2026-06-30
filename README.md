**Developer:** Mursisru

# Aviation Career Tracker (ACT)

[![Nuclear Option](https://img.shields.io/badge/Game-Nuclear%20Option-blue)](https://store.steampowered.com/app/2168680/Nuclear_Option/)
[![NOLoader](https://img.shields.io/badge/Loader-NOLoader-purple)](https://github.com/Mursisru/NOLoader)
[![Version](https://img.shields.io/github/v/release/Mursisru/NOAviationCareerTracker?label=Version&color=green)](https://github.com/Mursisru/NOAviationCareerTracker/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow)](https://github.com/Mursisru/NOAviationCareerTracker/blob/main/LICENSE)
[![.NET Framework 4.8](https://img.shields.io/badge/.NET%20Framework-4.8-512BD4)](https://dotnet.microsoft.com/download/dotnet-framework/net48)

**Aviation Career Tracker** is a NOLoader meta-mod for the flight sim [**Nuclear Option**](https://store.steampowered.com/app/2168680/Nuclear_Option/). It adds a persistent career layer: rank progression, tactical flight analytics, a graphical flight log, binary mission recordings (`.norep`), and a TacView-style 3D replay — without polling the game loop for state.

**Mod id:** `com.at747.aviationcareer`  
**Load stage:** `MainMenu`  
**Current release:** `0.1.0`

---

## Critical warnings

> [!IMPORTANT]
> **NOLoader required** - install [NOLoader](https://github.com/Mursisru/NOLoader/releases) before this mod.

> [!IMPORTANT]
> **Close Nuclear Option before PatchTool** - managed DLLs must be unlocked.

> [!IMPORTANT]
> **Run PatchTool after install or game update** - without IL patches (15 Cecil postfixes in `mod.json`), UI and hooks silently fail.

> [!WARNING]
> **Placeholder or stale `expectedSignatureHash` prevents load** - verify real 16-hex hashes match your game build.

> [!NOTE]
> **`loadStage: MainMenu`** - career UI mounts from main menu; data in `Data/profile.json` and `Data/Recordings/*.norep`.

## Table of contents

- [Critical warnings](#critical-warnings)
- [Features](#features)
- [Screenshots](#screenshots)
- [Requirements](#requirements)
- [Player installation](#player-installation)
- [Configuration](#configuration)
- [Flight Log tabs](#flight-log-tabs)
- [Career & progression](#career--progression)
- [Developer guide](#developer-guide)
- [Project layout](#project-layout)
- [Troubleshooting](#troubleshooting)
- [Documentation](#documentation)
- [License](#license)

---

## Features

### Career dashboard

- **20 military ranks** — Cadet through Aviation Immortal (+25% XP requirement per tier).
- **Career snapshot** — rank badge, XP bar, destroyed/lost value bars, K/D, total XP, net economy value.
- **Signature loadout** — favorite aircraft, tactic, and weapon tracked across missions.

### Flight Log (6 tabs)

| Tab | Tracks |
|-----|--------|
| **Faction Ledger** | Kills, friendly fire, collateral, Guardian Angel intercepts |
| **Weapons Audit** | Intercept XP, primary weapon recommendation, fired-weapon bars |
| **Flight Performance** | Peak G, belly slide distance, glide distance, storm/fog time aloft |
| **Tactical Patterns** | Ring-buffer pattern detection (NOE, BVR, SEAD, ghost altitude, …) |
| **Black Box** | Close-call ejections, telemetry ring-buffer summary |
| **Recorder / TacView** | Live/standby recorder state, saved `.norep` count, debrief launch |

### Recording & replay

- **`.norep` binary format** — compact delta frames, background writer thread, indexed under `Data/Recordings/`.
- **TacView player** — orthographic replay scene, entity ribbons, timeline 1×–8×, launched from debrief.

### UI injection

Fullscreen overlay (not the vanilla menu column) in:

- **Main Menu** — “Aviation Career” entry after Workshop
- **Briefing** — `AircraftSelectionMenu`
- **Pause** — in-mission pause overlay
- **Debrief** — leaderboard / post-mission with optional **Launch 3D Reconstruction**

### Zero-overhead telemetry

Combat, weapons, gear, pilot state, and pause/debrief hooks use **Cecil IL postfixes**. Aircraft/missile kinematics are sampled from `NetworkTransform::Receive` — no `Update()` polling for game state.

---

## Screenshots

Open the in-game **Aviation Career** panel from the main menu to see the dashboard and tabbed flight log. UI uses a dark military-tech theme with cyan accents and anchor-based layout (no layout-group stretch artifacts).

---

## Requirements

| Component | Notes |
|-----------|-------|
| [Nuclear Option](https://store.steampowered.com/app/2168680/Nuclear_Option/) | Steam build matching `mod.json` patch hashes |
| [NOLoader](https://github.com/Mursisru/NOLoader) | [Install guide](https://github.com/Mursisru/NOLoader/blob/master/docs/INSTALL.md) |
| PatchTool | Run once after install or game update |
| **Developers:** .NET Framework 4.8 SDK | Plus sibling clone `NOLoader_Engine` |

---

## Player installation

> [!IMPORTANT]
> **NOLoader required** - install [NOLoader](https://github.com/Mursisru/NOLoader/releases) before this mod.

See **[docs/INSTALL.md](docs/INSTALL.md)** for step-by-step instructions.

**Quick path:**

1. Install NOLoader into the game folder.
2. Copy release zip contents to `Nuclear Option\NOLoader\mods\AviationCareer\`.
3. Close the game and run **PatchTool** (or dev `deploy-mod.ps1`).
4. Launch → Main menu → **Aviation Career**.

---

## Configuration

File: `NOLoader\mods\AviationCareer\mod_config.ini`

```ini
[AviationCareer]
Enabled=true
XpOverlayThreshold=250
AutoRecord=true
RecordFlushIntervalSec=2
TacViewDefaultSpeed=1
```

| Key | Default | Description |
|-----|---------|-------------|
| `Enabled` | `true` | Master switch |
| `XpOverlayThreshold` | `250` | Minimum XP delta before overlay popup |
| `AutoRecord` | `true` | Auto-start `.norep` on mission |
| `RecordFlushIntervalSec` | `2` | Writer thread flush interval (seconds) |
| `TacViewDefaultSpeed` | `1` | Default TacView playback multiplier |

---

## Flight Log tabs

Session-scoped analytics reset each mission unless noted. Persistent values (XP, rank, lifetime K/D) live in `Data/profile.json`.

- **Faction Ledger** — combat ethics counters and session summary cards.
- **Weapons Audit** — weapon fired counts, intercept XP from `WeaponsXpTable`, primary weapon recommendation.
- **Flight Performance** — G-load, belly slide, glide, weather exposure from telemetry stream.
- **Tactical Patterns** — detected maneuvers from 45 s ring buffer; awards combo XP multipliers.
- **Black Box** — close-call ejection window (≤ 2 s before aircraft loss, pilot survived).
- **Recorder** — `.norep` path, recording state, replay index; debrief shows launch button.

---

## Career & progression

- **XP** — awarded from kills, intercepts, patterns, and penalties (friendly fire, collateral).
- **Unit values** — embedded `UnitEconomyCatalog.json` (aircraft, ground, naval, static).
- **Weighted K/D** — lifetime ratio stored on profile.
- **Ranks** — 20 titles; XP curve uses base 1000 to rank 2 with ×1.25 growth.

Rank titles: Cadet, Ensign, Lieutenant, Captain, Major, Colonel, Brigadier, General, Ace, Double Ace, Strategist, Tactician, Commander, Marshal, Supreme Ace, Ghost Pilot, Legend, Myth, Demigod, Aviation Immortal.

---

## Developer guide

### Repository layout

Clone next to NOLoader:

```text
source\repos\
  NOLoader_Engine\
  NOAviationCareerTracker\    ← this repo
```

### Build

```powershell
cd C:\Users\at747\source\repos\NOAviationCareerTracker
dotnet build NOLoader.AviationCareer\NOLoader.AviationCareer.csproj -c DEV_SDK
```

Output: `NOLoader.AviationCareer\bin\DEV_SDK\net48\NOLoader.AviationCareer.dll`

Optional: copy `Directory.Build.props.example` → `Directory.Build.user.props` and set `NuclearOptionRoot` if your game path differs.

### Deploy to game (developers)

**Close Nuclear Option first** (PatchTool patches `Managed\Assembly-CSharp.dll`).

```powershell
.\scripts\deploy-mod.ps1                  # DEV_SDK build + PatchTool
.\scripts\deploy-mod.ps1 -SkipPatchTool   # copy DLL only (game already patched)
```

Target: `<Game>\NOLoader\mods\AviationCareer\`

### Patch hashes

Fifteen Cecil postfixes are declared in `mod.json`. After a game update:

```powershell
dotnet run --project ..\NOLoader_Engine\src\NOLoader.PatchTool\NOLoader.PatchTool.csproj -c DEV_SDK -- hash-patch MissionManager::StartMission
```

Update `expectedSignatureHash` in `mod.json`, redeploy, re-run PatchTool.

### Versioning

| Location | Format |
|----------|--------|
| GitHub release tag | `0.1.0` (semver only) |
| `AppVersion.Semver` | `0.1.0` |
| `AppVersion.DisplayVersion` | `0.1.0 Build DEV{n}P{m}…` (dev channel) |
| `mod.json` `version` | `0.1.0` |

---

## Project layout

```text
NOAviationCareerTracker/
├── NOLoader.AviationCareer/     # Mod source
│   ├── ACT.Core/                # Events, progression, analytics, profile
│   ├── ACT.Recording/           # .norep writer/reader, TacView
│   ├── ACT.UI/                  # Shell, dashboard, tabs, contexts
│   ├── Patches.cs               # IL postfix bodies
│   ├── mod.json                 # NOLoader manifest + patch list
│   └── mod_config.ini           # Default config template
├── scripts/deploy-mod.ps1
├── deploy/NOLoader/mods/AviationCareer/   # Staging (json/ini; DLLs local)
├── docs/INSTALL.md
├── docs/ARCHITECTURE.md
├── CHANGELOG.md
└── README.md
```

---

## Troubleshooting

| Issue | Action |
|-------|--------|
| Mod does not load | Check `NOLoader/logs/proxy.log`; verify DLL + `mod.json` in mod folder |
| No UI / hooks | Run PatchTool; verify hashes match game build |
| Entry button opens Workshop | Upgrade to ≥ 0.1.0 (standalone button fix) |
| Cyan geometric UI glitches | Upgrade to ≥ 0.1.0 (rank badge no longer uses Radial360 on stretched rects) |
| PatchTool IO error | Close the game completely before deploy |

Logs: `NOLoader/logs/proxy.log`, `NOLoader/logs/noloader_ring.log` (DEV builds).

---

## Documentation

- [Player installation](docs/INSTALL.md)
- [Architecture & patch map](docs/ARCHITECTURE.md)
- [Changelog](CHANGELOG.md)

---

## License

[MIT](LICENSE) — Copyright (c) 2026 at747 / Mursisru

---

## Links

- [NOLoader](https://github.com/Mursisru/NOLoader)
- [Nuclear Option on Steam](https://store.steampowered.com/app/2168680/Nuclear_Option/)
- [Releases](https://github.com/Mursisru/NOAviationCareerTracker/releases)

---

## Keywords

nuclear-option, noloader, mod, noaviationcareertracker, csharp, unity
