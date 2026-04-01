using UnityEngine;
using Kruty1918.Moyva.Buildings.API;

namespace Kruty1918.Moyva.Buildings.Runtime
{
    /// <summary>
    /// ScriptableObject-реєстр усіх будівель проекту.
    /// Створюється через: Assets → Create → Moyva → Buildings → BuildingRegistry
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingRegistry", menuName = "Moyva/Buildings/BuildingRegistry")]
    public class BuildingRegistrySO : ScriptableObject
    {
        [SerializeField] private BuildingConfig[] _buildings;

        public BuildingConfig[] Buildings => _buildings;

        /// <summary>Повертає конфіг будівлі за ID, або null якщо не знайдено.</summary>
        public BuildingConfig GetById(string id)
        {
            if (_buildings == null) return null;
            foreach (var b in _buildings)
                if (b != null && b.BuildingId == id) return b;
            return null;
        }
    }
}
