using System.Collections.Generic;

namespace Kruty1918.Moyva.Combat.API
{
    /// <summary>
    /// Централізований реєстр усіх активних health-сутностей у грі.
    ///
    /// Призначення:
    ///   - Єдина точка доступу для бойової, damage та targeting логіки.
    ///   - Серверна система атак, облоги, руйнування будівель звертається сюди,
    ///     не залежачи від конкретних типів об'єктів (юніт чи будівля).
    ///
    /// Lifecycle:
    ///   - Реєстрація: викликати <see cref="Register"/> після spawn сутності.
    ///   - Видалення:  викликати <see cref="Unregister"/> перед/під час destroy.
    /// </summary>
    public interface IHealthRegistry
    {
        /// <summary>
        /// Реєструє health-компонент у системі.
        /// Якщо сутність з таким EntityId вже зареєстрована — замінює її.
        /// </summary>
        void Register(IHealth health);

        /// <summary>
        /// Видаляє реєстрацію health-компонента за EntityId.
        /// Якщо сутність не знайдена — виклик ігнорується.
        /// </summary>
        void Unregister(string entityId);

        /// <summary>
        /// Повертає <see cref="IHealth"/> за EntityId.
        /// Повертає null, якщо не знайдено або сутність вже знищена.
        /// </summary>
        IHealth Get(string entityId);

        /// <summary>
        /// Повертає true та health-компонент, якщо сутність знайдена і не знищена.
        /// </summary>
        bool TryGet(string entityId, out IHealth health);

        /// <summary>
        /// Повертає знімок усіх активних (не знищених) health-сутностей.
        /// </summary>
        IReadOnlyCollection<IHealth> GetAll();

        /// <summary>
        /// Повертає знімок усіх активних health-сутностей із заданими ідентифікаторами.
        /// Зручно для групових операцій (наприклад, AOE-шкода).
        /// </summary>
        IReadOnlyCollection<IHealth> GetMany(IEnumerable<string> entityIds);

        /// <summary>Кількість зареєстрованих активних сутностей.</summary>
        int Count { get; }
    }
}
