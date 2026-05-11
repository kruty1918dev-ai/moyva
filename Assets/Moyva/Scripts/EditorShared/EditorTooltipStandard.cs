using UnityEngine;

namespace Kruty1918.Moyva.Editor.Shared
{
    public static class EditorTooltipStandard
    {
        public static string Build(string whatItDoes, string gameImpact)
        {
            string does = string.IsNullOrWhiteSpace(whatItDoes) ? "Налаштовує параметр." : whatItDoes.Trim();
            string impact = string.IsNullOrWhiteSpace(gameImpact) ? "Впливає на ігрову поведінку і баланс." : gameImpact.Trim();
            return $"Що робить: {does}\nВплив у грі: {impact}";
        }

        public static GUIContent Content(string label, string whatItDoes, string gameImpact)
        {
            return new GUIContent(label, Build(whatItDoes, gameImpact));
        }
    }
}
