using System;
using System.Collections.Generic;

namespace NOLoader.AviationCareer.ACT.Core
{
    public enum ACTEventKind
    {
        SessionStarted,
        SessionEnded,
        PilotStateChanged,
        GearChanged,
        WeaponFired,
        ContinuousFire,
        MissileSpawned,
        MissileCleared,
        MissileTargetAssigned,
        UnitDestroyed,
        DamageApplied,
        PilotEjected,
        EntityTransformDelta,
        MissileTransformDelta,
        RadarStateChange,
        SamHardLock,
        GamePaused,
        DebriefOpened,
        BriefingOpened,
        MainMenuReady,
        InputEdge,
        PatternDetected,
        XpAwarded,
        XpPenaltyApplied
    }

    public readonly struct ACTEvent
    {
        public ACTEventKind Kind { get; }
        public float SessionTime { get; }
        public uint SourceEntityId { get; }
        public uint TargetEntityId { get; }
        public string Payload { get; }

        public ACTEvent(ACTEventKind kind, float sessionTime, uint sourceEntityId = 0, uint targetEntityId = 0, string payload = "")
        {
            Kind = kind;
            SessionTime = sessionTime;
            SourceEntityId = sourceEntityId;
            TargetEntityId = targetEntityId;
            Payload = payload ?? string.Empty;
        }
    }

    public static class ACTEventBus
    {
        private static readonly List<Action<ACTEvent>> Subscribers = new List<Action<ACTEvent>>();
        private static readonly object Gate = new object();

        public static void Subscribe(Action<ACTEvent> handler)
        {
            if (handler == null)
                return;
            lock (Gate)
                Subscribers.Add(handler);
        }

        public static void Unsubscribe(Action<ACTEvent> handler)
        {
            if (handler == null)
                return;
            lock (Gate)
                Subscribers.Remove(handler);
        }

        public static void Publish(ACTEvent evt)
        {
            Action<ACTEvent>[] snapshot;
            lock (Gate)
                snapshot = Subscribers.ToArray();

            for (int i = 0; i < snapshot.Length; i++)
            {
                try
                {
                    snapshot[i](evt);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }
        }

        public static void Clear()
        {
            lock (Gate)
                Subscribers.Clear();
        }
    }
}
