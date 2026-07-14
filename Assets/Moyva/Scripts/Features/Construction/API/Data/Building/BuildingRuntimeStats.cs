using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public sealed class BuildingRuntimeStats
    {
        [MinValue(1)]
        [LabelText("Максимум міцності")]
        [PropertyTooltip("Що робить: Задає максимальне HP споруди.\nВплив у грі: Визначає, скільки пошкоджень будівля витримає до руйнування.")]
        public int MaxHp = 100;

        [MinValue(0)]
        [LabelText("Броня")]
        [PropertyTooltip("Що робить: Задає базовий захист споруди.\nВплив у грі: Зменшує або модифікує отриману шкоду, якщо бойова система це підтримує.")]
        public int Armor;

        [LabelText("Прапори поведінки")]
        [PropertyTooltip("Що робить: Вмикає базові runtime-властивості споруди.\nВплив у грі: Керує прохідністю, вибором, пошкодженням і потребою завершення.")]
        public BuildingRuntimeFlags Flags = BuildingRuntimeFlags.BlocksPath | BuildingRuntimeFlags.Selectable | BuildingRuntimeFlags.Damageable;

        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = false)]
        [LabelText("Runtime-теги")]
        [PropertyTooltip("Що робить: Додає технічні мітки для runtime-систем.\nВплив у грі: Працюють лише там, де конкретна система читає ці теги.")]
        public List<string> RuntimeTags = new List<string>();
    }
}
