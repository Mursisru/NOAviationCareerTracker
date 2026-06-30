using System.Collections.Generic;
using NOLoader.AviationCareer.ACT.Core.Analytics;
using NOLoader.AviationCareer.ACT.UI.Tabs;
using TMPro;
using UnityEngine;

namespace NOLoader.AviationCareer.ACT.UI
{
    internal static class ACTTabVisualRenderer
    {
        internal static void Render(RectTransform host, string tabId, IACTViewContext? context)
        {
            for (int i = host.childCount - 1; i >= 0; i--)
                Object.Destroy(host.GetChild(i).gameObject);

            ACTUiFactory.TextIn(host, GetTitle(tabId), ACTDesignTokens.FontSection, ACTDesignTokens.AccentCyan,
                TextAlignmentOptions.MidlineLeft, new Vector2(0.02f, 0.92f), new Vector2(0.7f, 0.99f)).fontStyle = FontStyles.Bold;

            var body = ACTRect.Create(host, "Body", new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.9f));

            switch (tabId)
            {
                case "faction": RenderFaction(body); break;
                case "weapons": RenderWeapons(body); break;
                case "flight": RenderFlight(body); break;
                case "patterns": RenderPatterns(body); break;
                case "blackbox": RenderBlackBox(body); break;
                case "recorder": RenderRecorder(body, context); break;
                default: Empty(body); break;
            }
        }

        private static string GetTitle(string tabId)
        {
            switch (tabId)
            {
                case "faction": return "COMBAT LEDGER";
                case "weapons": return "WEAPONS AUDIT";
                case "flight": return "FLIGHT PERFORMANCE";
                case "patterns": return "TACTICAL PATTERNS";
                case "blackbox": return "BLACK BOX";
                case "recorder": return "RECORDER / TACVIEW";
                default: return "FLIGHT LOG";
            }
        }

        private static void RenderFaction(Transform body)
        {
            var svc = TabServiceLocator.TryGet<FactionLedgerService>();
            if (svc == null) { Empty(body); return; }

            ACTUiFactory.MetricCard(body, "Kills", ACTDesignTokens.AccentGreen, new Vector2(0, 0.55f), new Vector2(0.24f, 0.98f)).Value!.text = svc.KillsByTeam.ToString();
            ACTUiFactory.MetricCard(body, "Friendly Fire", ACTDesignTokens.AccentMagenta, new Vector2(0.255f, 0.55f), new Vector2(0.495f, 0.98f)).Value!.text = svc.FriendlyFireEvents.ToString();
            ACTUiFactory.MetricCard(body, "Collateral", ACTDesignTokens.AccentAmber, new Vector2(0.51f, 0.55f), new Vector2(0.75f, 0.98f)).Value!.text = svc.CollateralEvents.ToString();
            ACTUiFactory.MetricCard(body, "Guardian Angel", ACTDesignTokens.AccentCyan, new Vector2(0.765f, 0.55f), new Vector2(0.99f, 0.98f)).Value!.text = svc.GuardianAngelEvents.ToString();

            ACTUiFactory.TextIn(body, "Session combat summary — values reset each mission.", ACTDesignTokens.FontBody,
                ACTDesignTokens.TextMuted, TextAlignmentOptions.MidlineLeft, new Vector2(0, 0.08f), new Vector2(1, 0.48f));
        }

        private static void RenderWeapons(Transform body)
        {
            var svc = TabServiceLocator.TryGet<WeaponsAuditService>();
            if (svc == null) { Empty(body); return; }

            ACTUiFactory.MetricCard(body, "Intercept XP", ACTDesignTokens.AccentViolet, new Vector2(0, 0.72f), new Vector2(0.32f, 0.98f)).Value!.text = svc.TotalInterceptXp.ToString("N0");
            ACTUiFactory.MetricCard(body, "Primary Weapon", ACTDesignTokens.AccentAmber, new Vector2(0.34f, 0.72f), new Vector2(0.99f, 0.98f)).Value!.text = svc.GetRecommendation().ToUpperInvariant();

            RenderWeaponBars(body, GetFiredCounts(svc));
        }

