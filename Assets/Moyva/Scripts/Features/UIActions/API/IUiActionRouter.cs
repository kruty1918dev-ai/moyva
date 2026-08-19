namespace Kruty1918.Moyva.UIActions.API
{
    public interface IUiActionRouter
    {
        UiActionResult Execute(in UiActionRequest request);

        UiActionResult Execute(
            UiActionId actionId,
            UiActionSource source = UiActionSource.Programmatic,
            string contextId = null,
            string targetId = null);
    }
}
