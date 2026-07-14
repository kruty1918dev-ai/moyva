using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public sealed class BuildingPreviewSettings
    {
        [LabelText("Позиція камери")]
        [PropertyTooltip("Що робить: Задає зміщення preview-камери відносно префаба.\nВплив у грі: Визначає ракурс згенерованої іконки.")]
        public Vector3 CameraOffset = new Vector3(4f, 5f, -6f);
        [LabelText("Поворот камери")]
        [PropertyTooltip("Що робить: Задає кути preview-камери.\nВплив у грі: Змінює напрямок, з якого видно будівлю на іконці.")]
        public Vector3 CameraEulerAngles = new Vector3(45f, -35f, 0f);
        [Min(1f)]
        [LabelText("Ортографічний розмір")]
        [PropertyTooltip("Що робить: Керує масштабом ортографічної preview-камери.\nВплив у грі: Більше значення робить будівлю меншою в кадрі.")]
        public float OrthographicSize = 4f;
        [LabelText("Колір фону")]
        [PropertyTooltip("Що робить: Задає фон згенерованого preview.\nВплив у грі: Впливає лише на зображення в інтерфейсі.")]
        public Color BackgroundColor = new Color(0f, 0f, 0f, 0f);
    }
}
