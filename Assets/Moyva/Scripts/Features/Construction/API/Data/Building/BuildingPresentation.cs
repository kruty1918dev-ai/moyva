using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Presentation.API;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>Варіанти префабів будівлі за фракцією та станом будівництва.</summary>
    [Serializable]
    public sealed class BuildingPresentationVariants
    {
        /// <summary>Префаб стану будівництва.</summary>
        [AssetsOnly]
        [LabelText("Префаб будівництва")]
        [PropertyTooltip("Що робить: Вказує replacement 3D-префаб, який повністю замінює базовий visual, доки будівля ще будується.\nВплив у грі: Якщо будівля не operational, гравець бачить цей prefab замість фінальної моделі.")]
        public GameObject ConstructionPrefab;

        /// <summary>Префаби-варіанти за ключем фракції.</summary>
        [AssetsOnly]
        [LabelText("Варіанти за кольором власника")]
        [PropertyTooltip("Що робить: Мапить ключ палітри власника (blue/green/red/yellow) на варіант префаба будівлі.\nВплив у грі: Поставлена будівля використовує модель у кольорі фракції власника; без запису для ключа використовується базовий префаб.")]
        [SerializeField] public Dictionary<string, GameObject> PrefabVariants = new Dictionary<string, GameObject>();
    }

    /// <summary>Runtime-конфігурація презентації будівлі з варіантами префабів.</summary>
    [Serializable]
    public sealed class BuildingRuntimePresentationConfig : EntityPresentationConfig
    {
        /// <summary>Варіанти префабів будівлі.</summary>
        public BuildingPresentationVariants Variants = new BuildingPresentationVariants();
    }

    /// <summary>Презентаційні дані будівлі: префаб, іконка, тінти, варіанти та preview-налаштування.</summary>
    [Serializable]
    public sealed class BuildingPresentation : EntityPresentationConfig
    {
        /// <summary>Основний префаб будівлі.</summary>
        [AssetsOnly]
        [Required]
        [LabelText("Префаб")]
        [PropertyTooltip("Що робить: Вказує 3D-префаб поставленої будівлі.\nВплив у грі: Саме цей об'єкт створюється після підтвердження будівництва.")]
        public GameObject Prefab;

        /// <summary>Варіанти префабів за фракцією.</summary>
        [InlineProperty]
        [HideLabel]
        public BuildingPresentationVariants Variants = new BuildingPresentationVariants();

        // MOYVA_BUILDING_ICON_PRESENTATION_PASS66
        /// <summary>Іконка будівлі для UI.</summary>
        [AssetsOnly]
        [PreviewField(96, ObjectFieldAlignment.Right)]
        [LabelText("Іконка конструкції")]
        [PropertyTooltip("Що робить: Задає іконку будівлі для дерева Building Designer, меню та панелей.\nВплив у грі: Допомагає швидко розпізнати споруду; якщо Sprite не задано, у дереві редактора іконка не показується.")]
        public Sprite Icon;

        /// <summary>Спрайт попереднього перегляду в runtime.</summary>
        [AssetsOnly]
        [PreviewField(64, ObjectFieldAlignment.Left)]
        [LabelText("Runtime-прев'ю")]
        [PropertyTooltip("Що робить: Зберігає готове зображення будівлі, доступне у білді.\nВплив у грі: Використовується там, де редакторський preview префаба недоступний.")]
        public Sprite RuntimePreview;

        /// <summary>Тінт будівлі в UI.</summary>
        [LabelText("Колір інтерфейсу")]
        [PropertyTooltip("Що робить: Тонує елементи UI цієї будівлі.\nВплив у грі: Змінює лише подання в інтерфейсі, а не матеріал 3D-моделі.")]
        public Color UiTint = Color.white;

        /// <summary>Вертикальний зсув візуала над поверхнею.</summary>
        [LabelText("Вертикальне зміщення")]
        [PropertyTooltip("Що робить: Зсуває preview і поставлену модель уздовж Y.\nВплив у грі: Вирівнює префаб із поверхнею без зміни логічної клітинки.")]
        public float VisualYOffset;

        /// <summary>Налаштування preview будівлі.</summary>
        [InlineProperty]
        [HideLabel]
        [LabelText("Налаштування preview")]
        [PropertyTooltip("Що робить: Налаштовує камеру генерації зображення будівлі.\nВплив у грі: Впливає лише на вигляд preview-іконки.")]
        public BuildingPreviewSettings PreviewSettings = new BuildingPreviewSettings();
    }
}
