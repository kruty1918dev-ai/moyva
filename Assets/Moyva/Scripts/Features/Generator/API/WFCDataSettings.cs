using System;
using System.Collections.Generic;
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
        public string TileID;
        [Tooltip("Центральний тайл, для якого застосовується це правило ")]
        public string TileCentralID;

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

        [Header("Polishing Settings")]
        [Tooltip("Скільки ітерацій полірування пройти")]
        public int PassCount = 1;
    }
}