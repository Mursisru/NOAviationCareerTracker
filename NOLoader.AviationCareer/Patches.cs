using System;
using NOLoader.AviationCareer.ACT.Core;
using NOLoader.AviationCareer.ACT.UI.Contexts;
using UnityEngine;

namespace NOLoader.AviationCareer
{
    internal static class Patches
    {
        private static float SessionTime => ACTBootstrap.Session?.IsActive == true
            ? Time.time - ACTBootstrap.Session.SessionStartTime
            : Time.time;

        public static void StartMissionPostfix()
        {
            if (!ACTConfigCache.Instance.Enabled)
                return;
            string mission = MissionManager.CurrentMission != null ? MissionManager.CurrentMission.Name : "Menu";
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.SessionStarted, Time.time, payload: mission));
        }

        public static void ReportKilledPostfix(Unit __instance)
        {
            if (__instance == null || !ACTConfigCache.Instance.Enabled)
                return;
            uint id = (uint)__instance.GetInstanceID();
            string name = __instance.name ?? "unit";
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.UnitDestroyed, SessionTime, id, payload: name));
        }

        public static void RegisterMissilePostfix(Unit __instance, Missile missile)
        {
            if (__instance == null || missile == null || !ACTConfigCache.Instance.Enabled)
                return;
            uint ownerId = (uint)__instance.GetInstanceID();
            uint missileId = (uint)missile.GetInstanceID();
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.MissileSpawned, SessionTime, ownerId, missileId, missile.name));
            if (missile.targetID.IsValid)
                ACTEventBus.Publish(new ACTEvent(ACTEventKind.MissileTargetAssigned, SessionTime, missileId, missile.targetID.Id));
        }

        public static void DeregisterMissilePostfix(Unit __instance, Missile missile)
        {
            if (__instance == null || missile == null || !ACTConfigCache.Instance.Enabled)
                return;
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.MissileCleared, SessionTime, (uint)missile.GetInstanceID(), payload: missile.name));
        }

        public static void ApplyDamagePostfix(UnitPart __instance, float netPierceDamage, float netBlastDamage, float netFireDamage, float netImpactDamage)
        {
            if (__instance == null || !ACTConfigCache.Instance.Enabled)
                return;
            float damage = netPierceDamage + netBlastDamage + netFireDamage + netImpactDamage;
            if (damage <= 0.5f)
                return;
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.DamageApplied, SessionTime, payload: $"dmg={damage:F1}"));
        }

        public static void WeaponFirePostfix(WeaponStation __instance, Unit owner, Unit target)
        {
            if (__instance == null || owner == null || !ACTConfigCache.Instance.Enabled)
                return;
            string weapon = __instance.WeaponInfo != null ? __instance.WeaponInfo.name : "Gun";
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.WeaponFired, SessionTime, (uint)owner.GetInstanceID(), payload: weapon));
        }

        public static void SetFiringStatePostfix(Unit __instance, int index, bool firing)
        {
            if (__instance == null || !ACTConfigCache.Instance.Enabled)
                return;
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.ContinuousFire, SessionTime, (uint)__instance.GetInstanceID(), payload: firing ? "START" : "STOP"));
        }

        public static void SetGearPostfix(Aircraft __instance, bool deployed)
        {
            if (__instance == null || !ACTConfigCache.Instance.Enabled)
                return;
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.GearChanged, SessionTime, (uint)__instance.GetInstanceID(), payload: deployed ? "GEAR DOWN" : "GEAR UP"));
        }

        public static void PilotSwitchStatePostfix(Pilot __instance)
        {
            PublishPilotState(__instance);
        }

        public static void PilotSwitchStateNewPostfix(Pilot __instance)
        {
            PublishPilotState(__instance);
        }

        private static void PublishPilotState(Pilot pilot)
        {
            if (pilot == null || !ACTConfigCache.Instance.Enabled)
                return;
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.PilotStateChanged, SessionTime, (uint)pilot.GetInstanceID(), payload: pilot.GetType().Name));
        }

        public static void PauseGamePostfix(GameplayUI __instance)
        {
            if (__instance == null || !ACTConfigCache.Instance.Enabled)
                return;
            ACTContextHosts.MountPause(__instance);
        }

        public static void LeaderboardAwakePostfix(Leaderboard __instance)
        {
            if (__instance == null || !ACTConfigCache.Instance.Enabled)
                return;
            ACTContextHosts.MountDebrief(__instance);
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.SessionEnded, SessionTime));
        }

        public static void BriefingStartPostfix(AircraftSelectionMenu __instance)
        {
            if (__instance == null || !ACTConfigCache.Instance.Enabled)
                return;
            ACTContextHosts.MountBriefing(__instance);
        }

        public static void MainMenuStartPostfix(MainMenu __instance)
        {
            if (__instance == null || !ACTConfigCache.Instance.Enabled)
                return;
            ACTContextHosts.MountMainMenu(__instance);
        }

        public static void AircraftNetworkTransformReceivePostfix(NuclearOption.NetworkTransforms.AircraftNetworkTransform __instance, double timestamp, Mirage.Serialization.NetworkReader reader)
        {
            PublishTransform(__instance, ACTEventKind.EntityTransformDelta);
        }

        public static void MissileNetworkTransformReceivePostfix(NuclearOption.NetworkTransforms.MissileNetworkTransform __instance, double timestamp, Mirage.Serialization.NetworkReader reader)
        {
            PublishTransform(__instance, ACTEventKind.MissileTransformDelta);
        }

        private static void PublishTransform(Component comp, ACTEventKind kind)
        {
            if (comp == null || !ACTConfigCache.Instance.Enabled)
                return;
            var unit = comp.GetComponent<Unit>();
            float alt = 0f;
            float spd = 0f;
            if (unit != null)
            {
                alt = unit.transform.position.y;
                var rb = unit.rb;
                if (rb != null)
                    spd = rb.velocity.magnitude * 3.6f;
            }
            string payload = $"0|{alt:F0}|{spd:F0}|0|0|radarOff";
            ACTEventBus.Publish(new ACTEvent(kind, SessionTime, (uint)comp.GetInstanceID(), payload: payload));
        }
    }
}
