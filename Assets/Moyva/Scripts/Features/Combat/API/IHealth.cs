using System;

namespace Kruty1918.Moyva.Combat.API
{
    /// <summary>
    /// Централізований інтерфейс здоров'я для всіх ігрових сутностей (будівлі, юніти тощо).
    ///
    /// Правило сервер-авторитетності:
    ///   Лише серверна логіка може викликати <see cref="TakeDamage"/>, <see cref="Heal"/>, <see cref="Kill"/>.
    ///   Клієнти отримують актуальний стан через систему реплікації і не мають права змінювати HP напряму.
    /// </summary>
    public interface IHealth
    {
        /// <summary>Унікальний ідентифікатор сутності (unitId або buildingInstanceId).</summary>
        string EntityId { get; }

        /// <summary>Поточне значення HP. Завжди в діапазоні [0, MaxHp].</summary>
        int CurrentHp { get; }

        /// <summary>Максимальне значення HP, визначене конфігурацією.</summary>
        int MaxHp { get; }

        /// <summary>True, якщо сутність знищена (CurrentHp == 0).</summary>
        bool IsDestroyed { get; }

        /// <summary>
        /// Подія про зміну HP. Параметри: (entityId, oldHp, newHp, maxHp).
        /// Може використовуватися UI health-барами та логами бою.
        /// </summary>
        event Action<string, int, int, int> OnHealthChanged;

        /// <summary>
        /// Подія знищення сутності. Параметр: entityId.
        /// Викликається одноразово, коли CurrentHp досягає 0.
        /// </summary>
        event Action<string> OnDestroyed;

        /// <summary>
        /// [Тільки сервер] Застосовує шкоду до сутності.
        /// Значення <paramref name="amount"/> має бути > 0.
        /// Якщо сутність вже знищена — виклик ігнорується.
        /// </summary>
        void TakeDamage(int amount);

        /// <summary>
        /// [Тільки сервер] Відновлює HP сутності.
        /// Значення <paramref name="amount"/> має бути > 0.
        /// HP не може перевищити <see cref="MaxHp"/>.
        /// Якщо сутність знищена — виклик ігнорується.
        /// </summary>
        void Heal(int amount);

        /// <summary>
        /// [Тільки сервер] Негайно знищує сутність (встановлює HP = 0).
        /// Гарантує виклик <see cref="OnDestroyed"/>.
        /// </summary>
        void Kill();

        /// <summary>
        /// Ініціалізує компонент здоров'я з конфігураційних даних.
        /// Повинна викликатися один раз під час spawn сутності.
        /// </summary>
        void Initialize(string entityId, int maxHp);
    }
}