        private static void RenderFlight(Transform body)
        {
            var svc = TabServiceLocator.TryGet<FlightPerformanceService>();
            if (svc == null) { Empty(body); return; }

            SetBar(body, "Peak G", svc.PeakGLoad, 12f, ACTDesignTokens.AccentMagenta, 0.82f, 0.98f);
            SetBar(body, "Belly Slide (m)", svc.BellySlideDistanceM, 5000f, ACTDesignTokens.AccentAmber, 0.64f, 0.8f);
            SetBar(body, "Glide (m)", svc.GlideDistanceM, 8000f, ACTDesignTokens.AccentCyan, 0.46f, 0.62f);
            SetBar(body, "Storm (sec)", svc.StormFlightSec, 600f, ACTDesignTokens.AccentViolet, 0.28f, 0.44f);
            SetBar(body, "Fog (sec)", svc.FogFlightSec, 600f, ACTDesignTokens.AccentGreen, 0.1f, 0.26f);
        }

        private static void RenderPatterns(Transform body)
        {
            var svc = TabServiceLocator.TryGet<TacticalPatternService>();
            if (svc == null) { Empty(body); return; }

            var list = svc.DetectedPatterns;
            if (list.Count == 0)
            {
                var panel = ACTRect.Create(body, "Empty", new Vector2(0, 0.35f), new Vector2(1, 0.95f));
                ACTUiFactory.PanelFrame(panel, ACTDesignTokens.BgInset);
                ACTUiFactory.TextIn(panel, "No tactical patterns detected in this session yet.", ACTDesignTokens.FontBody,
                    ACTDesignTokens.TextMuted, TextAlignmentOptions.Midline, Vector2.zero, Vector2.one);
                return;
            }

            Color[] colors = { ACTDesignTokens.AccentCyan, ACTDesignTokens.AccentViolet, ACTDesignTokens.AccentGreen, ACTDesignTokens.AccentAmber };
            float rowH = 0.14f;
            for (int i = 0; i < list.Count && i < 6; i++)
            {
                float yMax = 0.98f - i * rowH;
                float yMin = yMax - rowH + 0.012f;
                var badge = ACTRect.Create(body, "Badge", new Vector2(0, yMin), new Vector2(1, yMax));
                ACTUiFactory.PanelFrame(badge, new Color(colors[i % colors.Length].r, colors[i % colors.Length].g, colors[i % colors.Length].b, 0.15f));
                ACTUiFactory.TextIn(badge, "◆  " + list[i].ToUpperInvariant(), ACTDesignTokens.FontBody, colors[i % colors.Length],
                    TextAlignmentOptions.MidlineLeft, new Vector2(0.02f, 0.1f), new Vector2(0.98f, 0.9f));
            }
        }

        private static void RenderBlackBox(Transform body)
        {
            var svc = TabServiceLocator.TryGet<BlackBoxService>();
            if (svc == null) { Empty(body); return; }

            var ring = ACTUiFactory.RankRing(body, new Vector2(0, 0.25f), new Vector2(0.18f, 0.98f));
            if (ring.Fill != null)
            {
                ring.Fill.color = ACTDesignTokens.AccentAmber;
                ring.Fill.fillAmount = Mathf.Max(0.05f, Mathf.Clamp01(svc.CloseCallCount / 5f));
            }
            if (ring.Center != null) ring.Center.text = svc.CloseCallCount.ToString();
            if (ring.Subtitle != null) ring.Subtitle.text = "CLOSE CALLS";

            ACTUiFactory.MetricCard(body, "Close-Call Ejections", ACTDesignTokens.AccentMagenta, new Vector2(0.2f, 0.55f), new Vector2(0.42f, 0.98f)).Value!.text = svc.CloseCallCount.ToString();

            var info = ACTRect.Create(body, "Info", new Vector2(0.2f, 0.08f), new Vector2(0.99f, 0.5f));
            ACTUiFactory.PanelFrame(info, ACTDesignTokens.BgInset);
            ACTUiFactory.TextIn(info, "45-second telemetry ring buffer powers pattern detection and post-mission review.",
                ACTDesignTokens.FontBody, ACTDesignTokens.TextMuted, TextAlignmentOptions.MidlineLeft,
                new Vector2(0.04f, 0.1f), new Vector2(0.96f, 0.9f));
        }

