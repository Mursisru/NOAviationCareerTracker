namespace NOLoader.AviationCareer.ACT.UI
{
    public interface IACTTab
    {
        string TabId { get; }
        string TabTitle { get; }
        string TabButtonLabel { get; }
        bool IsVisible(IACTViewContext context);
        void Bind(IACTViewContext context);
        void Unbind();
        string GetContent();
    }
}
