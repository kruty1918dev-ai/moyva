using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/HeightMapSettings", fileName = "HeightMapSettings")]
    public class HeightMapSettings : ScriptableObject
    {
        [Tooltip("Набір шарів висоти, які перетворюють числову карту висот у базову карту тайлів. Кожен шар задає інтервал висот і Tile ID, що має використовуватись у цьому інтервалі.")]
        public HeightLayer[] HeightLayers;

        private void OnValidate()
        {
            if (HeightLayers == null) return;
            foreach (var layer in HeightLayers)
                layer.MigrateVariantsIfNeeded();
        }
    }

    /// <summary>Тайл з шансом для зваженого випадкового вибору.</summary>
    [System.Serializable]
    public class WeightedTileEntry
    {
        [Tooltip("ID тайла.")]
        [TileId] public string TileID;

        [Tooltip("Ймовірність вибору цього тайла (0–1). Сума всіх шансів у шарі не має перевищувати 1.")]
        [Range(0f, 1f)] public float Chance;
    }

    [System.Serializable]
    public class HeightLayer
    {
        [Tooltip("Базовий ID тайла для цього інтервалу висоти.")]
        [TileId] public string TileID;

        [Tooltip("Ймовірність вибору базового тайла (0–1).")]
        [Range(0f, 1f)] public float TileIDChance = 1f;

        [Tooltip("Варіанти тайлів з індивідуальними шансами для випадкового вибору.")]
        public WeightedTileEntry[] WeightedVariants;

        // Збережено для міграції зі старого формату — не редагувати вручну.
        [HideInInspector] public string[] VariantTileIDs;

        [Tooltip("Нижня межа висоти для цього шару.")]
        public float MinHeight;

        [Tooltip("Верхня межа висоти для цього шару.")]
        public float MaxHeight;

        /// <summary>Переносить старі VariantTileIDs у WeightedVariants із рівномірним розподілом шансів.</summary>
        internal void MigrateVariantsIfNeeded()
        {
            if (VariantTileIDs == null || VariantTileIDs.Length == 0) return;
            if (WeightedVariants != null && WeightedVariants.Length > 0) return;

            int total = 1 + VariantTileIDs.Length;
            float each = 1f / total;
            TileIDChance = each;

            var migrated = new WeightedTileEntry[VariantTileIDs.Length];
            for (int i = 0; i < VariantTileIDs.Length; i++)
                migrated[i] = new WeightedTileEntry { TileID = VariantTileIDs[i], Chance = each };

            WeightedVariants = migrated;
            VariantTileIDs = null;
        }
    }
}