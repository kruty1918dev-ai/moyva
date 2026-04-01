using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Buildings.API
{
    [CreateAssetMenu(fileName = "BuildingRegistry", menuName = "Moyva/Buildings/BuildingRegistry")]
    public class BuildingRegistrySO : ScriptableObject
    {
        public List<BuildingConfig> Configs = new List<BuildingConfig>();

        /// <summary>Отримати конфіг за TypeId. Повертає null якщо не знайдено.</summary>
        public BuildingConfig GetConfig(string typeId) =>
            Configs.Find(c => c.TypeId == typeId);

        /// <summary>Отримати всі будівлі вказаної категорії.</summary>
        public List<BuildingConfig> GetByCategory(BuildingCategory category) =>
            Configs.FindAll(c => c.Category == category);
    }
}
