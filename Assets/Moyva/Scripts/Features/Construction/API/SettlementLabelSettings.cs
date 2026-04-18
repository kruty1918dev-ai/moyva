using System;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public sealed class SettlementLabelTextSettings
    {
        [Tooltip("Розмір шрифту TextMesh.")]
        [SerializeField] private int _fontSize = 40;

        [Tooltip("Колір тексту.")]
        [SerializeField] private Color _color = Color.white;

        [Tooltip("Зміщення тексту відносно центру будівлі (локальні координати).")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 1.5f, -0.1f);

        [Tooltip("Якір тексту.")]
        [SerializeField] private TextAnchor _anchor = TextAnchor.LowerCenter;

        [Tooltip("Вирівнювання тексту.")]
        [SerializeField] private TextAlignment _alignment = TextAlignment.Center;

        [Tooltip("Sorting Layer Name для MeshRenderer.")]
        [SerializeField] private string _sortingLayerName = "Default";

        [Tooltip("Sorting Order для MeshRenderer.")]
        [SerializeField] private int _sortingOrder = 100;

        public int FontSize => _fontSize;
        public Color Color => _color;
        public Vector3 Offset => _offset;
        public TextAnchor Anchor => _anchor;
        public TextAlignment Alignment => _alignment;
        public string SortingLayerName => _sortingLayerName;
        public int SortingOrder => _sortingOrder;
    }

    [Serializable]
    public sealed class SettlementLabelSettings
    {
        [Header("Ратуша — налаштування тексту")]
        [SerializeField] private SettlementLabelTextSettings _townHall = new();

        [Header("Замок — налаштування тексту")]
        [SerializeField] private SettlementLabelTextSettings _castle = new();

        public SettlementLabelTextSettings TownHall => _townHall;
        public SettlementLabelTextSettings Castle => _castle;
    }
}
