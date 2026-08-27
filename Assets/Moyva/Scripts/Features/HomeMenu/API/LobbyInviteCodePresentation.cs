namespace Kruty1918.Moyva.HomeMenu.API
{
    public readonly struct LobbyInviteCodePresentation
    {
        public readonly string Label;
        public readonly string Code;

        public LobbyInviteCodePresentation(string label, string code)
        {
            Label = string.IsNullOrWhiteSpace(label) ? "Invite Code" : label.Trim();
            Code = code?.Trim() ?? string.Empty;
        }

        public string DisplayText => string.IsNullOrWhiteSpace(Code)
            ? $"{Label}: N/A"
            : $"{Label}: {Code}";
    }
}
