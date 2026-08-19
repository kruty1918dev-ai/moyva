namespace Kruty1918.Moyva.UIActions.API
{
    public interface IUiActionHandler
    {
        bool CanHandle(string actionId);
        UiActionResult Handle(UiActionRequest request);
    }
}
