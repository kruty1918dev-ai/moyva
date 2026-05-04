using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    public enum Neighborhood8
    {
        Top, TopRight, Right, BottomRight,
        Bottom, BottomLeft, Left, TopLeft
    }

    [Serializable]
    public struct DirectionalConstraint
    {
        public Neighborhood8 Direction;
        [Tooltip("Список допустимих ID сусідів у цьому напрямку")]
        public List<string> AllowedNeighbors;
    }

    [Serializable]
    public struct WFCTileRule
    {
        [Tooltip("ID тайла (результат), який ми хочемо отримати")]
        [TileId] public string TileID;
        [Tooltip("Центральний тайл, для якого застосовується це правило ")]
        [TileId] public string TileCentralID;

        [Tooltip("Обмеження: щоб цей TileID з'явився, ці сусіди ПОВИННІ бути навколо")]
        public List<DirectionalConstraint> Constraints;

        [Tooltip("Пріоритет правила (якщо підходять декілька варіантів)")]
        public int Priority;

        [Tooltip("Якщо правило збігається лише частково (наприклад, 6 з 8 сусідів), чи міняти тайл?")]
        [Range(0.5f, 1f)]
        public float MatchThreshold ;
    }

    [CreateAssetMenu(menuName = "Moyva/Generator/WFCDataSettings", fileName = "WFCDataSettings")]
    public class WFCDataSettings : ScriptableObject
    {
        [Header("Ruleset")]
        public List<WFCTileRule> TileRules;
        
        [Tooltip("Віртуальні ID для правил WFC, які не зобов'язані існувати в TileRegistry. Зручно для прапорців на кшталт flag:road, flag:river, marker:poi.")]
        public string[] VirtualTileIds;

        [Header("Polishing Settings")]
        [Tooltip("Скільки ітерацій полірування пройти")]
        public int PassCount = 1;

        [Header("Near Water Band")]
        [Tooltip("Перед WFC примусово замінює сушу біля води на вибраний Tile ID. Корисно для стабільного берегового краю (наприклад grass біля water).")]
        public bool ForceTileNearWaterBand = false;

        [Tooltip("Tile ID, який буде поставлено у смузі біля води.")]
        [TileId] public string NearWaterTileId = "grass";

        [Tooltip("Радіус смуги біля води в клітинках.")]
        [Range(1, 6)]
        public int NearWaterRadius = 1;

        [Tooltip("Якщо увімкнено, радіус смуги рахується з діагоналями (квадратна/8-напрямна околиця).")]
        public bool IncludeDiagonalsForNearWater = true;

        [Tooltip("Список ID, які вважаються водою для побудови прибережної смуги.")]
        [TileId] public string[] WaterLikeTileIds =
        {
            "water",
            "sea",
            "coast",
            "water-shallow",
            "water-deep",
            "lake",
            "river"
        };

        [Tooltip("Tile ID, яким WFC заповнює зовнішній контур мапи. Для архіпелагів це може бути ocean-deep, для старих графів — water.")]
        [TileId] public string BorderWaterTileId = "water";
    }
}