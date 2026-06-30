using NOLoader.AviationCareer.ACT.Core.Analytics;

namespace NOLoader.AviationCareer.ACT.UI.Tabs
{
    internal abstract class ServiceTabBase : IACTTab
    {
        protected IACTViewContext? Context;

        public abstract string TabId { get; }
        public abstract string TabTitle { get; }
        public virtual string TabButtonLabel => TabTitle;

        public virtual bool IsVisible(IACTViewContext context) => true;

        public void Bind(IACTViewContext context) => Context = context;
        public void Unbind() => Context = null;

        public abstract string GetContent();

        protected T? GetService<T>() where T : class => TabServiceLocator.TryGet<T>();
    }

    internal sealed class FactionLedgerTabView : ServiceTabBase
    {
        public override string TabId => "faction";
        public override string TabTitle => "Faction Ledger";
        public override string TabButtonLabel => "Faction";
        public override string GetContent() => GetService<FactionLedgerService>()?.BuildReport() ?? "—";
    }

    internal sealed class WeaponsAuditTabView : ServiceTabBase
    {
        public override string TabId => "weapons";
        public override string TabTitle => "Weapons Audit";
        public override string TabButtonLabel => "Weapons";
        public override string GetContent() => GetService<WeaponsAuditService>()?.BuildReport() ?? "—";
    }

    internal sealed class FlightPerformanceTabView : ServiceTabBase
    {
        public override string TabId => "flight";
        public override string TabTitle => "Flight Performance";
        public override string TabButtonLabel => "Flight";
        public override bool IsVisible(IACTViewContext context) => context.ShowLiveSession || context.ShowDebriefLaunch || context.ShowFullDashboard;
        public override string GetContent() => GetService<FlightPerformanceService>()?.BuildReport() ?? "—";
    }

    internal sealed class TacticalPatternsTabView : ServiceTabBase
    {
        public override string TabId => "patterns";
        public override string TabTitle => "Tactical Patterns";
        public override string TabButtonLabel => "Patterns";
        public override string GetContent() => GetService<TacticalPatternService>()?.BuildReport() ?? "—";
    }

    internal sealed class BlackBoxTabView : ServiceTabBase
    {
        public override string TabId => "blackbox";
        public override string TabTitle => "Black Box";
        public override string TabButtonLabel => "Black Box";
        public override string GetContent() => GetService<BlackBoxService>()?.BuildReport() ?? "—";
    }

    internal sealed class RecorderTacViewTabView : ServiceTabBase
    {
        public override string TabId => "recorder";
        public override string TabTitle => "Recorder / TacView";
        public override string TabButtonLabel => "Recorder";
        public override bool IsVisible(IACTViewContext context) =>
            context.ShowFullDashboard || context.ShowLiveSession || context.ShowDebriefLaunch;

        public override string GetContent()
        {
            var svc = GetService<RecorderService>();
            if (svc == null)
                return "—";
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(svc.BuildReport());
            if (Context?.ShowDebriefLaunch == true)
                sb.AppendLine("[Launch 3D Reconstruction]");
            return sb.ToString();
        }
    }

    internal static class TabServiceLocator
    {
        private static FactionLedgerService? _ledger;
        private static WeaponsAuditService? _weapons;
        private static FlightPerformanceService? _flight;
        private static TacticalPatternService? _patterns;
        private static BlackBoxService? _blackBox;
        private static RecorderService? _recorder;

        internal static void Register(FactionLedgerService s) => _ledger = s;
        internal static void Register(WeaponsAuditService s) => _weapons = s;
        internal static void Register(FlightPerformanceService s) => _flight = s;
        internal static void Register(TacticalPatternService s) => _patterns = s;
        internal static void Register(BlackBoxService s) => _blackBox = s;
        internal static void Register(RecorderService s) => _recorder = s;

        internal static T? TryGet<T>() where T : class
        {
            if (typeof(T) == typeof(FactionLedgerService)) return _ledger as T;
            if (typeof(T) == typeof(WeaponsAuditService)) return _weapons as T;
            if (typeof(T) == typeof(FlightPerformanceService)) return _flight as T;
            if (typeof(T) == typeof(TacticalPatternService)) return _patterns as T;
            if (typeof(T) == typeof(BlackBoxService)) return _blackBox as T;
            if (typeof(T) == typeof(RecorderService)) return _recorder as T;
            return null;
        }
    }
}
