# Architecture

High-level design of **Aviation Career Tracker (ACT)** as a NOLoader meta-mod.

## Layer map

```text
AviationCareerMod (INOMod, INOModTickSlow)
├── ACT.Core
│   ├── ACTBootstrap / ACTEventBus / ACTSessionRuntime
│   ├── ACTProfileStore / ACTConfigCache
│   ├── Progression (XPEngine, RankSystem, PenaltyEngine, ComboMultiplierEngine)
│   ├── Data (UnitEconomyDatabase + embedded catalog JSON)
│   └── Analytics (6 session services + RingBuffer45s)
├── ACT.Recording
│   ├── ACTNorepWriter / ACTNorepReader / ACTDeltaBuffer
│   ├── NorepFormat (.norep binary schema)
│   └── TacView (ACTTacViewPlayer)
├── ACT.UI
│   ├── ACTCanvasHost / ACTShellController / ACTDashboardView
│   ├── ACTTabVisualRenderer / ACTFlightLogUI
│   ├── Contexts (MainMenu, Briefing, Pause, Leaderboard hosts)
│   └── ACTGameUiBridge (standalone menu buttons)
└── Patches.cs (Cecil inject targets → ACTEventBus)
```

## Event flow

1. **Game method postfix** (`Patches.cs`) publishes `ACTEvent` to `ACTEventBus`.
2. **Session services** (`IACTSessionService`) consume events; reset on `SessionStarted`.
3. **XPEngine / PenaltyEngine** update persistent `ACTProfile` via `ACTProfileStore`.
4. **ACTNorepWriter** records deltas on a background thread when recording is active.
5. **UI** reads profile + services on tab select; no per-frame game object scans.

## IL patches (15)

| Target | Purpose |
|--------|---------|
| `MissionManager::StartMission` | Session start, weather id, recorder arm |
| `Unit::ReportKilled` | Kill ledger, XP, economy value |
| `Unit::RegisterMissile` / `DeregisterMissile` | Missile lifecycle |
| `UnitPart::ApplyDamage` | Damage / friendly fire hints |
| `WeaponStation::Fire` | Weapons audit |
| `Unit::SetFiringState` | Continuous fire |
| `Aircraft::SetGear` | Belly slide / gear telemetry |
| `Pilot::SwitchState` / `SwitchStateNew` | Ejection / pilot state |
| `GameplayUI::PauseGame` | Pause overlay host |
| `Leaderboard::Awake` | Debrief overlay host |
| `AircraftSelectionMenu::OnEnable` | Briefing overlay host |
| `MainMenu::Start` | Main menu entry + fallback UI mount |
| `AircraftNetworkTransform::Receive` | Aircraft telemetry sampling |
| `MissileNetworkTransform::Receive` | Missile telemetry sampling |

Hashes are stored in `mod.json` as `expectedSignatureHash` (16 hex chars). They must match the installed game build.

## UI layout

```text
┌─────────────────────────────────────────────────────────────┐
│ Top bar — title · ESC hint · close                          │
├──────────┬──────────────────────────────────────────────────┤
│ Nav rail │ Career Snapshot (dashboard)                      │
│ 6 tabs   ├──────────────────────────────────────────────────┤
│          │ Tab content (Combat Ledger, Flight, etc.)          │
│          │ [optional: Launch 3D Reconstruction on debrief]  │
└──────────┴──────────────────────────────────────────────────┘
```

- Root canvas: `ACTCanvasHost` — fullscreen overlay, `sortingOrder` 9000, parented to game root canvas (not the menu sidebar).
- Buttons: **never** `Instantiate` game menu buttons for click handlers (preserves Workshop/Missions listeners).

## `.norep` format (v1)

| Field | Value |
|-------|-------|
| Magic | `NOREP\0` |
| Footer magic | `PERN` |
| Version | `0x0001` |
| Frame types | transform delta, input edge, combat event, spawn/despawn, batch transform, … |

Writer flushes on a timer (`RecordFlushIntervalSec`). Reader powers TacView playback.

## Rank progression

- **20 ranks** — Cadet → Aviation Immortal.
- Base **1000 XP** to rank 2, **×1.25** growth per subsequent rank tier.
- BepInEx `[BepInPlugin]` semver must stay `0.1.0`; display string may include `Build DEV…`.

## Extension points

- Add tab: implement `IACTTab` in `ACT.UI.Tabs`, register in `ACTFlightLogUI`, add branch in `ACTTabVisualRenderer`.
- Add hook: postfix in `Patches.cs`, descriptor in `ReplayHookMap.cs`, entry in `mod.json`, re-run PatchTool.
- Add pattern: extend `TacticalPatternService.TryMatchPatterns` with ring-buffer queries.
