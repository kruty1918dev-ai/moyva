using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public sealed class BuildingConstructionData
    {
        [TableList(AlwaysExpanded = false)]
        [LabelText("Вартість будівництва")]
        [PropertyTooltip("Що робить: Перелічує ресурси та їх кількість для однієї споруди.\nВплив у грі: Ресурси перевіряються в preview і списуються лише після успішного розміщення.")]
        public List<BuildingDefinition.BuildingConstructionCostEntry> Cost = new List<BuildingDefinition.BuildingConstructionCostEntry>();

        [MinValue(0)]
        [LabelText("Ходів будівництва")]
        [PropertyTooltip("Що робить: Задає базову тривалість будівництва в ходах.\nВплив у грі: 0 означає миттєву готовність, якщо runtime використовує поетапне будівництво.")]
        public int BuildTurns = 1;

        [LabelText("Потребує будівельника")]
        [PropertyTooltip("Що робить: Визначає, чи потрібен призначений юніт-будівельник.\nВплив у грі: Може блокувати прогрес без доступного робітника.")]
        public bool RequiresBuilder = true;

        [MinValue(0)]
        [LabelText("Обсяг роботи")]
        [PropertyTooltip("Що робить: Задає загальну кількість будівельної роботи.\nВплив у грі: Більше значення подовжує завершення в системах прогресу.")]
        public int WorkRequired;
    }
}
