namespace Kruty1918.Moyva.HomeMenu.API
{
    public interface IConfirmationService
    {
        void Show(ConfirmationRequest request);
        void ForeceHide();
        bool TryGetReqest(out ConfirmationRequest? request);
    }
}