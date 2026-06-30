using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NOLoader.AviationCareer.ACT.UI
{
    internal static class ACTUiStyles
    {
        internal static readonly Color PanelBg = new Color(0.08f, 0.1f, 0.12f, 0.88f);
        internal static readonly Color PanelBorder = new Color(0.55f, 0.58f, 0.62f, 0.35f);
        internal static readonly Color FallbackButtonBg = new Color(0.22f, 0.24f, 0.27f, 0.72f);
        internal static readonly Color FallbackButtonHi = new Color(0.32f, 0.35f, 0.4f, 0.9f);
        internal static readonly Color TextPrimary = new Color(0.86f, 0.88f, 0.9f, 1f);

        internal static Text CreateFallbackText(Transform parent, string text, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject("ACT_Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = fontSize;
            t.color = TextPrimary;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            t.text = text;
            Stretch(go.GetComponent<RectTransform>());
            return t;
        }

        internal static Button CreateFallbackButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject("ACT_Btn_" + label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = FallbackButtonBg;
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = FallbackButtonBg;
            colors.highlightedColor = FallbackButtonHi;
            colors.pressedColor = FallbackButtonHi;
            btn.colors = colors;
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var textGo = new GameObject("Label", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 14;
            text.color = TextPrimary;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.text = label.ToUpperInvariant();
            Stretch(textGo.GetComponent<RectTransform>());
            return btn;
        }

        internal static RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var bg = go.AddComponent<Image>();
            bg.color = PanelBg;
            bg.raycastTarget = true;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = PanelBorder;
            outline.effectDistance = new Vector2(1f, -1f);
            return rt;
        }

        internal static ScrollRect CreateScrollBody(Transform parent, out RectTransform contentRt, out Text legacyText, out TMP_Text? tmpText)
        {
            var scrollGo = new GameObject("ACT_Scroll", typeof(RectTransform));
            scrollGo.transform.SetParent(parent, false);
            Stretch(scrollGo.GetComponent<RectTransform>());

            var viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(scrollGo.transform, false);
            Stretch(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = new Vector2(0, 400);

            tmpText = ACTGameUiBridge.CreateBodyText(content.transform);
            if (tmpText != null)
            {
                legacyText = null!;
            }
            else
            {
                legacyText = CreateFallbackText(content.transform, "", 14, TextAnchor.UpperLeft);
                tmpText = null;
            }

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRt;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            return scroll;
        }

        internal static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        internal static (Vector2 min, Vector2 max) OverlayEntryRect(ACTGameContextKind kind)
        {
            if (kind == ACTGameContextKind.MainMenu)
                return (Vector2.zero, Vector2.zero);
            return (new Vector2(0.78f, 0.9f), new Vector2(0.985f, 0.965f));
        }

        internal static (Vector2 min, Vector2 max) WindowRect(ACTGameContextKind kind) =>
            (Vector2.zero, Vector2.one);
    }
}
