namespace NOLoader.AviationCareer.ACT.UI
{
    public enum ACTGameContextKind
    {
        MainMenu,
        Briefing,
        Pause,
        Debrief
    }

    public interface IACTViewContext
    {
        ACTGameContextKind Kind { get; }
        bool ShowFullDashboard { get; }
        bool ShowTacViewBrowser { get; }
        bool ShowLiveSession { get; }
        bool ShowDebriefLaunch { get; }
    }

    public sealed class ACTMainMenuContext : IACTViewContext
    {
        public ACTGameContextKind Kind => ACTGameContextKind.MainMenu;
        public bool ShowFullDashboard => true;
        public bool ShowTacViewBrowser => true;
        public bool ShowLiveSession => false;
        public bool ShowDebriefLaunch => false;
    }

    public sealed class ACTBriefingContext : IACTViewContext
    {
        public ACTGameContextKind Kind => ACTGameContextKind.Briefing;
        public bool ShowFullDashboard => false;
        public bool ShowTacViewBrowser => false;
        public bool ShowLiveSession => true;
        public bool ShowDebriefLaunch => false;
    }

    public sealed class ACTPauseContext : IACTViewContext
    {
        public ACTGameContextKind Kind => ACTGameContextKind.Pause;
        public bool ShowFullDashboard => false;
        public bool ShowTacViewBrowser => false;
        public bool ShowLiveSession => true;
        public bool ShowDebriefLaunch => false;
    }

    public sealed class ACTDebriefContext : IACTViewContext
    {
        public ACTGameContextKind Kind => ACTGameContextKind.Debrief;
        public bool ShowFullDashboard => false;
        public bool ShowTacViewBrowser => false;
        public bool ShowLiveSession => false;
        public bool ShowDebriefLaunch => true;
    }
}
