using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NOLoader.AviationCareer.ACT.UI
{
    internal sealed class ACTMetricCardRefs
    {
        internal TMP_Text? Value;
        internal TMP_Text? Subtitle;
    }

    internal sealed class ACTBarRefs
    {
        internal Image? Fill;
        internal TMP_Text? Value;
    }

    internal sealed class ACTRingRefs
    {
        internal Image? Fill;
        internal TMP_Text? Center;
        internal TMP_Text? Subtitle;
    }

    internal static class ACTUiFactory
    {
        private static Sprite? _white;

        internal static Sprite WhiteSprite =>
            _white ?? (_white = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f));

        internal static TMP_FontAsset? ResolveFont()
        {
            var tmp = ACTGameUiBridge.MenuButtonTemplate?.GetComponentInChildren<TMP_Text>(true);
            return tmp != null ? tmp.font : null;
        }

        internal static TMP_Text TextIn(Transform parent, string content, float size, Color color, TextAlignmentOptions align,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject("Txt", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            var font = ResolveFont();
            if (font != null)
                tmp.font = font;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            tmp.text = content;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.margin = new Vector4(4f, 2f, 4f, 2f);
            return tmp;
        }

        internal static Image MakeImage(Transform parent, Color color, bool raycast = false)
        {
            var go = new GameObject("Img", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = WhiteSprite;
            img.type = Image.Type.Simple;
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        internal static void PanelFrame(Transform parent, Color bg)
        {
            Bg(parent, bg);
            var border = MakeImage(parent, ACTDesignTokens.BorderSubtle);
            var brt = border.rectTransform;
            brt.anchorMin = Vector2.zero;
            brt.anchorMax = Vector2.one;
            brt.offsetMin = new Vector2(-1f, -1f);
            brt.offsetMax = new Vector2(1f, 1f);
            border.raycastTarget = false;
        }

        internal static Image Bg(Transform parent, Color color, bool raycast = false)
        {
            var img = MakeImage(parent, color, raycast);
            ACTRect.Stretch(img.rectTransform);
            return img;
        }

        internal static ACTRingRefs RankRing(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var host = ACTRect.Create(parent, "Badge", anchorMin, anchorMax);
            host.gameObject.AddComponent<RectMask2D>();
            var fitter = host.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            fitter.aspectRatio = 1f;

            PanelFrame(host, ACTDesignTokens.BgInset);

            var track = MakeImage(host, ACTDesignTokens.BarTrack);
            var trackRt = track.rectTransform;
            trackRt.anchorMin = new Vector2(0.1f, 0.9f);
            trackRt.anchorMax = new Vector2(0.9f, 0.96f);
            trackRt.offsetMin = Vector2.zero;
            trackRt.offsetMax = Vector2.zero;

            var fill = MakeImage(host, ACTDesignTokens.AccentCyan);
            var fillRt = fill.rectTransform;
            fillRt.anchorMin = new Vector2(0.1f, 0.9f);
            fillRt.anchorMax = new Vector2(0.9f, 0.96f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0.05f;

            var center = TextIn(host, "1", 28f, ACTDesignTokens.TextBright, TextAlignmentOptions.Center,
                new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.82f));
            center.fontStyle = FontStyles.Bold;

            var sub = TextIn(host, "CADET", ACTDesignTokens.FontSmall, ACTDesignTokens.AccentCyan, TextAlignmentOptions.Center,
                new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.24f));

            return new ACTRingRefs { Fill = fill, Center = center, Subtitle = sub };
        }

        internal static ACTMetricCardRefs MetricCard(Transform parent, string label, Color accent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var card = ACTRect.Create(parent, "Card", anchorMin, anchorMax);
            PanelFrame(card, ACTDesignTokens.BgInset);

            var accentBar = MakeImage(card, accent);
            var abRt = accentBar.rectTransform;
            abRt.anchorMin = new Vector2(0, 0.08f);
            abRt.anchorMax = new Vector2(0, 0.92f);
            abRt.pivot = new Vector2(0, 0.5f);
            abRt.sizeDelta = new Vector2(3f, 0);

            TextIn(card, label.ToUpperInvariant(), ACTDesignTokens.FontCardLabel, ACTDesignTokens.TextMuted, TextAlignmentOptions.MidlineLeft,
                new Vector2(0.12f, 0.72f), new Vector2(0.95f, 0.94f));

            var val = TextIn(card, "0", ACTDesignTokens.FontCardValue, ACTDesignTokens.TextBright, TextAlignmentOptions.MidlineLeft,
                new Vector2(0.12f, 0.34f), new Vector2(0.95f, 0.72f));
            val.fontStyle = FontStyles.Bold;

            var sub = TextIn(card, "—", ACTDesignTokens.FontSmall, ACTDesignTokens.TextMuted, TextAlignmentOptions.MidlineLeft,
                new Vector2(0.12f, 0.08f), new Vector2(0.95f, 0.32f));

            return new ACTMetricCardRefs { Value = val, Subtitle = sub };
        }

        internal static ACTBarRefs Bar(Transform parent, string label, Color fillColor, Vector2 anchorMin, Vector2 anchorMax)
        {
            var row = ACTRect.Create(parent, "Bar", anchorMin, anchorMax);
            PanelFrame(row, ACTDesignTokens.BgInset);

            TextIn(row, label.ToUpperInvariant(), ACTDesignTokens.FontBody, ACTDesignTokens.TextBright, TextAlignmentOptions.MidlineLeft,
                new Vector2(0.03f, 0.52f), new Vector2(0.7f, 0.94f));

            var val = TextIn(row, "0", ACTDesignTokens.FontBody, ACTDesignTokens.AccentCyan, TextAlignmentOptions.MidlineRight,
                new Vector2(0.72f, 0.52f), new Vector2(0.97f, 0.94f));
            val.fontStyle = FontStyles.Bold;

            var track = MakeImage(row, ACTDesignTokens.BarTrack);
            var trackRt = track.rectTransform;
            trackRt.anchorMin = new Vector2(0.03f, 0.1f);
            trackRt.anchorMax = new Vector2(0.97f, 0.46f);
            trackRt.offsetMin = Vector2.zero;
            trackRt.offsetMax = Vector2.zero;

            var fill = MakeImage(row, fillColor);
            var fillRt = fill.rectTransform;
            fillRt.anchorMin = new Vector2(0.03f, 0.1f);
            fillRt.anchorMax = new Vector2(0.97f, 0.46f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;

            return new ACTBarRefs { Fill = fill, Value = val };
        }

        internal static (Image fill, TMP_Text label) XpBar(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var host = ACTRect.Create(parent, "XpBar", anchorMin, anchorMax);

            var label = TextIn(host, "XP", ACTDesignTokens.FontSmall, ACTDesignTokens.TextMuted, TextAlignmentOptions.BottomLeft,
                new Vector2(0, 0.62f), new Vector2(1, 1f));

            var track = MakeImage(host, ACTDesignTokens.BarTrack);
            var trackRt = track.rectTransform;
            trackRt.anchorMin = new Vector2(0, 0.08f);
            trackRt.anchorMax = new Vector2(1, 0.58f);
            trackRt.offsetMin = Vector2.zero;
            trackRt.offsetMax = Vector2.zero;

            var fill = MakeImage(host, ACTDesignTokens.AccentViolet);
            var fillRt = fill.rectTransform;
            fillRt.anchorMin = new Vector2(0, 0.08f);
            fillRt.anchorMax = new Vector2(1, 0.58f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;

            return (fill, label);
        }
    }
}
