using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Vfx.API;

namespace Kruty1918.Moyva.Vfx.Runtime
{
    /// <summary>
    /// Frozen lookup "eventName|context" → rule. Context rules win over the base
    /// event rule, same convention as the audio feedback layer.
    /// </summary>
    public sealed class VfxDefinitionRegistry
    {
        private readonly Dictionary<string, VfxEffectRule> _rules =
            new Dictionary<string, VfxEffectRule>(StringComparer.OrdinalIgnoreCase);

        private VfxDefinitionRegistry() { }

        public IReadOnlyCollection<VfxEffectRule> Rules => _rules.Values;

        public static VfxDefinitionRegistry Build(VfxCatalogConfig config)
        {
            var registry = new VfxDefinitionRegistry();
            if (config?.effects == null)
                return registry;

            foreach (VfxEffectRule rule in config.effects)
            {
                if (rule == null || string.IsNullOrWhiteSpace(rule.eventName))
                    continue;
                registry._rules[Key(rule.eventName, rule.context)] = rule;
            }

            return registry;
        }

        public VfxEffectRule Resolve(string eventName, string context = null)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                return null;

            if (!string.IsNullOrWhiteSpace(context)
                && _rules.TryGetValue(Key(eventName, context), out VfxEffectRule contextual))
                return contextual;

            return _rules.TryGetValue(Key(eventName, null), out VfxEffectRule rule)
                ? rule
                : null;
        }

        public bool TryResolve(string eventName, string context, out VfxEffectRule rule)
        {
            rule = Resolve(eventName, context);
            return rule != null;
        }

        private static string Key(string eventName, string context)
            => string.IsNullOrWhiteSpace(context)
                ? eventName.Trim()
                : eventName.Trim() + "|" + context.Trim();
    }
}
