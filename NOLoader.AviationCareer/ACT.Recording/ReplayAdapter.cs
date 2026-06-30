using System;
using System.Text;
using NOLoader.AviationCareer.ACT.Core;

namespace NOLoader.AviationCareer.ACT.Recording
{
    public static class ReplayAdapter
    {
        private static float _sessionStart;

        public static void HandleEvent(ACTEvent evt, ACTDeltaBuffer buffer, ACTSessionRuntime session)
        {
            uint tickMs = (uint)Math.Max(0, evt.SessionTime * 1000f);

            switch (evt.Kind)
            {
                case ACTEventKind.SessionStarted:
                    _sessionStart = evt.SessionTime;
                    WriteMarker(buffer, tickMs, 0, evt.Payload);
                    break;
                case ACTEventKind.WeaponFired:
                    WriteCombat(buffer, tickMs, evt.SourceEntityId, NorepFormat.CombatEventKind.Fire, evt.Payload);
                    break;
                case ACTEventKind.UnitDestroyed:
                    WriteCombat(buffer, tickMs, evt.SourceEntityId, NorepFormat.CombatEventKind.Kill, evt.Payload);
                    break;
                case ACTEventKind.PilotEjected:
                    WriteCombat(buffer, tickMs, evt.SourceEntityId, NorepFormat.CombatEventKind.Eject, evt.Payload);
                    break;
                case ACTEventKind.EntityTransformDelta:
                case ACTEventKind.MissileTransformDelta:
                    WriteTransform(buffer, tickMs, evt);
                    break;
                case ACTEventKind.RadarStateChange:
                    WriteRadar(buffer, tickMs, evt.SourceEntityId, evt.TargetEntityId, evt.Payload);
                    break;
                case ACTEventKind.SessionEnded:
                    WriteMarker(buffer, tickMs, 1, "end");
                    break;
            }
        }

        private static void WriteMarker(ACTDeltaBuffer buffer, uint tickMs, byte kind, string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data ?? string.Empty);
            var payload = new byte[3 + bytes.Length];
            payload[0] = kind;
            payload[1] = (byte)(bytes.Length & 0xFF);
            payload[2] = (byte)((bytes.Length >> 8) & 0xFF);
            Buffer.BlockCopy(bytes, 0, payload, 3, bytes.Length);
            buffer.Enqueue(new ActFrame(tickMs, NorepFormat.FrameType.SessionMarker, payload));
        }

        private static void WriteCombat(ACTDeltaBuffer buffer, uint tickMs, uint entityId, NorepFormat.CombatEventKind kind, string weapon)
        {
            var weaponHash = Crc32(weapon);
            var payload = new byte[11];
            BitConverter.GetBytes(entityId).CopyTo(payload, 0);
            payload[4] = (byte)kind;
            BitConverter.GetBytes(weaponHash).CopyTo(payload, 5);
            BitConverter.GetBytes((ushort)0).CopyTo(payload, 9);
            buffer.Enqueue(new ActFrame(tickMs, NorepFormat.FrameType.CombatEvent, payload));
        }

        private static void WriteTransform(ACTDeltaBuffer buffer, uint tickMs, ACTEvent evt)
        {
            var payload = new byte[4];
            BitConverter.GetBytes(evt.SourceEntityId).CopyTo(payload, 0);
            buffer.Enqueue(new ActFrame(tickMs, NorepFormat.FrameType.EntityTransformDelta, payload));
        }

        private static void WriteRadar(ACTDeltaBuffer buffer, uint tickMs, uint entityId, uint targetId, string mode)
        {
            byte modeByte = 0;
            if (mode.IndexOf("scan", StringComparison.OrdinalIgnoreCase) >= 0) modeByte = 1;
            else if (mode.IndexOf("lock", StringComparison.OrdinalIgnoreCase) >= 0) modeByte = 2;
            var payload = new byte[9];
            BitConverter.GetBytes(entityId).CopyTo(payload, 0);
            payload[4] = modeByte;
            BitConverter.GetBytes(targetId).CopyTo(payload, 5);
            buffer.Enqueue(new ActFrame(tickMs, NorepFormat.FrameType.RadarStateChange, payload));
        }

        private static uint Crc32(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
            uint crc = 0xFFFFFFFF;
            for (int i = 0; i < bytes.Length; i++)
            {
                crc ^= bytes[i];
                for (int b = 0; b < 8; b++)
                    crc = (crc >> 1) ^ (0xEDB88320u & ~((crc & 1) - 1));
            }
            return ~crc;
        }
    }
}
