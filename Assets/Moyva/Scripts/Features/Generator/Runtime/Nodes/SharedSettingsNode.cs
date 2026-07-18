using System;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Спільні налаштування", "Допоміжні",
        "Реєструє спільні налаштування генератора (water tile IDs, river base ID, роздільник тощо) " +
        "у контексті графа. Інші ноди автоматично підхоплюють ці значення, уникаючи дублювання.")]
    public sealed class SharedSettingsNode : NodeBase
    {
        [Tooltip("Спільні налаштування генератора. Реєструються у контексті і доступні всім нодам.")]
        [SerializeField] private SharedGeneratorSettingsSO _settings;

        public override string Title => "Спільні налаштування";
        public override string Category => "Допоміжні";

        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();
        public override PortDefinition[] Outputs => Array.Empty<PortDefinition>();

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (_settings == null)
                return NodeOutput.Error("SharedGeneratorSettingsSO not assigned.");

            context.RegisterService<ISharedGeneratorSettings>(_settings);
            return NodeOutput.Success();
        }
    }
}
