using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public sealed class BuildingPresentation
    {
        [AssetsOnly]
        [Required]
        [LabelText("Префаб")]
        [PropertyTooltip("Що робить: Вказує 3D-префаб поставленої будівлі.\nВплив у грі: Саме цей об'єкт створюється після підтвердження будівництва.")]
        public GameObject Prefab;

        [AssetsOnly]
        [PreviewField(64, ObjectFieldAlignment.Left)]
        [LabelText("Іконка")]
        [PropertyTooltip("Що робить: Задає іконку будівлі для меню та панелей.\nВплив у грі: Допомагає гравцю швидко розпізнати споруду.")]
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
