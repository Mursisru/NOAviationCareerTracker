using NOLoader.AviationCareer.ACT.Core;
using NOLoader.AviationCareer.ACT.Core.Progression;
using TMPro;
using UnityEngine;

namespace NOLoader.AviationCareer.ACT.UI
{
    public sealed class ACTDashboardView : MonoBehaviour
    {
        private ACTRingRefs? _ring;
        private UnityEngine.UI.Image? _xpFill;
        private TMP_Text? _xpLabel;
        private TMP_Text? _rankTitle;
        private ACTMetricCardRefs? _kd;
        private ACTMetricCardRefs? _xp;
        private ACTMetricCardRefs? _net;
        private ACTBarRefs? _destroyed;
        private ACTBarRefs? _lost;
        private TMP_Text? _favAircraft;
        private TMP_Text? _favTactic;
        private TMP_Text? _favWeapon;

        public void Build(Transform parent)
        {
            ACTUiFactory.PanelFrame(parent, ACTDesignTokens.BgCard);

            _ring = ACTUiFactory.RankRing(parent, new Vector2(0.015f, 0.1f), new Vector2(0.145f, 0.9f));

            ACTUiFactory.TextIn(parent, "CAREER SNAPSHOT", ACTDesignTokens.FontCardLabel, ACTDesignTokens.AccentCyan,
                TextAlignmentOptions.MidlineLeft, new Vector2(0.17f, 0.88f), new Vector2(0.58f, 0.97f));

            _rankTitle = ACTUiFactory.TextIn(parent, "CADET", 26f, ACTDesignTokens.TextBright,
                TextAlignmentOptions.MidlineLeft, new Vector2(0.17f, 0.68f), new Vector2(0.58f, 0.88f));
            _rankTitle.fontStyle = FontStyles.Bold;

            var xpPair = ACTUiFactory.XpBar(parent, new Vector2(0.17f, 0.56f), new Vector2(0.58f, 0.66f));
            _xpFill = xpPair.fill;
            _xpLabel = xpPair.label;

            _destroyed = ACTUiFactory.Bar(parent, "Destroyed value", ACTDesignTokens.AccentGreen,
                new Vector2(0.17f, 0.42f), new Vector2(0.58f, 0.54f));
            _lost = ACTUiFactory.Bar(parent, "Lost value", ACTDesignTokens.AccentMagenta,
                new Vector2(0.17f, 0.3f), new Vector2(0.58f, 0.42f));

            _kd = ACTUiFactory.MetricCard(parent, "K/D", ACTDesignTokens.AccentMagenta,
                new Vector2(0.17f, 0.06f), new Vector2(0.29f, 0.28f));
            _xp = ACTUiFactory.MetricCard(parent, "Total XP", ACTDesignTokens.AccentViolet,
                new Vector2(0.295f, 0.06f), new Vector2(0.415f, 0.28f));
            _net = ACTUiFactory.MetricCard(parent, "Net Value", ACTDesignTokens.AccentGreen,
                new Vector2(0.42f, 0.06f), new Vector2(0.58f, 0.28f));

            var favPanel = ACTRect.Create(parent, "Fav", new Vector2(0.61f, 0.06f), new Vector2(0.985f, 0.94f));
            ACTUiFactory.PanelFrame(favPanel, ACTDesignTokens.BgPanel);

            ACTUiFactory.TextIn(favPanel, "SIGNATURE LOADOUT", ACTDesignTokens.FontCardLabel, ACTDesignTokens.TextMuted,
                TextAlignmentOptions.MidlineLeft, new Vector2(0.06f, 0.88f), new Vector2(0.94f, 0.97f));

            _favAircraft = ACTUiFactory.TextIn(favPanel, "AIRCRAFT\n—", ACTDesignTokens.FontBody, ACTDesignTokens.AccentAmber,
                TextAlignmentOptions.TopLeft, new Vector2(0.06f, 0.6f), new Vector2(0.94f, 0.84f));
            _favTactic = ACTUiFactory.TextIn(favPanel, "TACTIC\n—", ACTDesignTokens.FontBody, ACTDesignTokens.AccentCyan,
                TextAlignmentOptions.TopLeft, new Vector2(0.06f, 0.36f), new Vector2(0.94f, 0.58f));
            _favWeapon = ACTUiFactory.TextIn(favPanel, "WEAPON\n—", ACTDesignTokens.FontBody, ACTDesignTokens.AccentMagenta,
                TextAlignmentOptions.TopLeft, new Vector2(0.06f, 0.1f), new Vector2(0.94f, 0.32f));

            Refresh();
        }

