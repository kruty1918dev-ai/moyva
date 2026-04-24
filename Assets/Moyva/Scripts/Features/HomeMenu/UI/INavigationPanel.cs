namespace Kruty1918.Moyva.HomeMenu.UI
{
    public interface INavigationPanel
    {
        string MenuName { get; }
        void Open();
        void Close();
    }
}