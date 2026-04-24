namespace Kruty1918.Moyva.HomeMenu.API
{
    public struct ConfirmationRequest
    {
        public string LabelText;
        public string MessageText;
        public System.Action OnConfirm;
        public System.Action OnCancel;
    }
}