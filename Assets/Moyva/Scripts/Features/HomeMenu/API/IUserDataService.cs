using System;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Сервіс керування користувацькими даними: збереженнями та конфігом.
    /// Використовується меню налаштувань для "видалення даних користувача".
    /// </summary>
    public interface IUserDataService
    {
        /// <summary>Повертає <c>true</c>, якщо в системі існує хоча б один слот збереження або конфіг.</summary>
        bool HasAnyUserData();

        /// <summary>
        /// Видаляє всі слоти збереження та глобальний конфіг.
        /// Повертає кількість видалених артефактів (слоти + конфіг).
        /// Операція атомарна з точки зору користувача: логує помилки але не кидає виключення.
        /// </summary>
        int DeleteAllUserData();

        /// <summary>Подія: всі дані успішно видалені.</summary>
        event Action UserDataDeleted;
    }
}
