using System;

namespace NOLoader.AviationCareer.ACT.Core.Analytics
{
    public readonly struct ACTTelemetrySample
    {
        public float Time { get; }
        public float AltitudeM { get; }
        public float SpeedKmh { get; }
        public float SpeedMach { get; }
        public bool RadarOff { get; }
        public float GLoad { get; }
        public bool GearDown { get; }
        public float BvrRangeKm { get; }

        public ACTTelemetrySample(float time, float altitudeM, float speedKmh, float speedMach, bool radarOff, float gLoad, bool gearDown, float bvrRangeKm)
        {
            Time = time;
            AltitudeM = altitudeM;
            SpeedKmh = speedKmh;
            SpeedMach = speedMach;
            RadarOff = radarOff;
            GLoad = gLoad;
            GearDown = gearDown;
            BvrRangeKm = bvrRangeKm;
        }
    }

    public sealed class RingBuffer45s
    {
        private const float WindowSec = 45f;
        private readonly ACTTelemetrySample[] _buffer;
        private int _head;
        private int _count;

        public RingBuffer45s(int capacity = 512)
        {
            _buffer = new ACTTelemetrySample[capacity];
        }

        public void Push(ACTTelemetrySample sample)
        {
            _buffer[_head] = sample;
            _head = (_head + 1) % _buffer.Length;
            if (_count < _buffer.Length)
                _count++;
            Trim(sample.Time);
        }

        public int Count => _count;

        public ACTTelemetrySample GetFromOldest(int index)
        {
            if (_count == 0)
                return default;
            int start = (_head - _count + _buffer.Length) % _buffer.Length;
            return _buffer[(start + index) % _buffer.Length];
        }

        private void Trim(float now)
        {
            while (_count > 0)
            {
                var oldest = GetFromOldest(0);
                if (now - oldest.Time <= WindowSec)
                    break;
                _count--;
            }
        }

        public float Duration()
        {
            if (_count < 2)
                return 0f;
            return GetFromOldest(_count - 1).Time - GetFromOldest(0).Time;
        }
    }
}
