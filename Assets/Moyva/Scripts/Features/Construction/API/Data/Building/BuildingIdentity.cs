using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public enum BuildingRole
    {
        [InspectorName("Не визначено")]
        None = 0,
        [InspectorName("Житло")]
        Housing = 1,
        [InspectorName("Виробництво")]
        Production = 2,
        [InspectorName("Сховище")]
        Storage = 3,
        [InspectorName("Центр поселення")]
        SettlementCenter = 4,
        [InspectorName("Оборона")]
        Defense = 5,
        [InspectorName("Стіна")]
        Wall = 6,
        [InspectorName("Ворота")]
        Gate = 7,
        [InspectorName("Декорація")]
        Decoration = 8,
        [InspectorName("Підтримка")]
        Support = 9,
    }

    [Serializable]
    public sealed class BuildingIdentity
    {
        [Required]
        [ValidateInput(nameof(HasValidId), "ID будівлі є обов'язковим.")]
        [LabelText("ID будівлі")]
        [PropertyTooltip("Що робить: Задає стабільний технічний ідентифікатор будівлі.\nВплив у грі: За цим ID збереження, меню, мережа й модулі знаходять тип споруди; не змінюйте його після релізу без міграції.")]
        public string Id = "new-building";

        [Required]
        [LabelText("Назва для гравця")]
        [PropertyTooltip("Що робить: Задає локалізовану назву будівлі в інтерфейсі.\nВплив у грі: Відображається в меню будівництва та інформаційних панелях.")]
        public string DisplayName = "Нова будівля";

        [LabelText("Категорія")]
        [PropertyTooltip("Що робить: Групує будівлю в меню та редакторі.\nВплив у грі: Визначає розділ каталогу, але не додає поведінку без модулів.")]
        public BuildingCategory Category = BuildingCategory.Civilian;
        [LabelText("Ігрова роль")]
        [PropertyTooltip("Що робить: Описує основне призначення будівлі для редакторських інструментів.\nВплив у грі: Допомагає автоматичним шаблонам і валідації правильно класифікувати споруду.")]
        public BuildingRole Role = BuildingRole.Support;

        [TextArea(2, 5)]
        [LabelText("Опис")]
        [PropertyTooltip("Що робить: Пояснює призначення будівлі для дизайнера та гравця.\nВплив у грі: Може відображатися в інформаційному UI; на правила розміщення не впливає.")]
        public string Description;

        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = false)]
        [LabelText("Теги")]
        [PropertyTooltip("Що робить: Додає довільні мітки для пошуку й майбутніх правил.\nВплив у грі: Самі по собі теги нічого не змінюють, доки їх не використовує система.")]
        public List<string> Tags = new List<string>();

        private bool HasValidId(string value) => !string.IsNullOrWhiteSpace(value);
    }
}
