namespace Kruty1918.Moyva.Shared.Common
{
    /// <summary>
    /// Domain error payload for Result-based control flow.
    /// </summary>
    public readonly struct DomainError
    {
        public static readonly DomainError None = new DomainError(DomainErrorCode.None, string.Empty);

        public DomainErrorCode Code { get; }
        public string Message { get; }

        public DomainError(DomainErrorCode code, string message)
        {
            Code = code;
            Message = message ?? string.Empty;
        }

        public bool IsNone => Code == DomainErrorCode.None;

        public override string ToString()
        {
            return IsNone ? "None" : $"{Code}: {Message}";
        }
    }
}
