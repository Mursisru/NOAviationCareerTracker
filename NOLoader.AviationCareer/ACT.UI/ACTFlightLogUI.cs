using System.Collections.Generic;
using NOLoader.AviationCareer.ACT.Core.Analytics;
using NOLoader.AviationCareer.ACT.UI.Tabs;

namespace NOLoader.AviationCareer.ACT.UI
{
    public sealed class ACTFlightLogUI
    {
        private readonly List<IACTTab> _tabs = new List<IACTTab>();
        private IACTViewContext? _context;
        private int _activeIndex;

        public ACTFlightLogUI()
        {
            _tabs.Add(new FactionLedgerTabView());
            _tabs.Add(new WeaponsAuditTabView());
            _tabs.Add(new FlightPerformanceTabView());
            _tabs.Add(new TacticalPatternsTabView());
            _tabs.Add(new BlackBoxTabView());
            _tabs.Add(new RecorderTacViewTabView());
        }

        public IReadOnlyList<IACTTab> Tabs => _tabs;

        public void Bind(IACTViewContext context)
        {
            _context = context;
            for (int i = 0; i < _tabs.Count; i++)
            {
                if (_tabs[i].IsVisible(context))
                {
                    _activeIndex = i;
                    break;
                }
            }
            _tabs[_activeIndex].Bind(context);
        }

        public void Unbind()
        {
            for (int i = 0; i < _tabs.Count; i++)
                _tabs[i].Unbind();
            _context = null;
        }

        public string Render()
        {
            if (_context == null)
                return string.Empty;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("--- ACT Flight Log ---");
            for (int i = 0; i < _tabs.Count; i++)
            {
                if (!_tabs[i].IsVisible(_context))
                    continue;
                var mark = i == _activeIndex ? ">" : " ";
                sb.AppendLine($"{mark} [{_tabs[i].TabTitle}]");
            }
            sb.AppendLine();
            sb.AppendLine(_tabs[_activeIndex].GetContent());
            return sb.ToString();
        }

        public void NextTab()
        {
            if (_context == null)
                return;
            _tabs[_activeIndex].Unbind();
            do
            {
                _activeIndex = (_activeIndex + 1) % _tabs.Count;
            } while (!_tabs[_activeIndex].IsVisible(_context));
            _tabs[_activeIndex].Bind(_context);
        }
    }
}
