using System;
using System.Collections.Generic;
using NOLoader.AviationCareer.ACT.Core;

namespace NOLoader.AviationCareer.ACT.Hooks
{
    public static class ReplayHookMap
    {
        public readonly struct HookDescriptor
        {
            public string Target { get; }
            public string Inject { get; }
            public string Method { get; }
            public ACTEventKind EventKind { get; }

            public HookDescriptor(string target, string inject, string method, ACTEventKind eventKind)
            {
                Target = target;
                Inject = inject;
                Method = method;
                EventKind = eventKind;
            }
        }

        public static IReadOnlyList<HookDescriptor> All { get; } = new[]
        {
            new HookDescriptor("MissionManager::StartMission", "NOLoader.AviationCareer.Patches::StartMissionPostfix", "Postfix", ACTEventKind.SessionStarted),
            new HookDescriptor("Unit::ReportKilled", "NOLoader.AviationCareer.Patches::ReportKilledPostfix", "Postfix", ACTEventKind.UnitDestroyed),
            new HookDescriptor("Unit::RegisterMissile", "NOLoader.AviationCareer.Patches::RegisterMissilePostfix", "Postfix", ACTEventKind.MissileSpawned),
            new HookDescriptor("Unit::DeregisterMissile", "NOLoader.AviationCareer.Patches::DeregisterMissilePostfix", "Postfix", ACTEventKind.MissileCleared),
            new HookDescriptor("UnitPart::ApplyDamage", "NOLoader.AviationCareer.Patches::ApplyDamagePostfix", "Postfix", ACTEventKind.DamageApplied),
            new HookDescriptor("WeaponStation::Fire", "NOLoader.AviationCareer.Patches::WeaponFirePostfix", "Postfix", ACTEventKind.WeaponFired),
            new HookDescriptor("Unit::SetFiringState", "NOLoader.AviationCareer.Patches::SetFiringStatePostfix", "Postfix", ACTEventKind.ContinuousFire),
            new HookDescriptor("Aircraft::SetGear", "NOLoader.AviationCareer.Patches::SetGearPostfix", "Postfix", ACTEventKind.GearChanged),
            new HookDescriptor("Pilot::SwitchState", "NOLoader.AviationCareer.Patches::PilotSwitchStatePostfix", "Postfix", ACTEventKind.PilotStateChanged),
            new HookDescriptor("Pilot::SwitchStateNew", "NOLoader.AviationCareer.Patches::PilotSwitchStateNewPostfix", "Postfix", ACTEventKind.PilotStateChanged),
            new HookDescriptor("GameplayUI::PauseGame", "NOLoader.AviationCareer.Patches::PauseGamePostfix", "Postfix", ACTEventKind.GamePaused),
            new HookDescriptor("Leaderboard::Awake", "NOLoader.AviationCareer.Patches::LeaderboardAwakePostfix", "Postfix", ACTEventKind.DebriefOpened),
            new HookDescriptor("AircraftSelectionMenu::OnEnable", "NOLoader.AviationCareer.Patches::BriefingStartPostfix", "Postfix", ACTEventKind.BriefingOpened),
        };
    }
}
