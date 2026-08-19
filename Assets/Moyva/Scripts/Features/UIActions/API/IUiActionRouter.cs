namespace Kruty1918.Moyva.UIActions.API
{
    public interface IUiActionRouter
    {
        UiActionResult Execute(UiActionRequest request);

        UiActionResult Execute(
            string actionId,
            UiActionSource source = UiActionSource.Programmatic,
            string context = null,
            string targetId = null);
    }
}
