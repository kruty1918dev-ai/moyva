using System;
using Kruty1918.Moyva.Presentation.API;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public sealed class BuildingPresentationVariants
    {
        [AssetsOnly]
        [LabelText("Префаб будівництва")]
        [PropertyTooltip("Що робить: Вказує replacement 3D-префаб, який повністю замінює базовий visual, доки будівля ще будується.\nВплив у грі: Якщо будівля не operational, гравець бачить цей prefab замість фінальної моделі.")]
        public GameObject ConstructionPrefab;
    }

    [Serializable]
    public sealed class BuildingRuntimePresentationConfig : EntityPresentationConfig
    {
        public BuildingPresentationVariants Variants = new BuildingPresentationVariants();
    }

    [Serializable]
    public sealed class BuildingPresentation : EntityPresentationConfig
    {
        [AssetsOnly]
        [Required]
        [LabelText("Префаб")]
        [PropertyTooltip("Що робить: Вказує 3D-префаб поставленої будівлі.\nВплив у грі: Саме цей об'єкт створюється після підтвердження будівництва.")]
        public GameObject Prefab;

        [InlineProperty]
        [HideLabel]
        public BuildingPresentationVariants Variants = new BuildingPresentationVariants();

        // MOYVA_BUILDING_ICON_PRESENTATION_PASS66
        [AssetsOnly]
        [PreviewField(96, ObjectFieldAlignment.Right)]
        [LabelText("Іконка конструкції")]
        [PropertyTooltip("Що робить: Задає іконку будівлі для дерева Building Designer, меню та панелей.\nВплив у грі: Допомагає швидко розпізнати споруду; якщо Sprite не задано, у дереві редактора іконка не показується.")]
        public Sprite Icon;

        [AssetsOnly]
        [PreviewField(64, ObjectFieldAlignment.Left)]
        [LabelText("Runtime-прев'ю")]
        [PropertyTooltip("Що робить: Зберігає готове зображення будівлі, доступне у білді.\nВплив у грі: Використовується там, де редакторський preview префаба недоступний.")]
        public Sprite RuntimePreview;

        [LabelText("Колір інтерфейсу")]
        [PropertyTooltip("Що робить: Тонує елементи UI цієї будівлі.\nВплив у грі: Змінює лише подання в інтерфейсі, а не матеріал 3D-моделі.")]
        public Color UiTint = Color.white;

        [LabelText("Вертикальне зміщення")]
        [PropertyTooltip("Що робить: Зсуває preview і поставлену модель уздовж Y.\nВплив у грі: Вирівнює префаб із поверхнею без зміни логічної клітинки.")]
        public float VisualYOffset;

        [InlineProperty]
        [HideLabel]
        [LabelText("Налаштування preview")]
        [PropertyTooltip("Що робить: Налаштовує камеру генерації зображення будівлі.\nВплив у грі: Впливає лише на вигляд preview-іконки.")]
        public BuildingPreviewSettings PreviewSettings = new BuildingPreviewSettings();
    }
}
