namespace Kruty1918.Moyva.HomeMenu.API
{
    public readonly struct CreateRoomPanelPresentation
    {
        public readonly string Title;
        public readonly string SectionTitle;
        public readonly string NextButtonText;

        public CreateRoomPanelPresentation(string title, string sectionTitle, string nextButtonText)
        {
            Title = title ?? string.Empty;
            SectionTitle = sectionTitle ?? string.Empty;
            NextButtonText = nextButtonText ?? string.Empty;
        }
    }
}
