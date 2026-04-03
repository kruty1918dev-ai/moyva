using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [Serializable]
    public class TerrainObjectRule
    {
        [MapObjectId] public string ObjectId;
        public float MinHeight;
        public float MaxHeight;
        public string BiomeIdFilter;
        [Range(0f, 1f)] public float SpawnChance;
    }

    [CreateAssetMenu(fileName = "MapObjectTerrainConfig", menuName = "Moyva/Generator/MapObjectTerrainConfig")]
    public class MapObjectTerrainConfig : ScriptableObject
    {
        public List<TerrainObjectRule> Rules;
    }
}
