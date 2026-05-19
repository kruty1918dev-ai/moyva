using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    /// <summary>
    /// Опис одного типу тайла у реєстрі.
    /// Містить ідентифікатор, вагу руху та (опційно) візуальний префаб.
    /// </summary>
    [System.Serializable]
    public class TileTypeDefinition
    {
        /// <summary>
        /// Унікальний ідентифікатор типу тайла.
        /// </summary>
        [SerializeField] private string _id;

        /// <summary>
        /// Вартість проходження тайла для логіки руху/патфайндингу.
        /// </summary>
        [SerializeField] private float _movementCost = 1f;

        /// <summary>
        /// Префаб візуального представлення тайла у світі.
        /// </summary>
        [SerializeField] private GameObject _visualPrefab;

        /// <summary>
        /// Публічний доступ до унікального TileId.
        /// </summary>
        public string Id => _id;

        /// <summary>
        /// Публічний доступ до ваги руху.
        /// </summary>
        public float MovementCost => _movementCost;

        /// <summary>
        /// Публічний доступ до префаба візуалізації.
        /// </summary>
        public GameObject VisualPrefab => _visualPrefab;
    }

    /// <summary>
    /// ScriptableObject-реєстр усіх типів тайлів, доступних у проєкті.
    /// Є джерелом даних для сервісів сітки, генерації та візуалізації.
    /// </summary>
    [CreateAssetMenu(fileName = "TileRegistry", menuName = "Moyva/Grid/TileRegistry")]
    public class TileRegistrySO : ScriptableObject
    {
        /// <summary>
        /// Масив визначень тайлів, налаштований у інспекторі Unity.
        /// </summary>
        [SerializeField] private TileTypeDefinition[] _definitions;

        /// <summary>
        /// Публічний доступ до всіх визначень тайлів.
        /// </summary>
        public TileTypeDefinition[] Definitions => _definitions;
    }
}