        private static void RenderRecorder(Transform body, IACTViewContext? context)
        {
            var svc = TabServiceLocator.TryGet<RecorderService>();
            if (svc == null) { Empty(body); return; }

            var idle = ACTUiFactory.MetricCard(body, svc.IsRecording ? "Rec Active" : "Rec Idle",
                svc.IsRecording ? ACTDesignTokens.AccentGreen : ACTDesignTokens.TextMuted,
                new Vector2(0, 0.55f), new Vector2(0.34f, 0.98f));
            if (idle.Value != null) idle.Value.text = svc.IsRecording ? "LIVE" : "STANDBY";
            if (idle.Subtitle != null) idle.Subtitle.text = svc.IsRecording ? "recording" : "standby";

            var saved = ACTUiFactory.MetricCard(body, "Saved Replays", ACTDesignTokens.AccentCyan, new Vector2(0.36f, 0.55f), new Vector2(0.58f, 0.98f));
            if (saved.Value != null) saved.Value.text = svc.Recordings.Count.ToString();
            if (saved.Subtitle != null) saved.Subtitle.text = "on disk";

            if (!string.IsNullOrEmpty(svc.CurrentFile))
            {
                ACTUiFactory.TextIn(body, System.IO.Path.GetFileName(svc.CurrentFile), ACTDesignTokens.FontSmall,
                    ACTDesignTokens.TextMuted, TextAlignmentOptions.MidlineLeft, new Vector2(0, 0.08f), new Vector2(0.7f, 0.48f));
            }
        }

        private static void SetBar(Transform body, string label, float value, float max, Color color, float yMin, float yMax)
        {
            var bar = ACTUiFactory.Bar(body, label, color, new Vector2(0, yMin), new Vector2(1, yMax));
            if (bar.Fill != null) bar.Fill.fillAmount = Mathf.Clamp01(value / Mathf.Max(max, 0.001f));
            if (bar.Value != null) bar.Value.text = value.ToString("F0");
        }

        private static Dictionary<string, int> GetFiredCounts(WeaponsAuditService svc)
        {
            var map = new Dictionary<string, int>();
            foreach (var line in svc.BuildReport().Split('\n'))
            {
                if (!line.StartsWith("Fired ")) continue;
                var rest = line.Substring(6);
                var idx = rest.LastIndexOf(':');
                if (idx <= 0) continue;
                if (int.TryParse(rest.Substring(idx + 1).Trim(), out var c))
                    map[rest.Substring(0, idx).Trim()] = c;
            }
            return map;
        }

        private static void RenderWeaponBars(Transform body, Dictionary<string, int> data)
        {
            if (data.Count == 0)
            {
                ACTUiFactory.TextIn(body, "No weapon events recorded.", ACTDesignTokens.FontBody, ACTDesignTokens.TextMuted,
                    TextAlignmentOptions.MidlineLeft, new Vector2(0, 0.35f), new Vector2(1, 0.65f));
                return;
            }

            int max = 1;
            foreach (var kv in data) max = Mathf.Max(max, kv.Value);
            int row = 0;
            foreach (var kv in data)
            {
                float yMax = 0.66f - row * 0.12f;
                float yMin = yMax - 0.11f;
                SetBar(body, kv.Key, kv.Value, max, ACTDesignTokens.AccentCyan, yMin, yMax);
                if (++row >= 5) break;
            }
        }

        private static void Empty(Transform body)
        {
            ACTUiFactory.TextIn(body, "No session data available.", ACTDesignTokens.FontBody, ACTDesignTokens.TextMuted,
                TextAlignmentOptions.Center, new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.6f));
        }
    }
}
