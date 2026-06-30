using System.Collections.Generic;
using NOLoader.AviationCareer.ACT.Core;
using NOLoader.AviationCareer.ACT.Recording;
using UnityEngine;

namespace NOLoader.AviationCareer.ACT.Recording.TacView
{
    public sealed class ACTTacViewPlayer : MonoBehaviour
    {
        private ACTNorepReader? _reader;
        private ACTTimelineController? _timeline;
        private ACTRibbonTrackRenderer? _ribbons;
        private ACTTelemetryOverlayPool? _overlays;
        private ACTEventMarkerLayer? _markers;
        private float _playbackSpeed = 1f;

        public static ACTTacViewPlayer Launch(string norepPath)
        {
            var root = new GameObject("ACT_TacViewRoot");
            Object.DontDestroyOnLoad(root);
            var player = root.AddComponent<ACTTacViewPlayer>();
            player.Load(norepPath);
            return player;
        }

        private void Load(string path)
        {
            _reader = new ACTNorepReader();
            if (!_reader.Load(path))
            {
                Debug.LogWarning($"[ACT] Failed to load .norep: {path}");
                return;
            }

            _timeline = gameObject.AddComponent<ACTTimelineController>();
            _timeline.Initialize(_reader.Frames.Count, ACTConfigCache.Instance.TacViewDefaultSpeed);

            _ribbons = gameObject.AddComponent<ACTRibbonTrackRenderer>();
            _ribbons.Initialize(_reader.Frames);

            _overlays = gameObject.AddComponent<ACTTelemetryOverlayPool>();
            _overlays.Initialize();

            _markers = gameObject.AddComponent<ACTEventMarkerLayer>();
            _markers.Initialize(_reader.Frames);

            SetupCamera();
            Debug.Log($"[ACT] TacView loaded {_reader.Frames.Count} frames — {_reader.MissionName}");
        }

        private void SetupCamera()
        {
            var camGo = new GameObject("ACT_TacViewCamera");
            camGo.transform.SetParent(transform);
            camGo.transform.position = new Vector3(0, 5000f, -10000f);
            camGo.transform.rotation = Quaternion.Euler(30f, 0, 0);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 8000f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.05f, 0.08f);
        }

        private void Update()
        {
            if (_timeline == null || _reader == null)
                return;
            _timeline.Tick(Time.deltaTime * _playbackSpeed);
            int frameIndex = _timeline.CurrentFrameIndex;
            if (frameIndex >= 0 && frameIndex < _reader.Frames.Count)
            {
                var frame = _reader.Frames[frameIndex];
                _ribbons?.ScrubTo(frameIndex);
                _markers?.Highlight(frameIndex);
            }
        }

        public void SetSpeed(float speed) => _playbackSpeed = Mathf.Clamp(speed, 1f, 8f);

        public void Shutdown()
        {
            if (gameObject != null)
                Destroy(gameObject);
        }
    }

    public sealed class ACTTimelineController : MonoBehaviour
    {
        public int CurrentFrameIndex { get; private set; }
        private int _maxFrames;
        private float _accum;

        public void Initialize(int maxFrames, float defaultSpeed)
        {
            _maxFrames = Mathf.Max(1, maxFrames);
            CurrentFrameIndex = 0;
        }

        public void Tick(float dt)
        {
            _accum += dt * 10f;
            if (_accum >= 1f)
            {
                _accum = 0;
                CurrentFrameIndex = (CurrentFrameIndex + 1) % _maxFrames;
            }
        }

        public void SeekNormalized(float t)
        {
            CurrentFrameIndex = Mathf.Clamp(Mathf.RoundToInt(t * (_maxFrames - 1)), 0, _maxFrames - 1);
        }
    }

    public sealed class ACTRibbonTrackRenderer : MonoBehaviour
    {
        private List<ActFrame> _frames = new List<ActFrame>();
        private LineRenderer? _line;

        public void Initialize(List<ActFrame> frames)
        {
            _frames = frames;
            _line = gameObject.AddComponent<LineRenderer>();
            _line.positionCount = 0;
            _line.startWidth = 50f;
            _line.endWidth = 50f;
            _line.material = new Material(Shader.Find("Sprites/Default"));
            _line.startColor = Color.green;
            _line.endColor = Color.green;
        }

        public void ScrubTo(int index)
        {
            if (_line == null)
                return;
            int count = Mathf.Min(index + 1, _frames.Count);
            _line.positionCount = count;
            for (int i = 0; i < count; i++)
            {
                float x = i * 10f;
                float y = (_frames[i].TickMs % 1000) * 0.1f;
                _line.SetPosition(i, new Vector3(x, y, 0));
            }
            SetRibbonColor(index);
        }

        private void SetRibbonColor(int index)
        {
            if (_line == null || index >= _frames.Count)
                return;
            var type = _frames[index].Type;
            Color c = Color.green;
            if (type == NorepFormat.FrameType.RadarStateChange)
                c = Color.yellow;
            else if (type == NorepFormat.FrameType.CombatEvent)
                c = Color.red;
            _line.startColor = c;
            _line.endColor = c;
        }
    }

    public sealed class ACTTelemetryOverlayPool : MonoBehaviour
    {
        public void Initialize() { }

        public void Show(float speed, float alt, float hdg, float aoa, float g, float thrust)
        {
        }
    }

    public sealed class ACTEventMarkerLayer : MonoBehaviour
    {
        private List<ActFrame> _frames = new List<ActFrame>();

        public void Initialize(List<ActFrame> frames) => _frames = frames;

        public void Highlight(int index)
        {
        }

        public void FocusMarker(int index)
        {
        }
    }
}
