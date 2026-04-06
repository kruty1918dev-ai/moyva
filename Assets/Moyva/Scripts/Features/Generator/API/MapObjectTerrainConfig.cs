using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [Serializable]
    public class TerrainObjectRule
    {
        [Tooltip("ID об'єкта, який можна спавнити за цим правилом. Має існувати в реєстрі об'єктів карти, інакше генерація створить порожнє або невалідне посилання.")]
        [MapObjectId] public string ObjectId;
        [Tooltip("Мінімальна висота, на якій правило дозволяє розміщувати об'єкт. Допомагає відсікати низини, береги або високогір'я.")]
        public float MinHeight;
        [Tooltip("Максимальна висота для цього об'єкта. Разом із MinHeight описує висотний діапазон допустимого спавну.")]
        public float MaxHeight;
        [Tooltip("Необов'язковий фільтр за біомом. Якщо заповнено, об'єкт буде з'являтися тільки на клітинках із цим Tile ID біому.")]
        [Kruty1918.Moyva.Grid.API.TileId] public string BiomeIdFilter;
        [Tooltip("Ймовірність спавну об'єкта в допустимій клітинці. 0 означає ніколи, 1 — завжди, якщо всі інші умови виконані.")]
        [Range(0f, 1f)] public float SpawnChance;
    }

    [CreateAssetMenu(fileName = "MapObjectTerrainConfig", menuName = "Moyva/Generator/MapObjectTerrainConfig")]
    public class MapObjectTerrainConfig : ScriptableObject
    {
        [Tooltip("Список правил розміщення об'єктів по місцевості. Кожне правило описує, який об'єкт, на якій висоті та в якому біомі може з'явитися.")]
        public List<TerrainObjectRule> Rules;
    }
}
