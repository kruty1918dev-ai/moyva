namespace Kruty1918.Moyva.SaveSystem
{
    /// <summary>
    /// Контракт для будь-якої системи, що хоче брати участь у циклі збереження/завантаження.
    /// Реалізуйте цей інтерфейс і зареєструйте його у Zenject-контейнері, щоб SaveService
    /// автоматично включив модуль у файл збереження.
    /// </summary>
    public interface ISaveModule
    {
        /// <summary>Серіалізує стан у контекст запису.</summary>
        void OnSave(ISaveContext context);

        /// <summary>Десеріалізує стан із контексту читання.</summary>
        void OnLoad(ISaveContext context);
    }
}
