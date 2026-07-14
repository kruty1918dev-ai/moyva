using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public enum BuildingFootprintAnchor
    {
        [InspectorName("Центр")]
        Center = 0,
        [InspectorName("Південно-західний кут")]
        SouthWest = 1,
        [InspectorName("Власна точка")]
        Custom = 2,
    }

    [Serializable]
    public sealed class BuildingFootprint
    {
        [MinValue(1)]
        [LabelText("Розмір")]
        [PropertyTooltip("Що робить: Задає ширину й висоту області будівлі в клітинках.\nВплив у грі: Визначає межі footprint і перевірку виходу за карту.")]
        public Vector2Int Size = Vector2Int.one;

        [LabelText("Точка прив'язки")]
        [PropertyTooltip("Що робить: Визначає, яка частина footprint суміщається з вибраною клітинкою.\nВплив у грі: Змінює фактичні координати всіх зайнятих клітинок.")]
        public BuildingFootprintAnchor Anchor = BuildingFootprintAnchor.Center;

        [ShowIf(nameof(Anchor), BuildingFootprintAnchor.Custom)]
        [LabelText("Власна точка прив'язки")]
        [PropertyTooltip("Що робить: Задає координату anchor всередині footprint вручну.\nВплив у грі: Використовується лише для режиму «Власна точка».")]
        public Vector2Int CustomAnchor;

        [LabelText("Блокує рух")]
        [PropertyTooltip("Що робить: Позначає клітинки споруди непрохідними.\nВплив у грі: Юніти не будуть будувати маршрут крізь footprint.")]
        public bool BlocksMovement = true;
        [LabelText("Блокує будівництво")]
        [PropertyTooltip("Що робить: Забороняє іншим будівлям займати ці клітинки.\nВплив у грі: Запобігає накладанню споруд.")]
        public bool BlocksConstruction = true;
        [LabelText("Потребує рівної землі")]
        [PropertyTooltip("Що робить: Вимагає однаковий рівень рельєфу під усіма зайнятими клітинками.\nВплив у грі: Блокує розміщення на перепадах висоти.")]
        public bool RequiresFlatGround = true;

        [TableList(AlwaysExpanded = false)]
        [LabelText("Зайняті клітинки")]
        [PropertyTooltip("Що робить: Явно перелічує локальні клітинки, які займає споруда.\nВплив у грі: Порожній список означає заповнення всього прямокутника Size.")]
        public Vector2Int[] OccupiedCells = Array.Empty<Vector2Int>();

        [TableList(AlwaysExpanded = false)]
        [LabelText("Входи")]
        [PropertyTooltip("Що робить: Позначає локальні клітинки входу до споруди.\nВплив у грі: Використовується навігацією й майбутньою логікою доступу.")]
        public Vector2Int[] EntranceCells = Array.Empty<Vector2Int>();

        [TableList(AlwaysExpanded = false)]
        [LabelText("З'єднання з дорогами")]
        [PropertyTooltip("Що робить: Позначає точки, де споруда очікує підключення дороги.\nВплив у грі: Використовується транспортними та візуальними системами з'єднань.")]
        public Vector2Int[] RoadConnectionCells = Array.Empty<Vector2Int>();
    }
}
