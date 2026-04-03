namespace Kruty1918.Moyva.Signals
{
    /// <summary>
    /// Надсилається будь-яким компонентом, що хоче ініціювати збереження у вказаний слот.
    /// </summary>
    public struct SaveRequestedSignal
    {
        public int Slot;
    }

    /// <summary>
    /// Надсилається будь-яким компонентом, що хоче ініціювати завантаження зі вказаного слоту.
    /// </summary>
    public struct LoadRequestedSignal
    {
        public int Slot;
    }

    /// <summary>
    /// Надсилається SaveService після спроби збереження (успішної або ні).
    /// </summary>
    public struct SaveCompletedSignal
    {
        public int    Slot;
        public bool   Success;
        public string ErrorMessage;
    }
}