        public void Refresh()
        {
            var p = ACTBootstrap.Profile;
            if (p == null)
                return;

            string rankName = RankSystem.GetRankTitle(p.CurrentRank).ToUpperInvariant();
            long xpNext = RankSystem.XpToNextRank(p.TotalXp, p.CurrentRank);
            long xpFloor = RankSystem.XpRequiredForRank(p.CurrentRank);
            long xpCeil = p.CurrentRank >= RankSystem.MaxRank ? p.TotalXp : RankSystem.XpRequiredForRank(p.CurrentRank + 1);
            float rankProgress = xpCeil > xpFloor ? (p.TotalXp - xpFloor) / (float)(xpCeil - xpFloor) : 1f;

            if (_ring != null)
            {
                if (_ring.Fill != null) _ring.Fill.fillAmount = Mathf.Max(0.05f, Mathf.Clamp01(p.CurrentRank / (float)RankSystem.MaxRank));
                if (_ring.Center != null) _ring.Center.text = p.CurrentRank.ToString();
                if (_ring.Subtitle != null) _ring.Subtitle.text = rankName;
            }

            if (_rankTitle != null) _rankTitle.text = rankName;
            if (_xpFill != null) _xpFill.fillAmount = Mathf.Clamp01(rankProgress);
            if (_xpLabel != null)
                _xpLabel.text = p.CurrentRank >= RankSystem.MaxRank
                    ? $"MAX RANK  ·  {p.TotalXp:N0} XP"
                    : $"{p.TotalXp:N0} XP  ·  {xpNext:N0} to rank {p.CurrentRank + 1}";

            if (_kd?.Value != null) _kd.Value.text = p.LifetimeWeightedKd.ToString("F2");
            if (_kd?.Subtitle != null) _kd.Subtitle.text = "lifetime";
            if (_xp?.Value != null) _xp.Value.text = FormatCompact(p.TotalXp);
            if (_xp?.Subtitle != null) _xp.Subtitle.text = $"rank {p.CurrentRank}/20";

            float net = p.DestroyedValueSum - p.LostValueSum;
            if (_net?.Value != null) _net.Value.text = FormatCompact((long)net);
            if (_net?.Subtitle != null) _net.Subtitle.text = net >= 0 ? "positive" : "negative";

            float maxVal = Mathf.Max(p.DestroyedValueSum, p.LostValueSum, 1f);
            if (_destroyed?.Fill != null) _destroyed.Fill.fillAmount = p.DestroyedValueSum / maxVal;
            if (_destroyed?.Value != null) _destroyed.Value.text = p.DestroyedValueSum.ToString("N0");
            if (_lost?.Fill != null) _lost.Fill.fillAmount = p.LostValueSum / maxVal;
            if (_lost?.Value != null) _lost.Value.text = p.LostValueSum.ToString("N0");

            if (_favAircraft != null) _favAircraft.text = "AIRCRAFT\n" + Truncate(p.FavoriteAircraft, 22);
            if (_favTactic != null) _favTactic.text = "TACTIC\n" + Truncate(p.FavoriteTactic, 22);
            if (_favWeapon != null) _favWeapon.text = "WEAPON\n" + Truncate(p.FavoriteWeapon, 22);
        }

        private static string FormatCompact(long v)
        {
            if (v >= 1_000_000) return (v / 1_000_000f).ToString("F1") + "M";
            if (v >= 1_000) return (v / 1_000f).ToString("F1") + "K";
            return v.ToString("N0");
        }

        private static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "—";
            return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
        }
    }
}
