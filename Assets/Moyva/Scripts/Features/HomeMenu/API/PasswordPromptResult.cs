namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Результат запиту пароля у користувача.
    /// </summary>
    public readonly struct PasswordPromptResult
    {
        /// <summary>True, якщо користувач натиснув OK; false — якщо скасував.</summary>
        public bool Confirmed { get; }
        /// <summary>Введений пароль (порожній, якщо <see cref="Confirmed"/> = false).</summary>
        public string Password { get; }

        /// <summary>Створити результат запиту пароля.</summary>
        public PasswordPromptResult(bool confirmed, string password)
        {
            Confirmed = confirmed;
            Password = password ?? string.Empty;
        }

        /// <summary>Результат скасованого запиту пароля.</summary>
        public static PasswordPromptResult Cancelled => new PasswordPromptResult(false, string.Empty);

        /// <summary>Результат успішного підтвердження з введеним паролем.</summary>
        public static PasswordPromptResult Confirm(string password) => new PasswordPromptResult(true, password);
    }
}
