using System.Collections.Generic;
using NOLoader.AviationCareer.ACT.Core;
using NOLoader.AviationCareer.ACT.Recording.TacView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NOLoader.AviationCareer.ACT.UI
{
    public sealed class ACTShellController : MonoBehaviour
    {
        private const float NavW = 0.115f;
        private const float TopH = 0.06f;
        private const float DashH = 0.32f;
        private const float ActionH = 0.05f;
        private const float NavRowH = 0.068f;

        private sealed class NavEntry
        {
            internal int TabIndex;
            internal Image? Accent;
            internal TMP_Text? Label;
            internal Image? Bg;
        }

        private ACTDashboardView? _dashboard;
        private ACTFlightLogUI? _flightLog;
        private IACTViewContext? _context;
        private ACTTacViewPlayer? _tacView;
        private GameObject? _panelRoot;
        private RectTransform? _contentHost;
        private int _activeTabIndex = -1;
        private readonly List<NavEntry> _navEntries = new List<NavEntry>();

        public static ACTShellController? Create(Transform overlayParent, IACTViewContext context, Button? menuEntryTemplate = null)
        {
            if (menuEntryTemplate != null)
                ACTGameUiBridge.MenuButtonTemplate = menuEntryTemplate;

            var root = ACTRect.Create(overlayParent, "ACT_Shell", Vector2.zero, Vector2.one);
            var shell = root.gameObject.AddComponent<ACTShellController>();
            shell.Initialize(context, menuEntryTemplate);
            return shell;
        }

        private void Initialize(IACTViewContext context, Button? menuEntryTemplate)
        {
            _context = context;
            _flightLog = new ACTFlightLogUI();

            if (context.Kind == ACTGameContextKind.MainMenu && menuEntryTemplate != null)
            {
                var workshop = ACTGameUiBridge.FindButtonByLabel(menuEntryTemplate.transform.parent, "WORKSHOP");
                var style = workshop ?? menuEntryTemplate;
                var anchor = workshop ?? menuEntryTemplate;
                ACTGameUiBridge.InsertMenuButtonAfter(anchor, style, "Aviation Career", OpenPanel);
            }
            else
            {
                var r = ACTUiStyles.OverlayEntryRect(context.Kind);
                ACTGameUiBridge.CreateMenuButton(transform, "Aviation Career", OpenPanel, r.min, r.max);
            }

            BuildPanel(context);
            _panelRoot!.SetActive(false);
        }

        private void BuildPanel(IACTViewContext context)
        {
            _panelRoot = ACTRect.Create(transform, "ACT_Panel", Vector2.zero, Vector2.one).gameObject;
            ACTUiFactory.Bg(_panelRoot.transform, ACTDesignTokens.BgDeep, true);

            BuildTopBar(_panelRoot.transform, context);
            BuildNav(_panelRoot.transform, context);
            BuildMain(_panelRoot.transform, context);
            SelectFirstTab();
        }

        private void BuildTopBar(Transform root, IACTViewContext context)
        {
            var bar = ACTRect.Create(root, "TopBar", new Vector2(0, 1f - TopH), Vector2.one);
            ACTUiFactory.PanelFrame(bar, ACTDesignTokens.BgPanel);

            var stripe = ACTUiFactory.MakeImage(bar, ACTDesignTokens.AccentCyan);
            stripe.rectTransform.anchorMin = new Vector2(0, 0);
            stripe.rectTransform.anchorMax = new Vector2(1, 0);
            stripe.rectTransform.sizeDelta = new Vector2(0, 2f);

            var title = ACTUiFactory.TextIn(bar, "AVIATION CAREER TRACKER", ACTDesignTokens.FontTitle, ACTDesignTokens.TextBright,
                TextAlignmentOptions.MidlineLeft, new Vector2(NavW + 0.01f, 0.1f), new Vector2(0.7f, 0.9f));
            title.fontStyle = FontStyles.Bold;

            ACTUiFactory.TextIn(bar, "ESC", ACTDesignTokens.FontSmall, ACTDesignTokens.TextMuted,
                TextAlignmentOptions.MidlineRight, new Vector2(0.86f, 0.15f), new Vector2(0.91f, 0.85f));

            var close = ACTRect.Create(bar, "Close", new Vector2(0.925f, 0.18f), new Vector2(0.98f, 0.82f));
            var closeBg = ACTUiFactory.Bg(close, new Color(0.35f, 0.12f, 0.16f, 1f), true);
            var closeBtn = close.gameObject.AddComponent<Button>();
            closeBtn.targetGraphic = closeBg;
            closeBtn.onClick = new Button.ButtonClickedEvent();
            closeBtn.onClick.AddListener(ClosePanel);
            ACTUiFactory.TextIn(close, "✕", 16f, ACTDesignTokens.TextBright, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
        }

        private void BuildNav(Transform root, IACTViewContext context)
        {
            if (_flightLog == null)
                return;

            _flightLog.Bind(context);
            var tabs = _flightLog.Tabs;

            var rail = ACTRect.Create(root, "Nav", new Vector2(0, 0), new Vector2(NavW, 1f - TopH));
            ACTUiFactory.PanelFrame(rail, ACTDesignTokens.BgPanel);

            ACTUiFactory.TextIn(rail, "FLIGHT LOG", ACTDesignTokens.FontCardLabel, ACTDesignTokens.AccentCyan,
                TextAlignmentOptions.MidlineLeft, new Vector2(0.1f, 0.935f), new Vector2(0.9f, 0.985f));

            int row = 0;
            for (int i = 0; i < tabs.Count; i++)
            {
                if (!tabs[i].IsVisible(context))
                    continue;

                int tabIndex = i;
                float yMax = 0.915f - row * (NavRowH + 0.008f);
                float yMin = yMax - NavRowH;

                var item = ACTRect.Create(rail, "Nav_" + tabs[i].TabId, new Vector2(0.06f, yMin), new Vector2(0.94f, yMax));
                var bg = ACTUiFactory.Bg(item, ACTDesignTokens.NavIdle, true);

                var accent = ACTUiFactory.MakeImage(item, ACTDesignTokens.AccentCyan);
                accent.rectTransform.anchorMin = new Vector2(0, 0.12f);
                accent.rectTransform.anchorMax = new Vector2(0, 0.88f);
                accent.rectTransform.pivot = new Vector2(0, 0.5f);
                accent.rectTransform.sizeDelta = new Vector2(3f, 0);
                accent.enabled = false;

                var txt = ACTUiFactory.TextIn(item, tabs[i].TabButtonLabel.ToUpperInvariant(), ACTDesignTokens.FontNav,
                    ACTDesignTokens.TextMuted, TextAlignmentOptions.MidlineLeft, new Vector2(0.16f, 0.05f), new Vector2(0.98f, 0.95f));

                var btn = item.gameObject.AddComponent<Button>();
                btn.targetGraphic = bg;
                btn.navigation = new Navigation { mode = Navigation.Mode.None };
                btn.onClick = new Button.ButtonClickedEvent();
                btn.onClick.AddListener(() => SelectTab(tabIndex));

                _navEntries.Add(new NavEntry { TabIndex = tabIndex, Accent = accent, Label = txt, Bg = bg });
                row++;
            }
        }

        private void BuildMain(Transform root, IACTViewContext context)
        {
            float left = NavW + 0.01f;
            float bottom = context.ShowDebriefLaunch ? ActionH + 0.014f : 0.014f;
            float top = 1f - TopH - 0.01f;

            if (context.ShowFullDashboard || context.ShowDebriefLaunch)
            {
                var dash = ACTRect.Create(root, "Dashboard", new Vector2(left, top - DashH), new Vector2(0.99f, top));
                _dashboard = dash.gameObject.AddComponent<ACTDashboardView>();
                _dashboard.Build(dash);
                top -= DashH + 0.01f;
            }

            _contentHost = ACTRect.Create(root, "Content", new Vector2(left, bottom), new Vector2(0.99f, top));
            ACTUiFactory.PanelFrame(_contentHost, ACTDesignTokens.BgCard);

            if (context.ShowDebriefLaunch)
            {
                var act = ACTRect.Create(root, "Action", new Vector2(left, 0.014f), new Vector2(0.99f, 0.014f + ActionH));
                var actBg = ACTUiFactory.Bg(act, ACTDesignTokens.NavActive, true);
                var btn = act.gameObject.AddComponent<Button>();
                btn.targetGraphic = actBg;
                btn.onClick = new Button.ButtonClickedEvent();
                btn.onClick.AddListener(() => LaunchTacView(ACTBootstrap.NorepWriter?.CurrentPath));
                ACTUiFactory.TextIn(act, "▶  LAUNCH 3D RECONSTRUCTION", ACTDesignTokens.FontBody, ACTDesignTokens.AccentCyan,
                    TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            }
        }

        private void SelectFirstTab()
        {
            if (_flightLog == null || _context == null)
                return;
            for (int i = 0; i < _flightLog.Tabs.Count; i++)
            {
                if (_flightLog.Tabs[i].IsVisible(_context))
                {
                    SelectTab(i);
                    return;
                }
            }
        }

        private void SelectTab(int index)
        {
            if (_flightLog == null || _context == null || _contentHost == null)
                return;

            _activeTabIndex = index;
            var tabs = _flightLog.Tabs;
            for (int i = 0; i < tabs.Count; i++)
                tabs[i].Unbind();

            if (index < 0 || index >= tabs.Count)
                return;

            tabs[index].Bind(_context);
            ACTTabVisualRenderer.Render(_contentHost, tabs[index].TabId, _context);

            for (int n = 0; n < _navEntries.Count; n++)
            {
                bool active = _navEntries[n].TabIndex == index;
                if (_navEntries[n].Accent != null)
                    _navEntries[n].Accent.enabled = active;
                if (_navEntries[n].Label != null)
                {
                    _navEntries[n].Label.color = active ? ACTDesignTokens.AccentCyan : ACTDesignTokens.TextMuted;
                    _navEntries[n].Label.fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
                }
                if (_navEntries[n].Bg != null)
                    _navEntries[n].Bg.color = active ? ACTDesignTokens.NavActive : ACTDesignTokens.NavIdle;
            }
        }

        private void OpenPanel()
        {
            if (_panelRoot == null)
                return;
            _panelRoot.SetActive(true);
            Refresh();
        }

        private void ClosePanel()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }

        public void Refresh()
        {
            _dashboard?.Refresh();
            if (_activeTabIndex >= 0)
                SelectTab(_activeTabIndex);
        }

        public void LaunchTacView(string? norepPath)
        {
            if (string.IsNullOrEmpty(norepPath))
                norepPath = ACTBootstrap.NorepWriter?.CurrentPath;
            if (string.IsNullOrEmpty(norepPath))
                return;
            _tacView = ACTTacViewPlayer.Launch(norepPath);
        }

        private void OnDestroy()
        {
            if (_flightLog != null)
            {
                foreach (var tab in _flightLog.Tabs)
                    tab.Unbind();
            }
            _tacView?.Shutdown();
        }
    }
}
