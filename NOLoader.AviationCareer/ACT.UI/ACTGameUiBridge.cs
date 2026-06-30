using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NOLoader.AviationCareer.ACT.UI
{
    internal static class ACTGameUiBridge
    {
        internal static Button? MenuButtonTemplate { get; set; }

        internal static Button CreateMenuButton(Transform parent, string label, UnityAction onClick, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject("ACT_" + SanitizeName(label), typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return BuildStandaloneButton(go, MenuButtonTemplate, label, onClick);
        }

        internal static Button InsertMenuButtonAfter(Button afterButton, Button styleTemplate, string label, UnityAction onClick)
        {
            var go = new GameObject("ACT_" + SanitizeName(label), typeof(RectTransform));
            go.transform.SetParent(afterButton.transform.parent, false);

            var srcRt = styleTemplate.GetComponent<RectTransform>();
            var rt = go.GetComponent<RectTransform>();
            if (srcRt != null)
            {
                rt.anchorMin = srcRt.anchorMin;
                rt.anchorMax = srcRt.anchorMax;
                rt.pivot = srcRt.pivot;
                rt.sizeDelta = srcRt.sizeDelta;
                rt.localScale = Vector3.one;
            }

            CopyLayoutElement(styleTemplate.gameObject, go);
            var btn = BuildStandaloneButton(go, styleTemplate, label, onClick);
            go.transform.SetSiblingIndex(afterButton.transform.GetSiblingIndex() + 1);

            var parentRt = afterButton.transform.parent as RectTransform;
            if (parentRt != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRt);

            return btn;
        }

        internal static Button? FindButtonByLabel(Transform scope, string labelContains)
        {
            if (scope == null)
                return null;
            foreach (var btn in scope.GetComponentsInChildren<Button>(true))
            {
                var tmp = btn.GetComponentInChildren<TMP_Text>(true);
                if (tmp != null && tmp.text.IndexOf(labelContains, StringComparison.OrdinalIgnoreCase) >= 0)
                    return btn;
                var txt = btn.GetComponentInChildren<Text>(true);
                if (txt != null && txt.text.IndexOf(labelContains, StringComparison.OrdinalIgnoreCase) >= 0)
                    return btn;
            }
            return null;
        }

        private static Button BuildStandaloneButton(GameObject host, Button? styleRef, string label, UnityAction onClick)
        {
            var img = host.GetComponent<Image>() ?? host.AddComponent<Image>();
            if (styleRef != null)
            {
                var srcImg = styleRef.GetComponent<Image>();
                if (srcImg != null)
                {
                    img.sprite = srcImg.sprite;
                    img.type = srcImg.type;
                    img.color = srcImg.color;
                    img.material = srcImg.material;
                    img.raycastTarget = true;
                }
            }
            else
            {
                img.color = ACTUiStyles.FallbackButtonBg;
                img.raycastTarget = true;
            }

            var btn = host.GetComponent<Button>() ?? host.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
            if (styleRef != null)
            {
                btn.transition = styleRef.transition;
                btn.colors = styleRef.colors;
                btn.spriteState = styleRef.spriteState;
            }
            btn.onClick = new Button.ButtonClickedEvent();
            btn.onClick.AddListener(onClick);

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(host.transform, false);
            ACTUiStyles.Stretch(labelGo.GetComponent<RectTransform>());

            if (styleRef != null)
            {
                var srcTmp = styleRef.GetComponentInChildren<TMP_Text>(true);
                if (srcTmp != null)
                {
                    var tmp = labelGo.AddComponent<TextMeshProUGUI>();
                    tmp.font = srcTmp.font;
                    tmp.fontSharedMaterial = srcTmp.fontSharedMaterial;
                    tmp.fontSize = srcTmp.fontSize;
                    tmp.color = srcTmp.color;
                    tmp.fontStyle = srcTmp.fontStyle;
                    tmp.characterSpacing = srcTmp.characterSpacing;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.raycastTarget = false;
                    tmp.text = label.ToUpperInvariant();
                    return btn;
                }
            }

            var legacy = labelGo.AddComponent<Text>();
            legacy.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            legacy.fontSize = 14;
            legacy.color = ACTUiStyles.TextPrimary;
            legacy.alignment = TextAnchor.MiddleCenter;
            legacy.raycastTarget = false;
            legacy.text = label.ToUpperInvariant();
            return btn;
        }

        internal static void SetLabel(GameObject root, string label)
        {
            string upper = label.ToUpperInvariant();
            var tmp = root.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null)
            {
                tmp.text = upper;
                tmp.raycastTarget = false;
                return;
            }
            var txt = root.GetComponentInChildren<Text>(true);
            if (txt != null)
            {
                txt.text = upper;
                txt.raycastTarget = false;
            }
        }

        internal static TMP_Text? CreateBodyText(Transform parent)
        {
            var templateTmp = MenuButtonTemplate?.GetComponentInChildren<TMP_Text>(true);
            if (templateTmp != null)
            {
                var go = new GameObject("ACT_Body", typeof(RectTransform));
                go.transform.SetParent(parent, false);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.font = templateTmp.font;
                tmp.fontSharedMaterial = templateTmp.fontSharedMaterial;
                tmp.fontSize = Mathf.Max(14f, templateTmp.fontSize * 0.75f);
                tmp.color = templateTmp.color;
                tmp.alignment = TextAlignmentOptions.TopLeft;
                tmp.enableWordWrapping = true;
                tmp.overflowMode = TextOverflowModes.Overflow;
                tmp.raycastTarget = false;
                var rt = go.GetComponent<RectTransform>();
                ACTUiStyles.Stretch(rt);
                rt.offsetMin = new Vector2(10f, 8f);
                rt.offsetMax = new Vector2(-10f, -8f);
                return tmp;
            }
            return null;
        }

        private static string SanitizeName(string label) => label.Replace(" ", "").Replace("/", "");

        private static void CopyLayoutElement(GameObject source, GameObject dest)
        {
            var src = source.GetComponent<LayoutElement>();
            if (src == null)
                return;
            var dst = dest.GetComponent<LayoutElement>() ?? dest.AddComponent<LayoutElement>();
            dst.minHeight = src.minHeight;
            dst.preferredHeight = src.preferredHeight;
            dst.flexibleHeight = src.flexibleHeight;
            dst.minWidth = src.minWidth;
            dst.preferredWidth = src.preferredWidth;
        }
    }
}
