using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public sealed class BuildingPlacementRules
    {
        [LabelText("Дозволити в тумані")]
        [PropertyTooltip("Що робить: Дозволяє ставити споруду на ще невидимій клітинці.\nВплив у грі: Вимикає стандартну вимогу видимості для цього типу будівлі.")]
        public bool CanPlaceInFog;
        [LabelText("Потребує впливу поселення")]
        [PropertyTooltip("Що робить: Вимагає, щоб споруда була в радіусі ратуші, замку або центру.\nВплив у грі: Обмежує розширення забудови контрольованою територією.")]
        public bool RequiresSettlementInfluence = true;
        [LabelText("Створює вплив поселення")]
        [PropertyTooltip("Що робить: Позначає споруду як джерело зони поселення.\nВплив у грі: Дозволяє розміщувати залежні будівлі навколо неї.")]
        public bool CreatesSettlementInfluence;
        [LabelText("Блокувати біля іншого центру")]
        [PropertyTooltip("Що робить: Забороняє ставити споруду в зоні наявного центру поселення.\nВплив у грі: Запобігає накладанню центральних зон.")]
        public bool BlockIfSettlementCenterInRange;

        [MinValue(0)]
        [LabelText("Радіус впливу")]
        [PropertyTooltip("Що робить: Задає дальність зони, яку створює ця будівля.\nВплив у грі: Більше значення дозволяє ширшу забудову навколо центру.")]
        public int InfluenceRadius;

        [MinValue(0)]
        [LabelText("Мінімум до інших центрів")]
        [PropertyTooltip("Що робить: Задає мінімальну відстань до ратуш, замків та інших центрів.\nВплив у грі: Не дозволяє центральним спорудам стояти надто близько.")]
        public int MinDistanceFromSettlementCenters;

        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = false)]
        [LabelText("Дозволені типи місцевості")]
        [PropertyTooltip("Що робить: Обмежує базову клітинку конкретними terrain ID.\nВплив у грі: Порожній список дозволяє будь-яку придатну місцевість.")]
        public string[] RequiredTerrainIds = Array.Empty<string>();

        [TableList(AlwaysExpanded = false)]
        [LabelText("Обов'язкові сусідні позиції")]
        [PropertyTooltip("Що робить: Задає локальні зміщення клітинок, які повинні існувати біля споруди.\nВплив у грі: Допомагає перевіряти форму берега, дороги або спеціального оточення.")]
        public Vector2Int[] RequiredNeighborOffsets = Array.Empty<Vector2Int>();

        [LabelText("Потрібна вода поруч")]
        [PropertyTooltip("Що робить: Вимагає воду в контрольованій околиці.\nВплив у грі: Використовується для портів, рибалок та водних споруд.")]
        public bool RequiresWaterNearby;
        [LabelText("Потрібен ліс поруч")]
        [PropertyTooltip("Що робить: Вимагає ліс у контрольованій околиці.\nВплив у грі: Обмежує лісові виробництва відповідним біомом.")]
        public bool RequiresForestNearby;
        [LabelText("Потрібні гори поруч")]
        [PropertyTooltip("Що робить: Вимагає гірську місцевість поблизу.\nВплив у грі: Обмежує шахти й гірські споруди.")]
        public bool RequiresMountainNearby;
        [LabelText("Потрібна дорога поруч")]
        [PropertyTooltip("Що робить: Вимагає підключення до дорожньої мережі.\nВплив у грі: Блокує ізольоване розміщення залежних споруд.")]
        public bool RequiresRoadNearby;

        [TableList(AlwaysExpanded = false)]
        [LabelText("Вимоги до тайлів поблизу")]
        [PropertyTooltip("Що робить: Задає точні ID, радіус і кількість потрібних тайлів.\nВплив у грі: Дає гнучкі правила оточення для спеціальних будівель.")]
        public TileRequirementDefinition[] NearbyTileRequirements = Array.Empty<TileRequirementDefinition>();
    }
}
