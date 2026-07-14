using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public sealed class BuildingValidationIssue
    {
        [LabelText("Рівень")]
        [Tooltip("Що робить: Показує серйозність знайденої проблеми.\nВплив у грі: Помилка блокує коректну конфігурацію, попередження потребує перевірки.")]
        public BuildingValidationSeverity Severity;
        [LabelText("Код")]
        [Tooltip("Що робить: Дає стабільний технічний код правила валідації.\nВплив у грі: Допомагає знайти відповідне правило або автоматичне виправлення.")]
        public string Code;
        [LabelText("Пояснення")]
        [Tooltip("Що робить: Українською пояснює знайдену проблему.\nВплив у грі: Підказує, яке налаштування потрібно виправити.")]
        public string Message;
    }
}
